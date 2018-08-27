using System;
using System.Collections.Generic;
using System.Linq;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using DMB0001v4.Model;
using System.Text.RegularExpressions;

namespace DMB0001v4.Skills
{
    /// <summary>
    /// Represents short responses to known queries.
    /// </summary>
    public class Retorts : ISkill
    {
        /// <summary>
        /// Admin mode word used to check beginnign of command.
        /// </summary>
        protected const string COMMAND_WORD = "rzezniksays";

        /// <summary>
        /// Locks files changing - usable in tests.
        /// </summary>
        public static bool ReadOnlyFile { get; set; }

        /// <summary>
        /// Backups retorts file in order not to loose all those retorts.
        /// </summary>
        /// <returns>true means backup was created, false otherwise</returns>
        private static bool BackupRetorts()
        {
            // If file lock is ON, skip making new files.
            if (ReadOnlyFile == true) return true;
            // Prepare catalog for the backup with backup name
            var backupPath = RetortsFullPath
                .Replace("Resources", "Resources\\Backups")
                .Replace(".json", $"_{Now()}.json");
            // Copy current file to backup
            File.Copy(RetortsFullPath, backupPath);
            // Check, if file exists
            return File.Exists(backupPath);
        }

        /// <summary>
        /// Converts given wildcard to regular .* and .
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns>regular wildcard pattern</returns>
        private string RegularPattern(string phrase) 
            => string.Format("^{0}$", Regex.Escape(phrase).Replace("\\*", ".*").Replace("\\?", "."));

        /// <summary>
        /// Checks, if given phrase is a admin mode command.
        /// </summary>
        /// <param name="phrase">given phrase</param>
        /// <returns>true means command, false otherwise</returns>
        private bool IsCommand(string phrase) => Regex.IsMatch(phrase, RegularPattern($"{COMMAND_WORD}*;*"));

        /// <summary>
        /// Processes given request.
        /// </summary>
        /// <param name="given">request</param>
        /// <returns>value means processed, null means 'not my thing'</returns>
        public string Process(string given)
        {
            // Check, if param has content
            if (string.IsNullOrWhiteSpace(given)) return null;
            // Check, if it comes as admin mode command - Regex match to COMMAND_WORD
            if (IsCommand(given)) return Process_asCommand(given);
            // Change to lowercase
            given = given.Trim().ToLower();
            // If a-like retort exists, return it's answer
            return _retorts.FirstOrDefault(r => r.Question == given)?.Answer;
        }

        /// <summary>
        /// Processes given retort as a admin mode command.
        /// </summary>
        /// <param name="given">given command line</param>
        /// <returns>response, if it was command, null otherwise</returns>
        private string Process_asCommand(string given)
        {
            string response = null;
            string key = null;
            // Check, if command has form of add retort
            if (given.StartsWith("rzezniksays addretort;", StringComparison.Ordinal))
            {
                // Process as add retort
                var split = given.Split(";");
                if (split.Length == 3)
                {
                    key = split[1].Trim().ToLower();
                    if (key.Length > 0 && split[2].Trim().Length > 0)
                    {
                        // Check, if retort already exists, else add it
                        response = !Contains(key)
                            ? Add(key, split[2].Trim())
                                ? $"Added new retort {key}."
                                : $"Couldn't add retort {key}."
                            : $"Retort {key} already exists."
;
                    }
                    else
                        response = "One of parameters was empty.";
                }
                else
                    response = "It should follow pattern: rzezniksays addretort;question;answer";
            }
            else if (given.StartsWith("rzezniksays removeretort;", StringComparison.Ordinal))
            {
                // Process as remove retort
                var split = given.Split(";");
                if (split.Length == 2)
                {
                    key = split[1].Trim().ToLower();
                    if (key.Length > 0)
                    {
                        // Check, if retort already with given key exists, then remove it
                        response = Contains(key)
                            ? (Remove(key)
                                ? $"Removed retort {key}."
                                : $"Couldn't remove retort {key}.")
                            : $"Retort with key {key} doesn't exist.";
                    }
                    else
                        response = "One of parameters was empty.";
                }
                else
                    response = "It should follow pattern: rzezniksays addretort;question;answer";
            }
            else if (given.StartsWith("rzezniksays countretorts;", StringComparison.Ordinal))
            {
                // Process as count retorts
                response = $"Counted known retorts: {_retorts.Count}";
            }
            else if (given.StartsWith("rzezniksays listretorts;", StringComparison.Ordinal))
            {
                // Process as list retorts
                StringBuilder stringBuilder = new StringBuilder("Listing Known retorts: ");
                foreach (var retort in _retorts)
                    stringBuilder.AppendLine().Append(retort.AsStackEntry());
                response = stringBuilder.ToString();
            }
            return response;
        }

