using System.Linq;
using DMB0001v4.Mind;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;

namespace DMB0001v4.Skills
{
    /// <summary>
    /// Skill of processing greetings and valedictions.
    /// </summary>
    public sealed class Greetings : ISkill
    {
        /// <summary>
        /// State of currently remembered facts and knowledge.
        /// </summary>
        private BrainState _state;
        /// <summary>
        /// Kept utils to call right method of Greeting or Valediction.
        /// </summary>
        private static DialogUtils _dialogUtils;
        /// <summary>
        /// Kept instance of skill.
        /// </summary>
        private static Greetings _instance;
        /// <summary>
        /// Serves as safety lock in creating instance of singleton.
        /// </summary>
        private static readonly object padlock = new object();

        /// <summary>
        /// Bloecked empty constructors - this skills is a snigleton.
        /// </summary>
        private Greetings()
        { }

        /// <summary>
        /// Creates new instance of greetings skill to process given phrase.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        private Greetings(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            _state = conversationStateProvider.GetConversationState<BrainState>(context);
            // Create new DialogUtils to hide logic in sub-methods
            if (_dialogUtils == null) {
                _dialogUtils = new DialogUtils(context, conversationStateProvider);
            }
        }

        /// <summary>
        /// Creates new instance of skill.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of greeting</returns>
        public ISkill GetInstance(ITurnContext context, IConversationStateProvider conversationStateProvider) 
            => Instance(context, conversationStateProvider);

        /// <summary>
        /// Creates new instance of skill.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of greeting</returns>
        public static Greetings Instance(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            if (_instance == null)
                lock (padlock)
                    if (_instance == null)
                        _instance = new Greetings(context, conversationStateProvider);
            return _instance;
        }

        /// <summary>
        /// Processes given phrase, as if it will be qualified as greeting or valediction, it will be marked and response will follow.
        /// </summary>
        /// <param name="given">given phrase</param>
        /// <returns>response, if was processed, null, if 'not my thing'</returns>
        public string Process(string given)
        {
            // Check entry param
            if (string.IsNullOrWhiteSpace(given)) return null;
            // Check, if _dialogUtils is initalized
            if (_dialogUtils == null) return "_dialogUtils is not present.";
            // Change to lower case
            given = given.Trim().ToLower();
            // Prepare response variable
            string responseText = null;
            // Check, in known greetings
            if (new[] { "hi", "hello", "welcome" }.Contains(given))
            {
                responseText = _dialogUtils.Greeting();
            }
            else if (new[] { "bye", "goodbye", "farewell" }.Contains(given))
            {
                responseText = _dialogUtils.Valediction();
            }
            return responseText;
        }

        /// <summary>
        /// Returns short description of this skill.
        /// </summary>
        /// <returns>short description</returns>
        public string About => "It operates all greetings and valedictions, so if passed phrase is a hello or bye, it will be processed here.";

        /// <summary>
        /// Count of known greetings.
        /// </summary>
        public int Count => 6;
    }
}
