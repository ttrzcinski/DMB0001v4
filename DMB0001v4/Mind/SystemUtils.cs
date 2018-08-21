using Microsoft.Bot.Builder;
using System;
using Microsoft.Bot.Builder.Core.Extensions;

namespace DMB0001v4.Mind
{
    public class SystemUtils
    {
        // TODO Add Unit Tets for those methods

        /// <summary>
        /// Context of current dialog.
        /// </summary>
        private ITurnContext _context;
        /// <summary>
        /// State of currently remebered facts and knowledge.
        /// </summary>
        private BrainState _state;

        /// <summary>
        /// Creates new instance of Utils for calling system.
        /// </summary>
        /// <param name="context">current dialog context</param>
        public SystemUtils(ITurnContext context)
        {
            _context = context; // TODO Maybe remove it from class variables - state is the only important one
            _state = context.GetConversationState<BrainState>();
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
                string startupPath = AppDomain.CurrentDomain.BaseDirectory;
                response = !string.IsNullOrEmpty(startupPath) ? startupPath : "I don't have permissions to ask System, where am I.";
            }
            catch (Exception exc_1)
            {
                response = exc_1.StackTrace;
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
                string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                response = !string.IsNullOrEmpty(userName) ? userName : "I don't have permissions to ask System, what is your name.";
                if (!response.StartsWith("I don't", StringComparison.Ordinal))
                {
                        _state.UsersName = response;
                }
            }
            catch (Exception exc_1)
            {
                response = exc_1.StackTrace;
            }
            return response;
        }
    }
}