        /// <summary>
        /// Checks, if a retort with given key (or key-value) exists.
        /// </summary>
        /// <param name="key">request of retort</param>
        /// <param name="value">(optional)response of retort</param>
        /// <returns>true means added, false otherwise</returns>
        private static bool Contains(string key, string value = null) => 
            value == null ?
                _retorts.Any(r => r.Question.Equals(key))
                : _retorts.Any(r => r.Question.Equals(key) && r.Answer.Equals(value));

        /// <summary>
        /// Persists current state of Retorts in file.
        /// </summary>
        /// <returns>true means persisted, false otherwise</returns>
        private static bool Persist()
        {
            // If file lock is ON, skip making new files.
            if (ReadOnlyFile == true) return true;
            // Prepare result var
            bool commit = false;
            // Backup retorts in order not to do something funky with data
            if (BackupRetorts())
            {
                //Clear file of retorts
                File.WriteAllText(RetortsFullPath, string.Empty);
                // Opens file of retorts for edit and add it at the end
                using (var file = File.CreateText(RetortsFullPath))
                {
                    var json = JsonConvert.SerializeObject(_retorts, Formatting.Indented);
                    file.Write(json);
                }
                commit = true;
            }
            else
                Console.WriteLine("Couldn't make a backup of retorts.");
            return commit;
        }

        /// <summary>
        /// Adds new retort to set.
        /// </summary>
        /// <param name="key">request of retort</param>
        /// <param name="value">response of retort</param>
        /// <returns>true means added, false otherwise</returns>
        public static bool Add(string key, string value)
        {
            // Check, if params have content
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) return false;
            // Prepare return falg
            bool result = false;
            // Check previous count to confirm, that an element was added
            if (!Contains(key, value))
            {
                var added = new Retort { Id = _retortsMaxId + 1, Question = key, Answer = value };
                _retorts.Add(added);
                //If persisted, commit, else rollback
                if (Persist())
                {
                    FindMaxRetortsId();
                    result = true;
                }
                else
                    _retorts.Remove(added);
            }
            return result;
        }

        /// <summary>
        /// Adds given retorts to kept set.
        /// </summary>
        /// <param name="items">retorts to add</param>
        /// <returns>true means added, false otherwise</returns>
        internal static bool AddAll(List<Retort> items)
        {
            // Check, if param list has content
            if (items == null || items.Count == 0) return false;
            // Prepare return falg
            bool result = false;
            // Prepare backup list
            var backupList = _retorts;
            // Make list of keys - ommiting the ids
            List<string> keys = items.Select(k => k.Question).ToList();
            // Fix ids in given list of items
            foreach (var item in items)
                item.Id = ++_retortsMaxId;
            // Remove all retors with new keys
            int changed = _retorts.RemoveAll(r => keys.Contains(r.Question));
            if (changed > 0)
            {
                // Add all new keys
                _retorts.AddRange(items);
                // Persist the change
                result = Persist();
                if (result)
                    FindMaxRetortsId();
                else
                    _retorts = backupList;
            }
            return result;
        }

        /// <summary>
        /// Removes pointed retort from set.
        /// </summary>
        /// <param name="key">request of retort</param>
        /// <returns>true means removed, false otherwise</returns>
        public static bool Remove(string key)
        {
            // Check, if param have content
            if (string.IsNullOrWhiteSpace(key)) return false;
            // Prepare return falg
            bool result = false;
            // Prepare backup list
            var backupList = _retorts;
            // Remove retors from list
            int listOfRemoved = _retorts.RemoveAll(r => r.Question.Equals(key));
            if (listOfRemoved > 0)
            {
                // Commit, if worked, rollback otherwise
                if (Persist())
                {
                    FindMaxRetortsId();
                    result = true;
                }
                else
                    _retorts = backupList;
            }
            return result;
        }

        /// <summary>
        /// Removes wanted list of keys from retorts.
        /// </summary>
        /// <param name="keys">retort keys to remove</param>
        /// <returns>true means change, false otherwise</returns>
        public static bool RemoveAll(IList<string> keys)
        {
            // Check, if param list has content
            if (keys == null || keys.Count == 0) return false;
            // Prepare return falg
            bool result = false;
            // Prepare backup list
            var backupList = _retorts;
            _retorts.RemoveAll(r => keys.Contains(r.Question));
            if (Persist())
            {
                FindMaxRetortsId();
                result = true;
            }
            else
                _retorts = backupList;
            return result;
        }

