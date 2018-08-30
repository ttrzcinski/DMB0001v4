namespace DMB0001v4.Model
{
    // Represents a single retort item from responses tree.
    /// <summary>
    /// Represents a single retort item from responses tree.
    /// </summary>
    public class Retort
    {
        public uint Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }

        // Returns concatenated form of retort to list it.
        /// <summary>
        /// Returns concatenated form of retort to list it.
        /// </summary>
        /// <returns>id) retort line</returns>
        public string AsStackEntry()
        {
            return $"{Id}) {Question}: {Answer}";
        }
    }
}
