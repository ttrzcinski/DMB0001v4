using DMB0001v4.Mind;

namespace DMB0001v4
{
    /// <summary>
    /// Class for storing conversation state as facts, notes and flags marking f.e. "This questions was already asked.". 
    /// </summary>
    public class BrainState
    {
        public string BotsName { get; set; } = "DMB";

        public int TurnCount { get; set; } = 0;

        public string UsersName { get; set; } = "Talker";

        public Question RisenQuestion { get; set; }

        public bool LikesPancakes { get; set; }

        public bool SaidHi { get; set; }

        public bool SaidByeAfter { get; set; }

        // TODO CONVERT THIS TO READ ON INIT FROM JSON FILE
    }
}
