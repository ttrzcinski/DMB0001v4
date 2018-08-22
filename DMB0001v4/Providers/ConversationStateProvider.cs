using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;

namespace DMB0001v4.Providers
{
    public class ConversationStateProvider : IConversationStateProvider
    {
        public TState GetConversationState<TState>(ITurnContext context) where TState : class, new()
        {
            return context.GetConversationState<TState>();
        }
    }
}
