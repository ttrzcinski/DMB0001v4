using System;
using System.IO;

namespace DMB0001v4.Mind
{
    public static class FileUtils
    {
        /// <summary>
        /// Assures presence of file in given location.
        /// </summary>
        /// <param name="path">given full filepath</param>
        /// <returns>true means assured, fale means errors</returns>
        public static bool assureFile(string path)
        {
            // Check entry params
            if (string.IsNullOrWhiteSpace(path)) return false;
            // Prepare return result
            bool result = false;
            // Check, if file exist
            if (File.Exists(path))
            {
                result = true;
            }
            else
            {
                string content = path.EndsWith(".json", StringComparison.Ordinal) ? "[ ]" : " ";
                StreamWriter fileWrite = null;
                try
                {
                    using (fileWrite = new StreamWriter(path))
                    {
                        fileWrite.WriteLine(content);
                    }
                    result = true;
                }
                catch (IOException ioex)
                {
                    result = false;
                }
                finally
                {
                    if (fileWrite != null)
                        try
                        {
                            fileWrite.Close();
                        }
                        catch (Exception ex)
                        {
                            result = false;
                        }
                }
            }
            return result;
        }
    }
}
