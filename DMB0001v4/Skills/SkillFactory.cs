using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using System.Collections.Generic;

namespace DMB0001v4.Skills
{
    /// <summary>
    /// Keeps all skills together in one multiton pool as a Factory beging singleton.
    /// </summary>
    public class SkillFactory
    {
        /// <summary>
        /// Kept pool filled with different skills.
        /// </summary>
        private static readonly Dictionary<string, ISkill> _skills = new Dictionary<string, ISkill>();

        /// <summary>
        /// Kept instance of skills factory.
        /// </summary>
        private static SkillFactory _factory;

        private readonly IConversationStateProvider _conversationStateProvider;

        /// <summary>
        /// Serves as safety lock in creating instance of singleton.
        /// </summary>
        private static readonly object padlock = new object();

        // this is the classic good old singleton trick (prevent direct instantiation)
        private SkillFactory()
        { }

        /// <summary>
        /// Returns the only instance of skill factory.
        /// </summary>
        /// <returns></returns>
        public static SkillFactory GetInstance()
        {
            if (_factory == null)
            {
                lock (padlock)
                {
                    if (_factory == null)
                    {
                        _factory = new SkillFactory();
                    }
                }
            }
            return _factory;
        }

        /// <summary>
        /// Returns wanted skill, if there exist one with such a name.
        /// </summary>
        /// <param name="key">skill's name</param>
        /// <param name="context">used context</param>
        /// <param name="conversationStateProvider">given conversation provider to access BrainState</param>
        /// <returns>wanted skill, if found, null otherwise</returns>
        public ISkill GetSkill(string key, ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            lock (_skills)
            {
                // if it doesn't exist, create it and store it
                if (_skills.TryGetValue(key, out ISkill instance))
                {
                    //It does exist
                    instance = _skills[key];
                }
                else
                {
                    // at this point, you can create a derived class instance
                    switch (key)
                    {
                        // TODO if would be nice, if this would be classfiles processor on directory - not to type every single skill in factory
                        case "greetings":
                            instance = Greetings.Instance(context, conversationStateProvider);
                            break;
                        case "retorts":
                            instance = Retorts.Instance(context, conversationStateProvider);
                            break;
                    }
                    //
                    if (instance != null)
                    {
                        _skills.Add(key, instance);
                    }
                }

                // always return the same ("singleton") instance for this key
                return instance;
            }
        }

        /// <summary>
        /// Returns count of all kept skills.
        /// </summary>
        /// <returns>count of all kept skills</returns>
        public static int Count => (_skills ?? new Dictionary<string, ISkill>()).Count;

        /// <summary>
        /// Clears pool of kept instances.
        /// </summary>
        public static void Clear() => (_skills ?? new Dictionary<string, ISkill>()).Clear();

        /// <summary>
        /// Processes given phrase through all known skills.
        /// </summary>
        /// <param name="toProcess">given phrase</param>
        /// <returns>response, if some responded, null otherwise</returns>
        public string Process(string toProcess)
        {
            // Prepare response
            string response = null;
            // Loop through all known skills, until some will respond.
            foreach (KeyValuePair<string, ISkill> skill in _skills)
            {
                response = skill.Value.Process(toProcess);
                // If responded, stop looping
                if (response != null) break;
            }
            // Return the response
            return response;
        }
    }
}
