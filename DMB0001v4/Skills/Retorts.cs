using System;
using System.Collections.Generic;
using System.Linq;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace DMB0001v4.Skills
{
    /// <summary>
    /// 
    /// </summary>
    public class Retorts : ISkill
    {
        /// <summary>
        /// State of currently remembered facts and knowledge.
        /// </summary>
        private BrainState _state;
        /// <summary>
        /// Kept instance of skill.
        /// </summary>
        private static Retorts _instance;
        /// <summary>
        /// Serves as safety lock in creating instance of singleton.
        /// </summary>
        private static readonly object padlock = new object();

        private static Dictionary<string, string> _retorts;

        /// <summary>
        /// Bloecked empty constructors - this skills is a snigleton.
        /// </summary>
        private Retorts()
        { }

        /// <summary>
        /// Creates new instance of retorts skill to process given phrase.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        private Retorts(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            _state = conversationStateProvider.GetConversationState<BrainState>(context);
            // Create new DialogUtils to hide logic in sub-methods
            //_dialogUtils = new DialogUtils(context, conversationStateProvider);
        }

        /// <summary>
        /// Creates new instance of skill.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of retorts</returns>
        public ISkill GetInstance(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            return Instance(context, conversationStateProvider);
        }

        /// <summary>
        /// Creates new instance of skill.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of retorts</returns>
        public static Retorts Instance(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            if (_instance == null)
                lock (padlock)
                    if (_instance == null)
                        _instance = new Retorts(context, conversationStateProvider);
            return _instance;
        }

        /// <summary>
        /// Gives short description about the skill, what it does.
        /// </summary>
        /// <returns>short description</returns>
        public string About() => "Contains set of quick responses from easily extendable file.";

        /// <summary>
        /// Assures, that retorts are present and are not null.
        /// </summary>
        private static void assureRetorts()
        {
            if (_retorts == null)
            {
                _retorts = new Dictionary<string, string>();
            }
            // TODO READ from file retorts
        }

        /// <summary>
        /// Processes given request.
        /// </summary>
        /// <param name="given">request</param>
        /// <returns>value means processed, null means 'not my thing'</returns>
        public string Process(string given)
        {
            // Assures init of list of retorts
            assureRetorts();
            // Returns wanted retort's value, if key is known in set
            return _retorts.ContainsKey(given) ? _retorts[given] : null;
        }

        /// <summary>
        /// Returns count of all kept retorts.
        /// </summary>
        /// <returns>count of all kept retorts</returns>
        public static int Count => (_retorts ?? new Dictionary<string, string>()).Count;

        /// <summary>
        /// Clears pool of kept instances.
        /// </summary>
        public static void Clear() => (_retorts ?? new Dictionary<string, string>()).Clear();

        /// <summary>
        /// Retursn flag, if retorts set is empty.
        /// </summary>
        /// <returns>true means empty, false otherwise</returns>
        public static bool IsEmpty() => Count == 0;

        /// <summary>
        /// Adds new retort to set.
        /// </summary>
        /// <param name="key">request of retort</param>
        /// <param name="value">response of retort</param>
        /// <returns>true means added, false otherwise</returns>
        public static bool Add(string key, string value)
        {
            // Assures, that NPE wont happen
            assureRetorts();
            // Prepare return falg
            bool result = false;
            // Check element before adding it
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
            {

                int beforeCount = _retorts.Count;
                if (!_retorts.ContainsKey(key))
                {
                    _retorts.Add(key, value);
                    result = beforeCount < _retorts.Count;
                    if (result)
                    {
                        // TODO Persist added value in file and in storages
                        ;//result = false;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Adds given retorts to kept set.
        /// </summary>
        /// <param name="elements">retorts to add</param>
        /// <returns>true means added, false otherwise</returns>
        public static bool AddAll(IDictionary<string, string> elements)
        {
            // Assures, that NPE wont happen
            assureRetorts();
            // Prepare return falg
            bool result = false;
            int beforeCount = _retorts.Count;
            foreach (KeyValuePair<string, string> element in elements)
            {
                // Check element before adding it
                if (!string.IsNullOrEmpty(element.Key))
                {
                    _retorts.Add(element.Key, element.Value);
                }
            }
            result = beforeCount < _retorts.Count;
            if (result)
            {
                // TODO Persist added value in file and in storages
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Removes pointed retort from set.
        /// </summary>
        /// <param name="key">request of retort</param>
        /// <returns>true means removed, false otherwise</returns>
        public static bool Remove(string key)
        {
            // Assures, that NPE wont happen
            assureRetorts();
            // Prepare return falg
            bool result = false;
            // Check element before try to remove
            if (!string.IsNullOrEmpty(key))
            {
                int beforeCount = _retorts.Count;
                result = _retorts.ContainsKey(key) ? _retorts.Remove(key) : false;
                int afterCount = _retorts.Count;
                result = beforeCount > afterCount;
                if (result)
                {
                    ; // TODO Persist removed value in file and in storages
                }
            }

            return result;
        }

        // TODO ADD REMOVE ALL
        /// <summary>
        /// Removed wanted list of retorts from kept set.
        /// </summary>
        /// <param name="elements">retorts to remove</param>
        /// <returns>true means added, false otherwise</returns>
        public static bool RemoveAll(IDictionary<string, string> elements)
        {
            // Assures, that NPE wont happen
            assureRetorts();
            // Prepare return falg
            bool result = false;
            int beforeCount = _retorts.Count;
            foreach (KeyValuePair<string, string> element in elements)
            {
                // Check element before try to remove
                if (!string.IsNullOrEmpty(element.Key))
                {
                    bool tempResult = _retorts.ContainsKey(element.Key) ? _retorts.Remove(element.Key) : false;
                }
            }
            result = beforeCount > _retorts.Count;
            if (result)
            {
                // TODO Persist removed value in file and in storages
                result = false;
            }
            return result;
        }
    }
}
