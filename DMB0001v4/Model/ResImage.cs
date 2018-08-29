using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMB0001v4.Model
{
    public class ResImage
    {
        public int Id { get; set; }
        public string url { get; set; }
        public string filename { get; set; }
        public string imageType { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        // Returns concatenated form of ResImage to list it.
        /// <summary>
        /// Returns concatenated form of ResImage to list it.
        /// </summary>
        /// <returns>id) ResImage as a line</returns>
        public string AsStackEntry()
        {
            return $"{Id}). {filename} ({width}x{height}) ({imageType}) from {url}.";
        }
    }
}
