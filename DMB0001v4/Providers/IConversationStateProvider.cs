using Microsoft.Bot.Builder;

namespace DMB0001v4.Providers
{
    public interface IConversationStateProvider
    {
        TState GetConversationState<TState>(ITurnContext context) where TState : class, new();
    }
}