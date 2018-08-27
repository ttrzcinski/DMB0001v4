using System.Collections.Generic;
using System.IO;
using DMB0001v4.Model;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System.Linq;

namespace DMB0001v4.Skills
{
    public class Questions : ISkill
    {
        /// <summary>
        /// PRocesses given question with know patterns of questions in order to extract facts, check common knowledge aboput them and put it to the answer.
        /// </summary>
        /// <param name="given">given question</param>
        /// <returns>answer wit hfacts, if found, null otherwise</returns>
        public string Process(string given)
        {
            string response = null;
            // Check in known patterns
            foreach (var known in  _knownPatterns)
            {
                if (known.Pattern_question.Equals(given))
                {
                    //TODO Add processing with facts
                    response = known.Pattern_answer;
                    break;
                }
            }
            return response;
        }

        // --- ---- --- LEAVE EVERYTHING BELOW THIS LINE AS IT IS - IT'S OK --- ----- ---

        /// <summary>
        /// State of currently remembered facts and knowledge.
        /// </summary>
        private BrainState _state;
        /// <summary>
        /// Kept instance of questions.
        /// </summary>
        private static Questions _instance;
        /// <summary>
        /// Serves as safety lock in creating instance of singleton.
        /// </summary>
        private static readonly object padlock = new object();
        // TODO MAKE IT RELATIVE TO PROJECT'S DIR
        /// <summary>
        /// Hardcoded path to fast retorts file.
        /// </summary>
        private const string RetortsFullPath = "C:\\vsproj\\DMB0001v4\\DMB0001v4\\DMB0001v4\\Resources\\known_patterns.json";
        /// <summary>
        /// Keopt list of known question patterns.
        /// </summary>
        private List<Question> _knownPatterns;
        /// <summary>
        /// The highest id of known patterns.
        /// </summary>
        private static int _patternsMaxId = -1;
        /// <summary>
        /// Blocked empty constructors - this skills is a snigleton.
        /// </summary>
        private Questions()
        { }

        /// <summary>
        /// Creates new instance of questions skill to process given phrase.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        private Questions(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            _state = conversationStateProvider.GetConversationState<BrainState>(context);
            // Create new DialogUtils to hide logic in sub-methods
            AssureQuestions();
        }

        /// <summary>
        /// Creates new instance of skill.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of questions</returns>
        public ISkill GetInstance(ITurnContext context, IConversationStateProvider conversationStateProvider)
            => Instance(context, conversationStateProvider);

        /// <summary>
        /// Creates new instance of skill.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of questions</returns>
        public static Questions Instance(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            if (_instance == null)
                lock (padlock)
                    if (_instance == null)
                        _instance = new Questions(context, conversationStateProvider);
            return _instance;
        }

        /// <summary>
        /// Assures presence of questions by initializing them and reading content from known patterns file.
        /// </summary>
        private void AssureQuestions()
        {
            if (_knownPatterns == null)
            {
                _knownPatterns = new List<Question>();
                // Loading know patterns from file
                LoadPatterns();
            }
        }

        /// <summary>
        /// Reads JSON file with known patterns.
        /// </summary>
        public void LoadPatterns()
        {
            // TODO: Change to relative path
            using (var reader = new StreamReader(RetortsFullPath))
            {
                var json = reader.ReadToEnd();
                var items = JsonConvert.DeserializeObject<List<Question>>(json);
                _knownPatterns = items;

                // TODO: Fix the path top search from within the project
                FixITToMax();
            }
        }

        /// <summary>
        /// Finds the highest id from retorts.
        /// </summary>
        private void FixITToMax()
        {
            if (_knownPatterns != null) _patternsMaxId = _knownPatterns.Select(t => t.Id).OrderByDescending(t => t).FirstOrDefault();
        }

        /// <summary>
        /// Returns top id of all known patterns.
        /// </summary>
        /// <returns>top id, if there are some patterns, on null or empty returns 0</returns>
        public int RetortsMaxId()
        {
            return _patternsMaxId;
        }

        /// <summary>
        /// Gives short description about the skill, what it does.
        /// </summary>
        /// <returns>short description</returns>
        public string About => "Processes questions thyrough common pattern in order to parse 'things' and read all the known details about a 'thing'.";

        /// <summary>
        /// REturns count of known question patterns.
        /// </summary>
        /// <returns>count of knwon patterns</returns>
        public int Count => _knownPatterns != null ? _knownPatterns.Count : 0;
    }
}
