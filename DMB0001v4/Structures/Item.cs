using System;

namespace DMB0001v4.Structures
{
    public class Item
    {
        private ulong id { get; }
        private DateTime Created { get; }
        private DateTime Modified { get; set; }
        private bool active { get; set; }
    }
}
