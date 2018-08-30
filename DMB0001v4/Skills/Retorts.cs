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
using DMB0001v4.Structures;

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
        private const string COMMAND_WORD = "rzezniksays";

        /// <summary>
        /// Locks files changing - usable in tests.
        /// </summary>
        public static bool ReadOnlyFile { get; set; }

        /// <summary>
        /// Checks, if given phrase is a admin mode command.
        /// </summary>
        /// <param name="phrase">given phrase</param>
        /// <returns>true means command, false otherwise</returns>
        private bool IsCommand(string phrase) => Regex.IsMatch(phrase, RegexUtils.RegularPattern($"{COMMAND_WORD}*;*"));

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
            if (given.StartsWith($"{COMMAND_WORD} addretort;", StringComparison.Ordinal))
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
                    response = "$It should follow pattern: {COMMAND_WORD} addretort;question;answer";
            }
            else if (given.StartsWith($"{COMMAND_WORD} removeretort;", StringComparison.Ordinal))
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
                    response = $"It should follow pattern: {COMMAND_WORD} addretort;question;answer";
            }
            else if (given.StartsWith($"{COMMAND_WORD} countretorts;", StringComparison.Ordinal))
            {
                // Process as count retorts
                response = $"Counted known retorts: {_items.Count}";
            }
            else if (given.StartsWith($"{COMMAND_WORD} listretorts;", StringComparison.Ordinal))
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
        /// Returns all occurrences of items with questions of given item.
        /// </summary>
        /// <param name="item">given item</param>
        /// <returns>list of  occurrences, if exist, empty list otherwise</returns>
        internal List<Retort> Occurrences(Retort item)
            => item == null || string.IsNullOrWhiteSpace(item.Question)
                ? new List<Retort>()
                : _items.Where(k => k.Question.Equals(item.Question, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>
        /// Returns all occurrences of items with given question.
        /// </summary>
        /// <param name="question"></param>
        /// <returns>list of  occurrences, if exist, empty list otherwise</returns>
        public List<Retort> Occurrences(string question)
            => _items.Where(k => k.Question.Equals(question, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>
        /// Returns all occurrences of items with given id.
        /// </summary>
        /// <param name="id">given id</param>
        /// <returns>list of  occurrences, if exist, empty list otherwise</returns>
        public List<Retort> Occurrences(uint id)
            => _items.Where(k => k.Id == id).ToList();

        /// <summary>
        /// Returns all occurrences of items with given ids.
        /// </summary>
        /// <param name="ids">given ids</param>
        /// <returns></returns>
        internal List<Retort> Occurrences(List<uint> ids)
        {
            // Check entry params
            if (ids == null || ids.Count == 0) return new List<Retort>();
            // Find intersection
            return _items.FindAll(item => ids.Contains(item.Id));
        }

        /// <summary>
        /// Returns all occurrences of items with given questions.
        /// </summary>
        /// <param name="questions">given questions</param>
        /// <returns></returns>
        internal List<Retort> Occurrences(List<string> questions)
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
        private bool Contains(string key, string value = null) => 
            value == null ?
                _items.Any(r => r.Question.Equals(key))
                : _items.Any(r => r.Question.Equals(key) && r.Answer.Equals(value));

        
        /// <summary>
        /// Returns an item with given id.
        /// </summary>
        /// <param name="key">given id</param>
        /// <returns>item, if found, null otherwise</returns>
        public Retort Get(uint key) => _items?.Find(r => r.Id == key);

        /// <summary>
        /// Returns all items with any of given ids. 
        /// </summary>
        /// <param name="keys">given idds</param>
        /// <returns>list with wanted items</returns>
        public List<Retort> GetAll(List<uint> keys)
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
        public bool Persist()
        {
            // If file lock is ON, skip making new files.
            if (ReadOnlyFile) return true;
            // Prepare result var
            var commit = false;
            // Assure file path
            AssureResourceFilePath();
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
        
        /// <summary>
        /// Adds new item to list.
        /// </summary>
        /// <param name="item">whole item</param>
        /// <returns>true means added, false otherwise</returns>
        public bool Add(Retort item) => item != null && Add(item.Question, item.Answer);

        /// <summary>
        /// Adds new retort to set.
        /// </summary>
        /// <param name="key">request of retort</param>
        /// <param name="value">response of retort</param>
        /// <returns>true means added, false otherwise</returns>
        public bool Add(string key, string value)
        {
            // Check, if params have content
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) return false;
            // Prepare return flag
            var result = false;
            // Check, if element already exists
            if (Contains(key, value)) return false;
            // Add it as it doesn't exists on the list
            var added = new Retort {Id = _daIndex.Next(), Question = key, Answer = value};
            _items.Add(added);
            //If persisted, commit, else rollback
            if (Persist())
            {
                FixMaxId();
                result = true;
            }
            else
                _items.Remove(added);
            return result;
        }

        /// <summary>
        /// Adds given retorts to kept set.
        /// </summary>
        /// <param name="items">retorts to add</param>
        /// <returns>true means added, false otherwise</returns>
        public bool AddAll(List<Retort> items)
        {
            // Check, if param list has content
            if (items == null || items.Count == 0) return false;
            // Prepare backup list
            var backupList = _items;
            // Make list of keys - omitting the ids
            var keys = items.Select(k => k.Question).ToList();
            // Fix ids in given list of items
            foreach (var item in items)
                item.Id = _daIndex.Next();
            // Remove all retorts with new keys
            if (_items.RemoveAll(r => keys.Contains(r.Question)) == 0) return false;
            // Add all new keys
            _items.AddRange(items);
            // Prepare return flag and Persist the change
            var result = Persist();
            if (result)
                FixMaxId();
            else
                _items = backupList;
            return result;
        }

        /// <summary>
        /// Removes pointed retort from set.
        /// </summary>
        /// <param name="key">item's id</param>
        /// <returns>true means removed, false otherwise</returns>
        public bool Remove(uint key)
        {
            // Check, if param have content
            if (key > 0) return false;
            // Prepare backup list
            var backupList = _items;
            // Remove retorts from list
            if (_items.RemoveAll(r => r.Id == key) == 0) return false;
            // Commit, if worked, rollback otherwise
            var result = Persist();
            if (result)
                FixMaxId();
            else
                _items = backupList;
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
            // Prepare backup list
            var backupList = _items;
            // Remove retorts from list
            if (_items.RemoveAll(r => r.Question.Equals(key)) == 0) return false;
            // Commit, if worked, rollback otherwise
            var result = Persist();
            if (result)
                FixMaxId();
            else
                _items = backupList;
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
            // Prepare backup list
            var backupList = _items;
            _items.RemoveAll(r => keys.Contains(r.Question));
            // Commit, if worked, rollback otherwise
            var result = Persist();
            if (result)
                FixMaxId();
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

        /// <summary>
        /// Hardcoded path to fast retorts file.
        /// </summary>
        private string _fileFullPath;

        /// <summary>
        /// Retorts as quick responses to questions.
        /// </summary>
        private List<Retort> _items;
        /// <summary>
        /// Kept list of items before persisting changes (for rollback).
        /// </summary>
        private List<Retort> _stashCopy;

        /// <summary>
        /// Holds index for this skill.
        /// </summary>
        private static DaIndex _daIndex;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public uint MaxId() => _daIndex.Current;

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
            InitItems();
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
            if (_instance != null) return _instance;
            // Start initializing an instance
            lock (padlock)
                if (_instance == null)
                    _instance = new Retorts(context, conversationStateProvider);
            return _instance;
        }

        /// <summary>
        /// Assures presence of retorts by initializing them and reading content from retorts file.
        /// </summary>
        public void InitItems()
        {
            if (_items != null) return;
            // Start initializing them
            _items = new List<Retort>();
            // Set _maxId to beginning
            _daIndex.Zero();
            // Read from file retorts
            Load();
        }

        /// <summary>
        /// Reads JSON file with retorts.
        /// </summary>
        public void Load()
        {
            // Assure file path
            AssureResourceFilePath();
            // Assure presence of file
            if (!FileUtils.AssureFile(_fileFullPath)) Console.WriteLine($"Couldn't read file {_fileFullPath}.");
            // Start reading the file    
            using (var reader = new StreamReader(_fileFullPath))
            {
                var json = reader.ReadToEnd();
                _items = JsonConvert.DeserializeObject<List<Retort>>(json);
                // Fix indexes after loading entities from file
                FixMaxId();
            }
        }

        /// <summary>
        /// Fills up the file path to resource catalog and default file of retorts.
        /// </summary>
        private void AssureResourceFilePath()
        {
            if (string.IsNullOrWhiteSpace(_fileFullPath))
                _fileFullPath = FileUtils.ResourcesCatalog() + "fast_retorts.json";
        }

        /// <summary>
        /// Backups retorts file in order not to loose all those retorts.
        /// </summary>
        /// <returns>true means backup was created, false otherwise</returns>
        private bool BackupRetorts()
        {
            // Assure file path
            AssureResourceFilePath();
            // If file lock is ON, skip making new files.
            if (ReadOnlyFile) return true;
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
        /// Clears pool of kept instances.
        /// </summary>
        public void Clear()
        {
            if (_items != null)
                _items.Clear();
            else
                InitItems();
        }

        /// <summary>
        /// Makes backup copy of current list of items.
        /// </summary>
        public void StashList() 
            => _stashCopy = _items.Select(item => new Retort { Id = item.Id, Question = item.Question, Answer = item.Answer }).ToList();

        /// <summary>
        /// Updates item with pointed id with given changes.
        /// </summary>
        /// <param name="key">pointed id</param>
        /// <param name="item">given changes</param>
        /// <returns>true means change, false otherwise</returns>
        public bool Update(uint key, Retort item)
        {
            // Check entry param
            if (item == null || string.IsNullOrWhiteSpace(item.Question)) return false;
            // Stash a backup
            StashList();
            // Check if item to update even exists
            if (Occurrences(key).Count == 0) return false;
            // Prepare return flag
            _items.Where(x => x.Id == key).Select(x => x.Question = item.Question).ToList();
            // If changed, commit, else rollback
            return Persist();
        }

        /// <summary>
        /// Removes all items with key or question of given item.
        /// </summary>
        /// <param name="key">given item</param>
        /// <returns>true, if changed, false otherwise</returns>
        public bool Remove(Retort item)
        {
            // Check entry param
            if (item == null) return false;
            // Stash a backup
            StashList();
            // Check if question exists, if element exists and should be updated or it should be added
            var repetitions = Occurrences(item.Question);
            if (repetitions.Count == 0) return false;
            // Remove all repetitions from kept list of items
            _items.RemoveAll(x => x.Id == repetitions[0].Id);
            _items.RemoveAll(x => x.Question == repetitions[0].Question);
            // If persisted, commit, else rollback
            return Persist();
        }

        /// <summary>
        /// Removes all items with given keys.
        /// </summary>
        /// <param name="keys">given list of keys</param>
        /// <returns>true means change, false otherwise</returns>
        public bool RemoveAll(List<uint> keys)
        {
            // Check entry params
            if (keys == null || keys.Count == 0) return false;
            // Stash a backup
            StashList();
            // Check if items with given keys exist
            var repetitions = Occurrences(keys);
            if (repetitions.Count == 0) return false;
            // Remove all repetitions from kept list of items
            foreach (var item in repetitions)
                _items.RemoveAll(x => (x.Id == item.Id || x.Question.ToLower() == item.Question.ToLower()));
            // If persisted, commit, else rollback
            return Persist();
        }

        /// <summary>
        /// Returns count of kept retorts.
        /// </summary>
        /// <returns>Count of retorts</returns>
        public uint Count => (uint)(_items?.Count ?? 0);

        /// <summary>
        /// Returns flag, if unknowns set is empty.
        /// </summary>
        /// <returns>true means empty, false otherwise</returns>
        public bool IsEmpty() => Count == 0;

        /// <summary>
        /// Finds the highest id from items.
        /// </summary>
        public void FixMaxId() => _daIndex.MarkUseds(_items?.Select(t => t.Id).OrderByDescending(t => t).ToList() ?? new List<uint>());

        /// <summary>
        /// Gives short description about the skill, what it does.
        /// </summary>
        /// <returns>short description</returns>
        public string About => "Contains set of quick responses from easily extendable file.";
    }
}
