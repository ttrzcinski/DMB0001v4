using DMB0001v4.Mind;
using DMB0001v4.Model;
using DMB0001v4.Providers;
using DMB0001v4.Structures;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DMB0001v4.Skills
{
    public class Unknowns : ISkillWithList<Unknown>
    {
        /// <summary>
        /// State of currently remembered facts and knowledge.
        /// </summary>
        private BrainState _state;
        /// <summary>
        /// Kept instance of items.
        /// </summary>
        private static Unknowns _instance;
        /// <summary>
        /// Serves as safety lock in creating instance of singleton.
        /// </summary>
        private static readonly object padlock = new object();

        /// <summary>
        /// Kept list of items.
        /// </summary>
        private List<Unknown> _items;
        /// <summary>
        /// Kept list of items before persisting changes (for rollback).
        /// </summary>
        private List<Unknown> _stashCopy;
        /// <summary>
        /// Current top id.
        /// </summary>
        //private int _maxId;
        private DaIndex _daIndex;
        /// <summary>
        /// Returns current top value of ids.
        /// </summary>
        /// <returns>top id</returns>
        public uint MaxId() => _daIndex.Current;

        /// <summary>
        /// File's file path with items.
        /// </summary>
        public const string _fileFullPath = "C:\\vsproj\\DMB0001v4\\DMB0001v4\\DMB0001v4\\Resources\\unknowns.json";

        /// <summary>
        /// Locks files changing - usable in tests.
        /// </summary>
        public static bool ReadOnlyFile { get; set; }

        /// <summary>
        /// Processes given unknown by adding it to lists.
        /// </summary>
        /// <param name="given"></param>
        /// <returns></returns>
        public string Process(string given)
        {
            // Check entry param
            if (string.IsNullOrWhiteSpace(given)) return null;
            // Convert it to lower case
            given = given.ToLower();
            // Try to add it
            return Add(new Unknown {Id = _daIndex.Next(), Question = given, Count = 1})
                ? "I would like to know, how to answer that.."
                : null;
        }

        uint ISkillWithList<Unknown>.Count => (uint)(_items?.Count ?? 0);

        /// <summary>
        /// Adds item to list with previous check, if a-like item already exists. If it exists, it just updates occurance counter of item.
        /// </summary>
        /// <param name="item">an item to add</param>
        /// <returns>true, if changed, false otherwise</returns>
        public bool Add(Unknown item)
        {
            // Check entry param
            if (item == null || string.IsNullOrWhiteSpace(item.Question)) return false;
            // Stash a backup
            StashList();
            // Check if question exists, if element exists and should be updated or it should be added
            List<Unknown> repetitions = Occurances(item);
            if (repetitions.Count > 0)
            {
                // Copy old values
                item = repetitions[0];
                // Remove all repetitions from kept list of items
                _items.RemoveAll(x => x.Id == repetitions[0].Id);
                _items.RemoveAll(x => x.Question == repetitions[0].Question);
                // Update counter
                item.Count = repetitions[0].Count + 1;
            }
            else
            {
                // Fix id, if already occured
                if (item.Id < MaxId()) item.Id = _daIndex.Next();
                // Fix count, as the item is new
                item.Count = 1;
            }
            // Add fixed item to list
            _items.Add(item);
            // If persisted, commit, else rollback
            return Persist();
        }

        /// <summary>
        /// Adds list of new items.
        /// </summary>
        /// <param name="items">given items</param>
        /// <returns>true means change, false otherwise</returns>
        public bool AddAll(List<Unknown> items)
        {
            // Check entry params
            if (items == null || items.Count == 0) return false;
            // Stash a backup
            StashList();
            // Check if question exists, if element exists and should be updated or it should be added
            List<string> questions = items.Select(x => x.Question).ToList();
            if (questions.Count > 0)
            {
                // Copy to list repetitions
                List<Unknown> repetitions = _items.Where(k => questions.Contains(k.Question)).ToList();
                // Increment count for those, which already occured
                foreach (var repetition in repetitions) repetition.Count++;
                // TODO REMOVE ALL ITEMS WITH GIVEN QUESTION TO LIST
                _items.RemoveAll(x => questions.Contains(x.Question));
                // TODO Increment count

            }
            else
            {
                // Fix data inside
                foreach (var item in items)
                {
                    // Ids to following max ids
                    item.Id = _daIndex.Next();
                    // Fix count to 1
                    item.Count = 1;
                }
                // Add all items, as they go
                _items.AddRange(items);
            }
            // If persisted, commit, else rollback
            return Persist();
        }

        /// <summary>
        /// Creates new instance of retorts skill to process given phrase.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        private Unknowns(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            _state = conversationStateProvider.GetConversationState<BrainState>(context);
            // Assures, that list of items is not null
            InitItems();
        }

        /// <summary>
        /// Creates new instance of skill.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of unknowns</returns>
        public ISkill GetInstance(ITurnContext context, IConversationStateProvider conversationStateProvider)
            => Instance(context, conversationStateProvider);

        /// <summary>
        /// Creates new instance of skill.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of unknowns</returns>
        public static Unknowns Instance(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            if (_instance == null)
                lock (padlock)
                    if (_instance == null)
                        _instance = new Unknowns(context, conversationStateProvider);
            return _instance;
        }

        /// <summary>
        /// Inner constructor blocked to keep it a singleton.
        /// </summary>
        private Unknowns() => InitItems();

        /// <summary>
        /// Assures, that items are set and loaded, when needed.
        /// </summary>
        public void InitItems()
        {
            if (_items == null)
            {
                _items = new List<Unknown>();
                // Load content from file to items
                LoadList();
            }
        }

        /// <summary>
        /// Reads JSON file with items.
        /// </summary>
        public void LoadList()
        {
            // Add check, if file exists to make a backup
            if (FileUtils.AssureFile(_fileFullPath))
            {
                // TODO: Change to relative path
                using (var reader = new StreamReader(_fileFullPath))
                {
                    var json = reader.ReadToEnd();
                    _items = JsonConvert.DeserializeObject<List<Unknown>>(json);
                    // TODO: Fix the path top search from within the project
                    FixMaxId();
                }
            }
        }

        /// <summary>
        /// Creates clone of kept list of items.
        /// </summary>
        /// <returns>clone of items</returns>
        public List<Unknown> DeepCopy()
        {
            var stash = new List<Unknown>();
            _items.ForEach((item) =>
            {
                stash.Add(item);
            });
            return stash;
        }

        /// <summary>
        /// Makes backup copy of current list of items.
        /// </summary>
        public void StashList() => _stashCopy = DeepCopy();

        /// <summary>
        /// Reverts local changes in kept list of items.
        /// </summary>
        private void Rollback()
        {
            if (_stashCopy != null)
            {
                _items = _stashCopy;
                _stashCopy = null;
            }
        }

        /// <summary>
        /// Persists current state of items in file.
        /// </summary>
        /// <returns>true means persisted, false otherwise</returns>
        public bool Persist()
        {
            // If file lock is ON, skip making new files.
            if (ReadOnlyFile == true) return true;
            // Prepare result var
            bool commit = false;
            // Add check, if file exists to make a backup
            if (!FileUtils.AssureFile(_fileFullPath)) return false;
            // Backup items in order not to do something funky with data
            if (Backup())
            {
                // Clear file's content
                File.WriteAllText(_fileFullPath, string.Empty);
                // Opens file of items for edit and adds them at the end
                using (var file = File.CreateText(_fileFullPath))
                {
                    var json = JsonConvert.SerializeObject(_items, Formatting.Indented);
                    file.Write(json);
                }
                // Fix id as it might've changed
                FixMaxId();
                // Mark success
                commit = true;
            }
            else
            {
                Console.WriteLine($"Rollback. - Couldn't persist a change in {this.GetType().Name}. ");
                Rollback();
            }

            return commit;
        }

        /// <summary>
        /// Backups retorts file in order not to loose all those items.
        /// </summary>
        /// <returns>true means backup was created, false otherwise</returns>
        public bool Backup()
        {
            // If file lock is ON, skip making new files.
            if (ReadOnlyFile) return true;
            // Check inner param
            if (string.IsNullOrWhiteSpace(_fileFullPath)) return false;
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
        /// Returns all occurances of items with questions of given item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>list of  occurances, if exist, empty list otherwise</returns>
        public List<Unknown> Occurances(Unknown item)
            => item == null || string.IsNullOrWhiteSpace(item.Question)
                ? new List<Unknown>()
                : _items.Where(k => k.Question.Equals(item.Question, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="question"></param>
        /// <returns>list of  occurances, if exist, empty list otherwise</returns>
        public List<Unknown> Occurances(string question)
            => _items.Where(k => k.Question.Equals(question, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>
        /// Returns all occurances of items with given id.
        /// </summary>
        /// <param name="id">given id</param>
        /// <returns>list of  occurances, if exist, empty list otherwise</returns>
        public List<Unknown> Occurances(uint id)
            => _items.Where(k => k.Id == id).ToList();

        /// <summary>
        /// Returns all occurances of items with given ids.
        /// </summary>
        /// <param name="ids">given ids</param>
        /// <returns></returns>
        public List<Unknown> Occurances(List<uint> ids)
        {
            // Check entry params
            if (ids == null || ids.Count == 0) return new List<Unknown>();
            // Find intersection
            return _items.FindAll(item => ids.Contains(item.Id));
        }

        /// <summary>
        /// Returns all occurances of items with given questions.
        /// </summary>
        /// <param name="questions">given questions</param>
        /// <returns></returns>
        public List<Unknown> Occurances(List<string> questions)
        {
            // Check entry params
            if (questions == null || questions.Count == 0) return new List<Unknown>();
            // Fix all given questions to lower case
            List<string> lowers = questions.Select(x => x.ToLower()).ToList();
            // Find intersection
            return _items.FindAll(item => lowers.Contains(item.Question.ToLower()));
        }

        /// <summary>
        /// Checks, if list has item with given key.
        /// </summary>
        /// <param name="key">item's key</param>
        /// <returns>true means it has, false otherwise</returns>
        public bool Contains(string key) => _items.Any(r => r.Id.Equals(key));

        /// <summary>
        /// Returns a single element with given id.
        /// </summary>
        /// <param name="key">an id</param>
        /// <returns>element, if found, null otherwise</returns>
        public Unknown Get(uint key) => _items.Find(x => x.Id == key);

        /// <summary>
        /// Returns a single element with given question.
        /// </summary>
        /// <param name="question">given id</param>
        /// <returns>element, if found, null otherwise</returns>
        public Unknown Get(string question)
            => !string.IsNullOrWhiteSpace(question)
                ? _items.Find(x => x.Question.ToLower().Equals(question.ToLower()))
                : null;

        /// <summary>
        /// Returns a single element with given ids.
        /// </summary>
        /// <param name="keys">given ids</param>
        /// <returns>items, if found. empty list otherwise</returns>
        public List<Unknown> GetAll(List<uint> keys) => keys != null && keys.Count != 0 ? _items.FindAll(r => keys.Contains(r.Id)) : new List<Unknown>();

        /// <summary>
        /// Sorts items by id.
        /// </summary>
        public void SortById() => _items = _items != null ? _items = _items.OrderBy(o => o.Id).ToList() : null;

        /// <summary>
        /// REturns whole set of unknowns as Stack lines for debug through console.
        /// </summary>
        /// <returns>list of stack lines</returns>
        public string AsStackLines()
        {
            if (_items == null || _items.Count == 0)
            {
                return "\t(List of unknowns: Empty)";
            }
            else
            {
                var stringBuilder = new StringBuilder("\t(List of unknowns:)");
                // Sort elements in asc order
                SortById();
                // Add item after item as line
                foreach (var item in _items)
                {
                    stringBuilder.AppendLine().Append("\t").Append(item.AsStackEntry());
                }
                return stringBuilder.ToString();
            }
        }

        /// <summary>
        /// Removes all items with key or question of given item.
        /// </summary>
        /// <param name="item">given item</param>
        /// <returns>true, if changed, false otherwise</returns>
        public bool Remove(Unknown item)
        {
            // Check entry param
            if (item == null) return false;
            // Stash a backup
            StashList();
            // Check if question exists, if element exists and should be updated or it should be added
            List<Unknown> repetitions = Occurances(item);
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
        /// Removes all itmes with given key.
        /// </summary>
        /// <param name="key">given key</param>
        /// <returns>true means change, false otherwise</returns>
        public bool Remove(uint key)
        {
            // Check entry param
            if (key < 1) return false;
            // Stash a backup
            StashList();
            // Check if question exists, if element exists
            List<Unknown> repetitions = Occurances(key);
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
        public bool RemoveAll(List<uint> keys)
        {
            // Check entry params
            if (keys == null || keys.Count == 0) return false;
            // Stash a backup
            StashList();
            // Check if items with given keys exist
            List<Unknown> repetitions = Occurances(keys);
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
        /// Updates every occurance of given id with given item.
        /// </summary>
        /// <param name="key">given id</param>
        /// <param name="item">given item</param>
        /// <returns></returns>
        public bool Update(uint key, Unknown item)
        {
            // Check entry param
            if (item == null || string.IsNullOrWhiteSpace(item.Question)) return false;
            // Stash a backup
            StashList();
            // Prepare return falg
            bool result = false;
            // Check if question exists, if element exists and should be updated or it should be added
            List<Unknown> repetitions = Occurances(key);
            if (repetitions.Count > 0)
            {
                _items.Where(x => x.Id == key).Select(x => x.Question = item.Question).ToList();
                // If persisted, commit, else rollback
                result = Persist();
            }
            return result;
        }

        /// <summary>
        /// Finds the highest id from items.
        /// </summary>
        public void FixMaxId()
        {
            var countedtop = (uint)(_items != null ? _items.Select(t => t.Id).OrderByDescending(t => t).FirstOrDefault() : 0);
            if (countedtop != 0) {
                List<uint> used = _items != null ? _items.Select(t => t.Id).OrderByDescending(t => t).ToList() : new List<uint>();
                _daIndex.MarkUseds(used);
            } else {
                _daIndex.Zero();
            }
        }

        /// <summary>
        /// Clears pool of kept instances.
        /// </summary>
        public void Clear() => (_items ?? new List<Unknown>()).Clear();

        /// <summary>
        /// Returns flag, if unknowns set is empty.
        /// </summary>
        /// <returns>true means empty, false otherwise</returns>
        public bool IsEmpty() {
            return ((_items ?? new List<Unknown>()).Count) == 0;
        }

        /// <summary>
        /// Gives short description about the skill, what it does.
        /// </summary>
        /// <returns>short description</returns>
        public string About => "Represents collection of unknown queries (those unaswered yet).";
    }
}