        // --- ---- --- LEAVE EVERYTHING BELOW THIS LINE AS IT IS - IT'S OK --- ----- ---

        /// <summary>
        /// State of currently remembered facts and knowledge.
        /// </summary>
        private BrainState _state;
        /// <summary>
        /// Kept instance of retorts.
        /// </summary>
        private static Retorts _instance;
        /// <summary>
        /// Serves as safety lock in creating instance of singleton.
        /// </summary>
        private static readonly object padlock = new object();

        // TODO MAKE IT RELATIVE TO PROJECT'S DIR
        /// <summary>
        /// Hardcoded path to fast retorts file.
        /// </summary>
        private const string RetortsFullPath = "C:\\vsproj\\DMB0001v4\\DMB0001v4\\DMB0001v4\\Resources\\fast_retorts.json";

        /// <summary>
        /// Retorts as quick responses to questions.
        /// </summary>
        private static List<Retort> _retorts;

        /// <summary>
        /// The highest id of retorts.
        /// </summary>
        private static int _retortsMaxId = -1;

        /// <summary>
        /// Blocked empty constructors - this skills is a snigleton.
        /// </summary>
        private Retorts()
        { }

        /// <summary>
        /// Creates new instance of retorts skill to process given phrase.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        private Retorts(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            _state = conversationStateProvider.GetConversationState<BrainState>(context);
            // Create new DialogUtils to hide logic in sub-methods
            AssureRetorts();
        }

        /// <summary>
        /// Creates new instance of skill.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of retorts</returns>
        public ISkill GetInstance(ITurnContext context, IConversationStateProvider conversationStateProvider)
            => Instance(context, conversationStateProvider);

        /// <summary>
        /// Creates new instance of skill.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of retorts</returns>
        public static Retorts Instance(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            if (_instance == null)
                lock (padlock)
                    if (_instance == null)
                        _instance = new Retorts(context, conversationStateProvider);
            return _instance;
        }

        /// <summary>
        /// Assures presence of retorts by initializing them and reading content from retorts file.
        /// </summary>
        private static void AssureRetorts()
        {
            if (_retorts == null)
            {
                _retorts = new List<Retort>();
                // Read from file retorts
                LoadRetorts();
            }
        }

        /// <summary>
        /// Reads JSON file with retorts.
        /// </summary>
        public static void LoadRetorts()
        {
            // TODO: Change to relative path
            using (var reader = new StreamReader(RetortsFullPath))
            {
                var json = reader.ReadToEnd();
                _retorts = JsonConvert.DeserializeObject<List<Retort>>(json);

                // TODO: Fix the path top search from within the project
                FindMaxRetortsId();
            }
        }

        /// <summary>
        /// Finds the highest id from retorts.
        /// </summary>
        private static void FindMaxRetortsId()
        {
            if (_retorts != null) _retortsMaxId = _retorts.Select(t => t.Id).OrderByDescending(t => t).FirstOrDefault();
        }

        /// <summary>
        /// Returns top id of all retorts.
        /// </summary>
        /// <returns>top id, if there are some retorts, on null or empty returns 0</returns>
        public int RetortsMaxId() => _retortsMaxId;

        /// <summary>
        /// Returns current timestamp.
        /// </summary>
        /// <returns>current timestamp</returns>
        private static string Now() => DateTime.Now.ToString("yyyyMMddHHmmssffff");

        /// <summary>
        /// Returns count of all kept retorts.
        /// </summary>
        /// <returns>count of all kept retorts</returns>
        public static int GetCount() => (_retorts ?? new List<Retort>()).Count;

        /// <summary>
        /// Clears pool of kept instances.
        /// </summary>
        public static void Clear() => (_retorts ?? new List<Retort>()).Clear();

        /// <summary>
        /// Retursn flag, if retorts set is empty.
        /// </summary>
        /// <returns>true means empty, false otherwise</returns>
        public static bool IsEmpty() => GetCount() == 0;

        /// <summary>
        /// Gives short description about the skill, what it does.
        /// </summary>
        /// <returns>short description</returns>
        public string About => "Contains set of quick responses from easily extendable file.";

        /// <summary>
        /// Returns count of kept retorts.
        /// </summary>
        /// <returns>Count of retorts</returns>
        public int Count => _retorts != null ? _retorts.Count : 0;
    }
}
