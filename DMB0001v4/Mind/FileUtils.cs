using System;
using System.IO;
using DMB0001v4.Extensions;

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
            => $"{DateTime.Now.Ticks}.{(string.IsNullOrWhiteSpace(extension) ? extension = "dafile" : extension)}";

        /// <summary>
        /// Returns project's catalog.
        /// </summary>
        /// <returns>project's catalog</returns>
        public static string ProjectCatalog()
        {
            string result = null;
            var debugPath = AppDomain.CurrentDomain.BaseDirectory;
            if (debugPath.HasContent())
            {
                int binOccurence = debugPath.IndexOf("bin");
                if (binOccurence > -1)
                    return debugPath.Substring(0, debugPath.IndexOf("bin"));
            }
            return result;
        }

        /// <summary>
        /// Returns resources catalog
        /// </summary>
        /// <returns></returns>
        public static string ResourcesCatalog() => ProjectCatalog() + "Resources\\";
    }
}
