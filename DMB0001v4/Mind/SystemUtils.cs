using Microsoft.Bot.Builder;
using System;
using DMB0001v4.Providers;

namespace DMB0001v4.Mind
{
    public class SystemUtils
    {
        // TODO Add Unit Tests for those methods

        /// <summary>
        /// State of currently remembered facts and knowledge.
        /// </summary>
        private BrainState _state;

        /// <summary>
        /// Creates new instance of Utils for calling system.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        public SystemUtils(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            _state = conversationStateProvider.GetConversationState<BrainState>(context);
        }

        /// <summary>
        /// Returns projects paths in safe mode.
        /// </summary>
        /// <returns>project path, if permissions are in place</returns>
        public string ProjectPath()
        {
            string response;
            try
            {
                var startupPath = AppDomain.CurrentDomain.BaseDirectory;
                response = !string.IsNullOrEmpty(startupPath) 
                    ? startupPath 
                    : "I don't have permissions to ask System, where am I.";
            }
            catch (Exception exception1)
            {
                response = exception1.StackTrace;
            }
            return response;
        }

        /// <summary>
        /// Reads from Security layer name of current user.
        /// </summary>
        /// <returns>current user's name</returns>
        public string UserName()
        {
            string response;
            try
            {
                var userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                response = !string.IsNullOrEmpty(userName) 
                    ? userName 
                    : "I don't have permissions to ask System, what is your name.";
                if (!response.StartsWith("I don't", StringComparison.Ordinal))
                {
                        _state.UsersName = response;
                }
            }
            catch (Exception exception1)
            {
                response = exception1.StackTrace;
            }
            return response;
        }
    }
}
