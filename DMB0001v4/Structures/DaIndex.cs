using System;
using System.Collections.Generic;

namespace DMB0001v4.Structures
{
    /// <summary>
    /// Creates new instance of index sequence for any structure of collection working in Resources or Dataset.
    /// </summary>
    public class DaIndex
    {
        private static uint _currVal = 0;

        private static List<uint> _unusedValues = (_unusedValues == null) ? new List<uint>() : _unusedValues;

        public void Zero() => _currVal = 0;

        public readonly uint Current = _currVal;

        public uint Next() => GrabNext();

        public void ReturnUnused(uint unused)
        {
            if (_unusedValues == null)
                _unusedValues = new List<uint>();
            _unusedValues.Add(unused);
        }

        private uint FromUnused()
        {
            uint result = 0;
            if (_unusedValues != null && _unusedValues.Count > 0)
            {
                result = _unusedValues[0];
                _unusedValues.Remove(result);
            }
            return result;
        }

        private uint GrabNext()
        {
            uint result = FromUnused();
            if (result != 0)
                return result;
            else
                return ++_currVal;
        }

        public void MarkUseds(List<uint> used) => _unusedValues.RemoveAll(x => used.Contains(x));
    }
}
