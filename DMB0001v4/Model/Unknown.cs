namespace DMB0001v4.Model
{
    public class Unknown
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public int Count { get; set; }
        //public DateTime LastMod { get; set; }

        // Returns concatenated form of unknown to list it.
        /// <summary>
        /// Returns concatenated form of unknown to list it.
        /// </summary>
        /// <returns>id) unknown as a line</returns>
        public string AsStackEntry()
        {
            return $"{Id}). {Question} - {Count} times.";// - modified {LastMod}";
        }
    }
}
