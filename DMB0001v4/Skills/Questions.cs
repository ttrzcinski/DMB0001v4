using System;
using System.Collections.Generic;
using System.IO;
using DMB0001v4.Model;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;

namespace DMB0001v4.Skills
{
    public class Questions : ISkill
    {
        public string Process(string given)
        {
            throw new NotImplementedException();
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
            assureQuestions();
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

        private void assureQuestions()
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
                //FixITToMax();
            }
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
