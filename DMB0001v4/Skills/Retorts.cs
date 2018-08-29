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
using DMB0001v4.Mind;

namespace DMB0001v4.Skills
{
    /// <summary>
    /// Represents short responses to known queries.
    /// </summary>
    public class Retorts : ISkillWithList<Retort>
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
            return _items.FirstOrDefault(r => r.Question == given)?.Answer;
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
                response = $"Counted known retorts: {_items.Count}";
            }
            else if (given.StartsWith("rzezniksays listretorts;", StringComparison.Ordinal))
            {
                // Process as list retorts
                StringBuilder stringBuilder = new StringBuilder("Listing Known retorts: ");
                foreach (var retort in _items)
                    stringBuilder.AppendLine().Append(retort.AsStackEntry());
                response = stringBuilder.ToString();
            }
            return response;
        }

        /// <summary>
        /// Returns all occurances of items with questions of given item.
        /// </summary>
        /// <param name="item">given item</param>
        /// <returns>list of  occurances, if exist, empty list otherwise</returns>
        internal List<Retort> Occurances(Retort item)
            => item == null || string.IsNullOrWhiteSpace(item.Question)
                ? new List<Retort>()
                : _items.Where(k => k.Question.Equals(item.Question, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>
        /// Returns all occurances of items with given question.
        /// </summary>
        /// <param name="question"></param>
        /// <returns>list of  occurances, if exist, empty list otherwise</returns>
        internal List<Retort> Occurances(string question)
            => _items.Where(k => k.Question.Equals(question, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>
        /// Returns all occurances of items with given id.
        /// </summary>
        /// <param name="id">given id</param>
        /// <returns>list of  occurances, if exist, empty list otherwise</returns>
        internal List<Retort> Occurances(int id)
            => _items.Where(k => k.Id == id).ToList();

        /// <summary>
        /// Returns all occurances of items with given ids.
        /// </summary>
        /// <param name="ids">given ids</param>
        /// <returns></returns>
        internal List<Retort> Occurances(List<int> ids)
        {
            // Check entry params
            if (ids == null || ids.Count == 0) return new List<Retort>();
            // Find intersection
            return _items.FindAll(item => ids.Contains(item.Id));
        }

        /// <summary>
        /// Returns all occurances of items with given questions.
        /// </summary>
        /// <param name="questions">given questions</param>
        /// <returns></returns>
        internal List<Retort> Occurances(List<string> questions)
        {
            // Check entry params
            if (questions == null || questions.Count == 0) return new List<Retort>();
            // Fix all given questions to lower case
            List<string> lowers = questions.Select(x => x.ToLower()).ToList();
            // Find intersection
            return _items.FindAll(item => lowers.Contains(item.Question.ToLower()));
        }

        /// <summary>
        /// Checks, if a retort with given key (or key-value) exists.
        /// </summary>
        /// <param name="key">request of retort</param>
        /// <param name="value">(optional)response of retort</param>
        /// <returns>true means it has, false otherwise</returns>
        private static bool Contains(string key, string value = null) => 
            value == null ?
                _items.Any(r => r.Question.Equals(key))
                : _items.Any(r => r.Question.Equals(key) && r.Answer.Equals(value));

        internal Retort Get(int key) => _items.Find(r => r.Id == key);

        internal List<Retort> GetAll(List<int> keys)
        {
            // Check entry params
            if (keys == null || keys.Count == 0) return new List<Retort>();
            // Returns all matching retorts
            return _items.FindAll(r => keys.Contains(r.Id));
        }

        /// <summary>
        /// Persists current state of Retorts in file.
        /// </summary>
        /// <returns>true means persisted, false otherwise</returns>
        internal static bool Persist()
        {
            // If file lock is ON, skip making new files.
            if (ReadOnlyFile == true) return true;
            // Prepare result var
            bool commit = false;
            // Backup retorts in order not to do something funky with data
            if (BackupRetorts())
            {
                //Clear file of retorts
                File.WriteAllText(_fileFullPath, string.Empty);
                // Opens file of retorts for edit and add it at the end
                using (var file = File.CreateText(_fileFullPath))
                {
                    var json = JsonConvert.SerializeObject(_items, Formatting.Indented);
                    file.Write(json);
                }
                commit = true;
            }
            else
                Console.WriteLine("Couldn't make a backup of retorts.");
            return commit;
        }

        //internal bool Add(Retort item) => Add(item.Question, item.Answer);

        //bool resultOfAdd = retorts.Add("hi", "hello");
        //bool removal = retorts.Remove("hi");

        /// <summary>
        /// Adds new retort to set.
        /// </summary>
        /// <param name="key">request of retort</param>
        /// <param name="value">response of retort</param>
        /// <returns>true means added, false otherwise</returns>
        private static bool Add(string key, string value, bool nonsense)
        {
            // Check, if params have content
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) return false;
            // Prepare return falg
            bool result = false;
            // Check previous count to confirm, that an element was added
            if (!Contains(key, value))
            {
                var added = new Retort();
                added.Id = _maxId + 1;
                added.Question = key;
                added.Answer = value;
                _items.Add(added);
                //If persisted, commit, else rollback
                if (Persist())
                {
                    FixMaxId(false);
                    result = true;
                }
                else
                    _items.Remove(added);
            }
            return result;
        }

        /// <summary>
        /// Adds new retort to set.
        /// </summary>
        /// <param name="key">request of retort</param>
        /// <param name="value">response of retort</param>
        /// <returns>true means added, false otherwise</returns>
        public bool Add(string key, string value) => Add(key, value, false);

        /// <summary>
        /// Adds given retorts to kept set.
        /// </summary>
        /// <param name="items">retorts to add</param>
        /// <returns>true means added, false otherwise</returns>
        internal bool AddAll(List<Retort> items)
        {
            // Check, if param list has content
            if (items == null || items.Count == 0) return false;
            // Prepare return falg
            bool result = false;
            // Prepare backup list
            var backupList = _items;
            // Make list of keys - ommiting the ids
            List<string> keys = items.Select(k => k.Question).ToList();
            // Fix ids in given list of items
            foreach (var item in items)
                item.Id = ++_maxId;
            // Remove all retors with new keys
            int changed = _items.RemoveAll(r => keys.Contains(r.Question));
            if (changed > 0)
            {
                // Add all new keys
                _items.AddRange(items);
                // Persist the change
                result = Persist();
                if (result)
                    FixMaxId();
                else
                    _items = backupList;
            }
            return result;
        }

        /// <summary>
        /// Removes pointed retort from set.
        /// </summary>
        /// <param name="key">request of retort</param>
        /// <returns>true means removed, false otherwise</returns>
        public bool Remove(string key)
        {
            // Check, if param have content
            if (string.IsNullOrWhiteSpace(key)) return false;
            // Prepare return falg
            bool result = false;
            // Prepare backup list
            var backupList = _items;
            // Remove retors from list
            int listOfRemoved = _items.RemoveAll(r => r.Question.Equals(key));
            if (listOfRemoved > 0)
            {
                // Commit, if worked, rollback otherwise
                if (Persist())
                {
                    FixMaxId();
                    result = true;
                }
                else
                    _items = backupList;
            }
            return result;
        }

        /// <summary>
        /// Removes wanted list of keys from retorts.
        /// </summary>
        /// <param name="keys">retort keys to remove</param>
        /// <returns>true means change, false otherwise</returns>
        public bool RemoveAll(IList<string> keys)
        {
            // Check, if param list has content
            if (keys == null || keys.Count == 0) return false;
            // Prepare return falg
            bool result = false;
            // Prepare backup list
            var backupList = _items;
            _items.RemoveAll(r => keys.Contains(r.Question));
            if (Persist())
            {
                FixMaxId();
                result = true;
            }
            else
                _items = backupList;
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
        private const string _fileFullPath = "C:\\vsproj\\DMB0001v4\\DMB0001v4\\DMB0001v4\\Resources\\fast_retorts.json";

        /// <summary>
        /// Retorts as quick responses to questions.
        /// </summary>
        private static List<Retort> _items;
        /// <summary>
        /// Kept list of items before persisting changes (for rollback).
        /// </summary>
        private List<Retort> _stashCopy;

        /// <summary>
        /// The highest id of retorts.
        /// </summary>
        private static int _maxId = 0;

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
            initItems();
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
        public void initItems()
        {
            if (_items == null)
            {
                _items = new List<Retort>();
                // Read from file retorts
                Load();
            }
        }

        /// <summary>
        /// Reads JSON file with retorts.
        /// </summary>
        public void Load()
        {
            // Assure presence of file
            if (FileUtils.AssureFile(_fileFullPath))
            {
                // TODO: Change to relative path
                using (var reader = new StreamReader(_fileFullPath))
                {
                    var json = reader.ReadToEnd();
                    _items = JsonConvert.DeserializeObject<List<Retort>>(json);
                    // TODO: Fix the path top search from within the project
                    FixMaxId();
                }
            }
            else
                Console.WriteLine($"Couldn't read file {_fileFullPath}.");
        }

        /// <summary>
        /// Backups retorts file in order not to loose all those retorts.
        /// </summary>
        /// <returns>true means backup was created, false otherwise</returns>
        private static bool BackupRetorts()
        {
            // If file lock is ON, skip making new files.
            if (ReadOnlyFile == true) return true;
            // Prepare catalog for the backup with backup name
            var backupPath = _fileFullPath
                .Replace("Resources", "Resources\\Backups")
                .Replace(".json", $"_{TimeUtils.Now()}.json");
            // Copy current file to backup
            File.Copy(_fileFullPath, backupPath);
            // Check, if file exists
            return File.Exists(backupPath);
        }

        /// <summary>
        /// Returns count of all kept retorts.
        /// </summary>
        /// <returns>count of all kept retorts</returns>
        public int GetCount() => _items != null ? _items.Count : 0;

        /// <summary>
        /// Clears pool of kept instances.
        /// </summary>
        public void Clear()
        {
            if (_items != null)
                _items.Clear();
        }

        /// <summary>
        /// Makes backup copy of current list of items.
        /// </summary>
        public void StashList() 
            => _stashCopy = _items.Select(item => new Retort { Id = item.Id, Question = item.Question, Answer = item.Answer }).ToList();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal bool Update(int key, Retort item)
        {
            // Check entry param
            if (item == null || string.IsNullOrWhiteSpace(item.Question)) return false;
            // Stash a backup
            StashList();
            // Prepare return falg
            bool result = false;
            // Check if question exists, if element exists and should be updated or it should be added
            List<Retort> repetitions = Occurances(key);
            if (repetitions.Count > 0)
            {
                _items.Where(x => x.Id == key).Select(x => x.Question = item.Question).ToList();
                // If persisted, commit, else rollback
                result = Persist();
            }
            return result;
        }

        /// <summary>
        /// Removes all items with key or question of given item.
        /// </summary>
        /// <param name="key">given item</param>
        /// <returns>true, if changed, false otherwise</returns>
        internal bool Remove(Retort item)
        {
            // Check entry param
            if (item == null) return false;
            // Stash a backup
            StashList();
            // Check if question exists, if element exists and should be updated or it should be added
            List<Retort> repetitions = Occurances(item);
            if (repetitions.Count > 0)
            {
                // Remove all repetitions from kept list of items
                _items.RemoveAll(x => x.Id == repetitions[0].Id);
                _items.RemoveAll(x => x.Question == repetitions[0].Question);
            }
            // If persisted, commit, else rollback
            return Persist();
        }

        /// <summary>
        /// Removes all items with given keys.
        /// </summary>
        /// <param name="keys">given list of keys</param>
        /// <returns>true means change, false otherwise</returns>
        public bool RemoveAll(List<int> keys)
        {
            // Check entry params
            if (keys == null || keys.Count == 0) return false;
            // Stash a backup
            StashList();
            // Check if items with given keys exist
            List<Retort> repetitions = Occurances(keys);
            if (repetitions.Count > 0)
            {
                // Remove all repetitions from kept list of items
                foreach (var item in repetitions)
                {
                    _items.RemoveAll(x => (x.Id == item.Id || x.Question.ToLower() == item.Question.ToLower()));
                }
            }
            // If persisted, commit, else rollback
            return Persist();
        }

        /// <summary>
        /// Returns count of kept retorts.
        /// </summary>
        /// <returns>Count of retorts</returns>
        public int Count => _items != null ? _items.Count : 0;

        /// <summary>
        /// Returns flag, if unknowns set is empty.
        /// </summary>
        /// <returns>true means empty, false otherwise</returns>
        public bool IsEmpty() => Count == 0;

        /// <summary>
        /// Returns current top value of ids.
        /// </summary>
        /// <returns>top id</returns>
        public int MaxId() => _maxId;

        bool ISkillWithList<Retort>.Persist() => Persist();

        bool ISkillWithList<Retort>.Add(Retort item) => Add(item.Question, item.Answer);

        bool ISkillWithList<Retort>.AddAll(List<Retort> items) => AddAll(items);

        Retort ISkillWithList<Retort>.Get(int key) => Get(key);

        List<Retort> ISkillWithList<Retort>.GetAll(List<int> keys) => GetAll(keys);

        bool ISkillWithList<Retort>.Update(int key, Retort item) => this.Update(key, item);

        public bool Remove(int key) => Remove(key);

        public void FixMaxId()
        {
           FixMaxId(false);
        }

        /// <summary>
        /// Finds the highest id from items.
        /// </summary>
        internal static void FixMaxId(bool nonsense) => _maxId = _items != null ? _items.Select(t => t.Id).OrderByDescending(t => t).FirstOrDefault() : 1;

        int ISkillWithList<Retort>.Count => Count;

        /// <summary>
        /// Gives short description about the skill, what it does.
        /// </summary>
        /// <returns>short description</returns>
        public string About => "Contains set of quick responses from easily extendable file.";
    }
}
