namespace DMB0001v4.Model
{
    // Represents a single retort item from responses tree.
    /// <summary>
    /// Represents a single retort item from responses tree.
    /// </summary>
    public class Retort
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }

        /// <summary>
        /// Creates a copy of given retort.
        /// </summary>
        /// <param name="item">given retort</param>
        /*public Retort(Retort item)
        {
            Id = item.Id;
            Question = item.Question;
            Answer = item.Answer;
        }

        public Retort(int id, string question, string answer)
        {
            Id = id;
            Question = question;
            Answer = answer;
        }*/

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
