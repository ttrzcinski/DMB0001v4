using System.Collections.Generic;

namespace DMB0001v4.Model
{
    // Represents a single question item from known patterns tree.
    /// <summary>
    /// Represents a single question item from known patterns tree.
    /// </summary>
    internal class Question
    {
        public int Id { get; set; }
        public string Pattern_question { get; set; }
        public List<string> Things_question { get; set; }
        public string Pattern_answer { get; set; }
        public List<string> Things_answer { get; set; }

        /*"Pattern_question": "where is {0}?",
        "Things_question": [ "Thing" ],
        "Pattern_answer": "The {0} is {1}.",
        "Things_answer": [ "Thing", , "Location" ]*/

        // Returns concatenated form of question to list it.
        /// <summary>
        /// Returns concatenated form of question to list it.
        /// </summary>
        /// <returns>id) question line</returns>
        public string AsStackEntry()
        {
            return $"{Id}) {Pattern_question}: with {Things_question} and the answer: {Pattern_answer} with {Things_answer}";
        }
    }
}
