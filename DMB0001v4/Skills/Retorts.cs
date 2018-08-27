using System;
using System.Collections.Generic;
using System.Linq;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System.IO;
using DMB0001v4.Model;
using System.Globalization;

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
        /// Returns current timestamp.
        /// </summary>
        /// <returns>current timestamp</returns>
        private static string Now()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssffff");
        }

        /// <summary>
        /// Backups retorts file in order not to loose all those retorts.
        /// </summary>
        /// <returns>true means backup was created, false otherwise</returns>
        private static bool BackupRetorts()
        {
            // Prepare name of backup file
            var backupPath = RetortsFullPath.Replace(".json", $"_{Now()}.json");
            // Copy current file to backup
            File.Copy(RetortsFullPath, backupPath);
            // Check, if file exists
            return File.Exists(backupPath);
        }

        /// <summary>
        /// Processes given request.
        /// </summary>
        /// <param name="given">request</param>
        /// <returns>value means processed, null means 'not my thing'</returns>
        public string Process(string given)
        {
            // Check, if param has content
            if (string.IsNullOrWhiteSpace(given)) return null;
            // Check, if it comes as admin mode command
            if (given.StartsWith(COMMAND_WORD, StringComparison.Ordinal) && given.Trim().Length > COMMAND_WORD.Length)
                return Process_asCommand(given);
            // Prepare response
            string response = null;
            // Change to lowercase
            given = given.Trim().ToLower();
            // TODO covert it to lambda expression
            // response = _retorts.Find(item => item.Question.ToLower().Equals(question)).Answer;
            foreach (var retort in _retorts)
                if (retort.Question.ToLower().Equals(given))
                {
                    response = retort.Answer;
                    break;
                }
            return response;
        }

        /// <summary>
        /// Processes given retort as a admin mode command.
        /// </summary>
        /// <param name="given">given command line</param>
        /// <returns>response, if it was command, null otherwise</returns>
        private string Process_asCommand(string given)
        {
            string response = null;
            // Check, if command has form of add retort
            if (given.StartsWith("rzezniksays addretort ;") || given.StartsWith("rzezniksays addretort;"))
            {
                var split = given.Split(";");
                if (split.Length == 3)
                    if (split[1].Trim().Length > 0 && split[2].Trim().Length > 0)
                    {
                        //TODO CHECK, IF RETORT ALREADY EXISTS
                        var result = Add(split[1].Trim().ToLower(), split[2].Trim());
                        response = result
                            ? $"Added new retort {split[1]}."
                            : $"Couldn't add retort {split[1]}.";
                    }
                    else
                        response = "One of parameters was empty.";
                else
                    response = "It should follow pattern: rzezniksays addretort;question;answer";
                // TODO ADD COMMAND TO LIST ALL RETORTS
                // TODO ADD COMMAND TO REMOVE A RETORT
            }
            return response;
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
            // Assures, that NPE wont happen
            AssureRetorts();
            // Prepare return falg
            bool result = false;
            // Check previous count to confirm, that an element was added
            int beforeCount = _retorts.Count;
            if (!ContainsKey(key))
            {
                var added = new Retort { Id = _retortsMaxId + 1, Question = key, Answer = value };
                _retorts.Add(added);
                result = beforeCount < _retorts.Count;
                if (result)
                {
                    // TODO Persist added value in file and in storages - if it works, move it to separate method Persist();
                    // Backup retorts in order not to do something funky
                    if (BackupRetorts())
                    {
                        //Clear file of retorts
                        File.WriteAllText(RetortsFullPath, string.Empty);
                        // Opens file of retorts for edit and add it at the end
                        using (var file = File.CreateText(RetortsFullPath))
                        {
                            var json = JsonConvert.SerializeObject(_retorts, Formatting.Indented);
                            file.Write(json);
                            _retortsMaxId++;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Couldn't make a backup of retorts.");
                    }
                    
                }
            }
            return result;
        }

        /// <summary>
        /// Adds given retorts to kept set.
        /// </summary>
        /// <param name="elements">retorts to add</param>
        /// <returns>true means added, false otherwise</returns>
        public static bool AddAll(IDictionary<string, string> elements)
        {
            // Assures, that NPE wont happen
            AssureRetorts();
            // Prepare return falg
            bool result = false;
            int beforeCount = _retorts.Count;
            foreach (KeyValuePair<string, string> element in elements)
            {
                // Check element before adding it
                if (!string.IsNullOrEmpty(element.Key))
                {
                    var added = new Retort { Id = _retortsMaxId + 1, Question = element.Key, Answer = element.Value };
                    _retorts.Add(added);
                }
            }
            result = beforeCount < _retorts.Count;
            if (result)
            {
                // TODO Persist added value in file and in storages
                result = false;
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
            // Assures, that NPE wont happen
            AssureRetorts();
            // Prepare return falg
            bool result = false;
            // Check element before try to remove
            if (!string.IsNullOrEmpty(key))
            {
                int beforeCount = _retorts.Count;
                result = ContainsKey(key);
                for (int i = 0; i < _retorts.Count; i++)
                {
                    var retort = _retorts[i];
                    if (retort.Question.Equals(key))
                    {
                        result = _retorts.Remove(retort);
                    }
                }
                int afterCount = _retorts.Count;
                result = beforeCount > afterCount;
                if (result)
                {
                    ; // TODO Persist removed value in file and in storages
                }
            }

            return result;
        }

        // TODO ADD REMOVE ALL
        /// <summary>
        /// Removed wanted list of retorts from kept set.
        /// </summary>
        /// <param name="elements">retorts to remove</param>
        /// <returns>true means added, false otherwise</returns>
        public static bool RemoveAll(IDictionary<string, string> elements)
        {
            // Assures, that NPE wont happen
            AssureRetorts();
            // Prepare return falg
            bool result = false;
            int beforeCount = _retorts.Count;
            foreach (KeyValuePair<string, string> element in elements)
            {
                // Check element before try to remove
                if (!string.IsNullOrEmpty(element.Key))
                {
                    bool tempResult = ContainsKey(element.Key);
                    for (int i = 0; i < _retorts.Count; i++)
                    {
                        var retort = _retorts[i];
                        if (retort.Question.Equals(element.Key))
                        {
                           tempResult = _retorts.Remove(retort);
                        }
                    }
                }
            }
            result = beforeCount > _retorts.Count;
            if (result)
            {
                // TODO Persist removed value in file and in storages
                result = false;
            }
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
                var items = JsonConvert.DeserializeObject<List<Retort>>(json);
                _retorts = items;

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
        public int RetortsMaxId()
        {
            return _retortsMaxId;
        }

        /// <summary>
        /// Returns count of all kept retorts.
        /// </summary>
        /// <returns>count of all kept retorts</returns>
        public static int GetCount()
        {
            return (_retorts ?? new List<Retort>()).Count;
        }

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
        /// Checks, if key is present in retorts.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true means presence, false otherwise</returns>
        public static bool ContainsKey(string key)
        {
            var result = false;
            foreach (var retort in _retorts)
                if (retort.Question.Equals(key))
                {
                    result = true;
                    break;
                }
            return result;
        }

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
