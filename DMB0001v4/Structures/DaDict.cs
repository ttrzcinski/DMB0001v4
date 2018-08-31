using DMB0001v4.Mind;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DMB0001v4.Structures
{
    public class DaDict<T> where T : Item
    {
        // Variables and properties

        /// <summary>
        /// Kept list of items.
        /// </summary>
        private Dictionary<uint, T> _items;
        /// <summary>
        /// Kept list of items before persisting changes (for rollback).
        /// </summary>
        private Dictionary<uint, T> _stashCopy;

        /// <summary>
        /// Locks files changing - usable in tests.
        /// </summary>
        public static bool ReadOnlyFile { get; set; }
        /// <summary>
        /// File's file path with items.
        /// </summary>
        private string _fileFullPath;
        /// <summary>
        /// File's file path with items.
        /// </summary>
        private string FileFullPath { get => _fileFullPath; set => _fileFullPath = value; }

        /// <summary>
        /// Holds index for this skill.
        /// </summary>
        private static DaIndex _daIndex;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public uint MaxId() => _daIndex.Current;

        // Constructors
        private DaDict() => InitItems();


        // Inner methods

        /// <summary>
        /// Adds item to list with previous check, if a-like item already exists. If it exists, it just updates occurence counter of item.
        /// </summary>
        /// <param name="item">an item to add</param>
        /// <returns>true, if changed, false otherwise</returns>
        public bool Add(T item)
        {
            // Check entry param
            if (item == null) return false;
            // Stash a backup
            StashList();
            // Add fixed item to list
            _items.Add(_daIndex.Next(), item);
            // If persisted, commit, else rollback
            return Persist();
        }

        /// <summary>
        /// Adds list of new items.
        /// </summary>
        /// <param name="items">given items</param>
        /// <returns>true means change, false otherwise</returns>
        public bool AddAll(List<T> items)
        {
            // Check entry params
            if (items == null || items.Count == 0) return false;
            // Stash a backup
            StashList();
            // Add fixed item to list
            foreach (var item in items)
                _items.Add(_daIndex.Next(), item);
            // If persisted, commit, else rollback
            return Persist();
        }

        /// <summary>
        /// Returns an item with given id.
        /// </summary>
        /// <param name="key">given id</param>
        /// <returns>item, if found, null otherwise</returns>
        public T Get(uint key) => _items.TryGetValue(key, out T value) ? value : null;

        /// <summary>
        /// Returns all items with any of given ids. 
        /// </summary>
        /// <param name="keys">given idds</param>
        /// <returns>list with wanted items</returns>
        public List<T> GetAll(List<uint> keys) =>
            (keys != null && keys.Count != 0) ? _items.Where(y => keys.Contains(y.Key)).Select(x => x.Value).ToList() : new List<T>();

        /// <summary>
        /// Updates item with pointed id with given changes.
        /// </summary>
        /// <param name="key">pointed id</param>
        /// <param name="item">given changes</param>
        /// <returns>true means change, false otherwise</returns>
        public bool Update(uint key, T item)
        {
            // Check entry param
            if (item == null || !_items.ContainsKey(key)) return false;
            // Stash a backup
            StashList();
            // Prepare return flag
            _items[key] = item;
            // If changed, commit, else rollback
            return Persist();
        }

        /// <summary>
        /// Updates items matching pointed ids with given changes.
        /// </summary>
        /// <param name="keys">pointed ids</param>
        /// <param name="items">items to change</param>
        /// <returns>true means change, false otherwise</returns>
        public bool UpdateAll(List<uint> keys, List<T> items)
        {
            // Check entry param
            if (items == null || keys == null || items.Count == 0 || keys.Count == 0) return false;
            // Stash a backup
            StashList();
            // Prepare return flag
            int i = 0;
            foreach (var item in _items)
            {
                if (keys.Contains(item.Key))
                {
                    _items[item.Key] = items[i];
                    items.RemoveAt(i);
                }
            }
            // If changed, commit, else rollback
            return Persist();
        }

        /// <summary>
        /// Removes pointed item from set.
        /// </summary>
        /// <param name="key">request of item</param>
        /// <returns>true means removed, false otherwise</returns>
        public bool Remove(uint key)
        {
            // Check, if items has key a-like
            if (!_items.ContainsKey(key)) return false;
            // Prepare backup list
            StashList();
            // Remove those items
            var removal = _items.Remove(key);
            // Commit, if worked, rollback otherwise
            return removal ? Persist() : false;
        }

        /// <summary>
        /// Removes wanted list of keys from items.
        /// </summary>
        /// <param name="keys">item keys to remove</param>
        /// <returns>true means change, false otherwise</returns>
        public bool RemoveAll(IList<uint> keys)
        {
            // Check, if param list has content
            if (keys == null || keys.Count == 0) return false;
            // Prepare backup list
            StashList();
            // Remove those items
            _items = _items.Where(t => (!keys.Contains(t.Key))).ToDictionary(pair => pair.Key, pair => pair.Value);
            // Commit, if worked, rollback otherwise
            return Persist();
        }

        /// <summary>
        /// Assures filepath to default, if not set.
        /// </summary>
        private void AssureFilePath()
        {
            if (string.IsNullOrWhiteSpace(_fileFullPath))
                _fileFullPath = FileUtils.ResourcesCatalog() + $"{this.GetType().Name}.json";// TODO check, if it get right type-name
        }

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
            if (ReadOnlyFile) return true;
            // Assert filepath
            AssureFilePath();
            // Add check, if file exists to make a backup
            if (!FileUtils.AssureFile(_fileFullPath)) return false;
            // Prepare result var
            var commit = false;
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
                // Fix id as it might have changed
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
        /// Backups items file in order not to loose all those items.
        /// </summary>
        /// <returns>true means backup was created, false otherwise</returns>
        public bool Backup()
        {
            // If file lock is ON, skip making new files.
            if (ReadOnlyFile) return true;
            // Assert filepath
            AssureFilePath();
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
        /// Creates clone of kept list of items.
        /// </summary>
        /// <returns>clone of items</returns>
        public Dictionary<uint, T> DeepCopy() => _items.ToDictionary(entry => entry.Key, entry => entry.Value);

        /// <summary>
        /// Makes backup copy of current list of items.
        /// </summary>
        public void StashList() => _stashCopy = DeepCopy();

        /// <summary>
        /// Assures presence of items.
        /// </summary>
        private void InitItems()
        {
            if (_items == null)
                _items = new Dictionary<uint, T>();
            // Fix id after change in items
            FixMaxId();
        }

        /// <summary>
        /// Finds the highest id from items.
        /// </summary>
        public void FixMaxId()
        {
            var topId = (uint) (_items != null ? _items.Keys.Aggregate((l,r) => l > r ? 1 : r) : 0);
            if (topId != 0)
            {
                _daIndex.MarkUseds(_items.Keys.ToList());
            }
            else
            {
                _daIndex.Zero();
            }
        }

        /// <summary>
        /// Clears list of kept items.
        /// </summary>
        public void Clear() => (_items ?? new Dictionary<uint, T>()).Clear();

        /// <summary>
        /// Returns current count of items in the list.
        /// </summary>
        public uint Count => (uint)(_items?.Count ?? 0);
    }
}