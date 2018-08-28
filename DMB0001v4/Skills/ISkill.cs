using DMB0001v4.Providers;
using Microsoft.Bot.Builder;

namespace DMB0001v4.Skills
{
    /// <summary>
    /// Set of basics, which every skill has to implements in order to make them a-like.
    /// </summary>
    public interface ISkill
    {
        /// <summary>
        /// Returns instance of the skill and creates one, if this is first call.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of skill</returns>
        ISkill GetInstance(ITurnContext context, IConversationStateProvider conversationStateProvider);

        /// <summary>
        /// Processes given request.
        /// </summary>
        /// <param name="given">request</param>
        /// <returns>value means processed, null means 'not my thing'</returns>
        string Process(string given);

        /// <summary>
        /// Gives short description about the skill, what it does.
        /// </summary>
        /// <returns>short description</returns>
        string About { get; }
    }
}
