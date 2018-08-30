using System.Collections.Generic;
using System.Linq;

namespace DMB0001v4.Structures
{
    public class DaDict<T>
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
        
        // Constructors
        
        
        
        // Inner methods
        
        // TODO add
        // TODO addall
        // TODO get
        // TODO getall
        // TODO update
        // TODO updateall
        // TODO remove
        // TODO removeall
        
        /// <summary>
        /// Creates clone of kept list of items.
        /// </summary>
        /// <returns>clone of items</returns>
        public Dictionary<uint,T> DeepCopy()
        {
            return _items.ToDictionary(entry => entry.Key, entry => entry.Value);
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
        /// Clears list of kept items.
        /// </summary>
        public void Clear() => (_items ?? new Dictionary<uint, T>()).Clear();
        
        /// <summary>
        /// Returns current count of items in the list.
        /// </summary>
        public uint Count => (uint)(_items?.Count ?? 0);
    }
}