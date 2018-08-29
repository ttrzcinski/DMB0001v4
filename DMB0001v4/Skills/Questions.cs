using System;
using System.Collections.Generic;
using System.IO;
using DMB0001v4.Model;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System.Linq;
using DMB0001v4.Mind;

namespace DMB0001v4.Skills
{
    public class Questions : ISkill
    {
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
        private string _fullPath { get; set; }
        /// <summary>
        /// Default path, if file's full path was not set.
        /// </summary>
        private string _defaultFullPath = "C:\\vsproj\\DMB0001v4\\DMB0001v4\\DMB0001v4\\Resources\\known_patterns.json";

        /// <summary>
        /// Kept list of known question patterns.
        /// </summary>
        private List<Question> _knownPatterns;
        /// <summary>
        /// The highest id of known patterns.
        /// </summary>
        private static int _maxId;
        /// <summary>
        /// Returns top id of all known patterns.
        /// </summary>
        /// <returns>top id, if there are some patterns, on null or empty returns 0</returns>
        public int MaxId() => _maxId;

        /// <summary>
        /// Blocked empty constructors - this skills is a snigleton.
        /// </summary>
        private Questions()
        {
            // Create new DialogUtils to hide logic in sub-methods
            AssureQuestions();
        }

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
                // Set _maxId to beginning
                _maxId = 0;
                // Loading know patterns from file
                Load();
            }
        }

        /// <summary>
        /// Reads JSON file with known patterns.
        /// </summary>
        public void Load()
        {
            // If kept path is null, use default
            if (string.IsNullOrWhiteSpace(_fullPath)) _fullPath = _defaultFullPath;
            // Assure file to load
            if (FileUtils.AssureFile(_fullPath))
                using (var reader = new StreamReader(_fullPath))
                {
                    var json = reader.ReadToEnd();
                    var items = JsonConvert.DeserializeObject<List<Question>>(json);
                    _knownPatterns = items;
                    // Fix the path top search from within the project
                    FixMaxId();
                }
            else
                Console.WriteLine($"Couldn't read file from {_fullPath}.");
        }

        /// <summary>
        /// Finds the highest id from items.
        /// </summary>
        public void FixMaxId()
        {
            if (_knownPatterns != null) _maxId = _knownPatterns.Select(t => t.Id).OrderByDescending(t => t).FirstOrDefault();
        }

        /// <summary>
        /// Returns next top id from items.
        /// </summary>
        /// <returns>next id, which can be used as id for added item</returns>
        public int NextId() => _maxId < 1 ? 1 : _maxId + 1;

        /// <summary>
        /// Processes given question with know patterns of questions in order to extract facts,
        /// check common knowledge about them and put it to the answer.
        /// </summary>
        /// <param name="given">given question</param>
        /// <returns>answer with facts, if found, null otherwise</returns>
        public string Process(string given)
        {
            // Check entry param
            if (string.IsNullOrWhiteSpace(given)) return null;
            // Change to lower case
            given = given.Trim().ToLower();
            // Check in known patterns
            return _knownPatterns.Find(x => x.Pattern_question == given)?.Pattern_answer;
        }

        /// <summary>
        /// Gives short description about the skill, what it does.
        /// </summary>
        /// <returns>short description</returns>
        public string About => "Processes questions through common pattern in order to parse 'things' and read all the known details about a 'thing'.";

        /// <summary>
        /// Returns count of known question patterns.
        /// </summary>
        /// <returns>count of knwon patterns</returns>
        public int Count => _knownPatterns?.Count ?? 0;
    }
}
