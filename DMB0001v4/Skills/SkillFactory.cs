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
        /// Returns the only instance of skill factory.
        /// </summary>
        /// <returns></returns>
        public static SkillFactory GetInstance(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            if (_factory == null)
            {
                lock (padlock)
                {
                    //
                    _conversationStateProvider = conversationStateProvider;
                    //
                    _factory = _factory ?? new SkillFactory();
                    // Check, if skill set is empty
                    if (_skills.Count == 0)
                    {
                        var knownSkills = new[] { "greetings", "retorts"};//, "questions" };
                        var greetingInstance = Greetings.Instance(context, conversationStateProvider);
                        var retortsInstance = Retorts.Instance(context, conversationStateProvider);
                        //var questionsInstance = Questions.Instance(context, conversationStateProvider);
                        //
                        _skills.Add("greetings", greetingInstance);
                        _skills.Add("retorts", retortsInstance);
                        //_skills.Add("questions", questionsInstance);
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
            // if it does exist, read it and return it
            if (_skills.TryGetValue(key, out ISkill instance))
            {
                instance = _skills[key];
            }
            else
            {
                // First time - call to init an instance
                switch (key)
                {
                    // TODO if would be nice, if this would be classfiles processor on directory - not to type every single skill in factory
                    case "greetings":
                        instance = Greetings.Instance(context, conversationStateProvider);
                        break;
                    case "retorts":
                        instance = Retorts.Instance(context, conversationStateProvider);
                        break;
                    /*case "questions":
                        instance = Questions.Instance(context, conversationStateProvider);
                        break;*/
                }
                // If obtained instance, add it to known skills
                if (instance != null)
                    _skills.Add(key, instance);
            }
            return instance;
        }

        // --- ---- --- LEAVE EVERYTHING BELOW THIS LINE AS IT IS - IT'S OK --- ----- ---
        /// <summary>
        /// Kept pool filled with different skills.
        /// </summary>
        private static Dictionary<string, ISkill> _skills;

        /// <summary>
        /// Kept instance of skills factory.
        /// </summary>
        private static SkillFactory _factory;

        /// <summary>
        /// Kept provider to pass mind's state and make it changeable from inside of skill.
        /// </summary>
        private static IConversationStateProvider _conversationStateProvider;

        /// <summary>
        /// Serves as safety lock in creating instance of singleton.
        /// </summary>
        private static readonly object padlock = new object();

        // this is the classic good old singleton trick (prevent direct instantiation)
        private SkillFactory() => AssureSkills();

        /// <summary>
        /// Assures, that skillset will be initialized.
        /// </summary>
        private void AssureSkills()
        {
            if (_skills == null)
                _skills = new Dictionary<string, ISkill>();
        }

        /// <summary>
        /// Processes given phrase through all known skills.
        /// </summary>
        /// <param name="toProcess">given phrase</param>
        /// <returns>response, if some responded, null otherwise</returns>
        public string Process(string toProcess)
        {
            string response = null;
            // Loop through all known skills, until some will respond.
            foreach (KeyValuePair<string, ISkill> skill in _skills)
            {
                response = skill.Value.Process(toProcess);
                // If responded, stop looping
                if (response != null) break;
            }
            return response;
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
    }
}
