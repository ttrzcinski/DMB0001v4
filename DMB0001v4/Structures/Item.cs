using System;

namespace DMB0001v4.Structures
{
    public class Item
    {
        public ulong Id { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime Modified { get; set; }
        public bool Active { get; set; }
    }
}
