using System;
using System.Collections.Generic;
using System.Linq;

namespace DMB0001v4.Skills
{
    public class SkillWithList<T>
    {
        // Variables
        private Dictionary<long, T> _items;
        private long _maxId;

        // TODO Constructors
        // TODO 
        // TODO INIT
        // TODO DEFAULT
        // TODO LOAD

        // TODO MAXID;
        // TODO NEXTID;
        // TODO FIX_MAXID;

        // Commiting change
        // TODO FILE_FULL_PATH
        // TODO PERSIST
        // TODO ROLLBACK
        // TODO BACKUP

        // Crud
        // TODO ADD
        // TODO ADDall

        // TODO GET
        // TODO GETall

        // TODO UPDATE
        // TODO UPDATEall

        // TODO REMOVE
        // TODO REMOVEall

        // Info
        // TODO ABOUT

        // Counts
        // TODO COUNT
        // TODO ISEMPTY
        // TODO EXISTS
        public bool Exists(T item)
        {
            if (_items == null)
            {
                return false;
            }
            else
            {
                return true;//_items.Contains(item);
            }
        }
    }
}
