using System;

namespace DMB0001v4.Mind
{
    public class SystemUtils
    {
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
                if (!string.IsNullOrEmpty(startupPath))
                {
                    response = startupPath;
                }
                else
                {
                    response = "I don't have permissions to ask System, where am I.";
                }
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
                if (!string.IsNullOrEmpty(userName))
                {
                    response = userName;
                }
                else
                {
                    response = "I don't have permissions to ask System, what is your name.";
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
