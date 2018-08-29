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
        public static bool AssureFile(string path)
        {
            // Check entry params
            if (string.IsNullOrWhiteSpace(path)) return false;
            // Prepare return result
            var result = false;
            // Check, if file exist
            if (File.Exists(path))
            {
                result = true;
            }
            else
            {
                var content = path.EndsWith(".json", StringComparison.Ordinal) ? "[ ]" : " ";
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

        /// <summary>
        /// Generates unique name of file.
        /// </summary>
        /// <returns>unique name of file</returns>
        public static string GenerateUniqueFileName(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) extension = "dafile";
            return $"{DateTime.Now.Ticks}.{extension}";
        }
    }
}
