using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;

namespace DMB0001v4.Skills
{
    public class Images : ISkill
    {
        /// <summary>
        /// Kept images in imgs.
        /// </summary>
        private List<string> _imgs;

        /// <summary>
        /// Kept instance of Iamges storage.
        /// </summary>
        private static Images _instance;

        public string About => "Will provide images support in processing and passing.";

        public int Count => _imgs != null ? _imgs.Count : 0;

        public ISkill GetInstance(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            if (_instance == null)
            {
                _instance = new Images();
            }

            return _instance;
        }

        public string Process(string given)
        {
            return "And what I suppose to do with that image?!";
        }

        public bool Download(string url, string fileName = null)
        {
            // Check, if given url is not empty
            if (string.IsNullOrWhiteSpace(url)) return false;
            //
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "downloaded_image.png";//GenerateUniqueFileName();
            // Download the image
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile(url, fileName);
            }
            //
            return false;
        }

        /// <summary>
        /// Generates unique name of file.
        /// </summary>
        /// <returns>unique name of file</returns>
        private string GenerateUniqueFileName() => $"downloaded_image_{_imgs.Count + 1}_{DateTime.Now.Ticks}.png";
    }
}
