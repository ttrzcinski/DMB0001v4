using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DMB0001v4.Mind;
using DMB0001v4.Model;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace DMB0001v4.Skills
{
    public class Images : ISkill
    {
        /// <summary>
        /// List of kept images.
        /// </summary>
        private List<ResImage> _images;
        /// <summary>
        /// Backup copy of list of images.
        /// </summary>
        private List<ResImage> _stashedImages;

        /// <summary>
        /// Kept instance of Iamges storage.
        /// </summary>
        private static Images _instance;
        /// <summary>
        /// Handle for returning instance
        /// </summary>
        public static Images Instance => _instance;

        // TODO: Change to relative path
        /// <summary>
        /// Path to catalog with images.
        /// </summary>
        private const string _resourcePath = "C:\\vsproj\\DMB0001v4\\DMB0001v4\\DMB0001v4\\imgs\\";
        /// <summary>
        /// Locks files changing - usable in tests.
        /// </summary>
        public static bool ReadOnlyFile { get; set; }
        /// <summary>
        /// Dictionary of generated activities as take away.
        /// </summary>
        private Dictionary<string, IMessageActivity> _activities;

        /// <summary>
        /// The highest id of items.
        /// </summary>
        private static int _maxId = 0;
        /// <summary>
        /// (Read-only) The highest id of items.
        /// </summary>
        public int MaxId => _maxId;
        /// <summary>
        /// (Read-only) Next usable id of items.
        /// </summary>
        public int NextMaxId => _maxId + 1;

        /// <summary>
        /// Obtains instance of this class with passing context and state.
        /// </summary>
        /// <param name="context">current context</param>
        /// <param name="conversationStateProvider">passed state provider</param>
        /// <returns>instance of skill</returns>
        public ISkill GetInstance(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            if (_instance == null)
                _instance = new Images();
            return _instance;
        }

        /// <summary>
        /// Creates new instance of image and returns it to local caller.
        /// </summary>
        private Images()
        {
            InitList();
        }

        /// <summary>
        /// Initlalizes list of kept items.
        /// </summary>
        public void InitList()
        {
            if (_activities == null)
                _activities = new Dictionary<string, IMessageActivity>();
        }

        /// <summary>
        /// Read images from resource catalog.
        /// </summary>
        public void Load()
        {
            // Assert resources catalog
            if (FileUtils.AssureFile(_resourcePath))
            {
                // Stashing previous state of list and rollback, if something went wrong
                StashBackup();
                // Get files info from catalog
                var extensions = new[] { "png", "jpg", "jpeg", "gif"};
                var files = Directory.EnumerateFiles(_resourcePath, "*.*", SearchOption.AllDirectories)
                    .Where(s => extensions.Contains(s.Split(".")[1]));
                // Zero the maxId
                _maxId = 0;
                // Iterate throug hthe images
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(_resourcePath + file);
                    // 
                    ResImage resImage = new ResImage();
                    resImage.Id = NextMaxId;
                    resImage.filename = file;
                    resImage.imageType = fileInfo.Extension;
                    // TODO Maybe some day add rest of fields
                    _maxId++;
                }
                FixMaxId();
            }
            else
                Console.WriteLine($"Couldn't read images from {_resourcePath}.");
        }

        /// <summary>
        /// Recounts top id from known items.
        /// </summary>
        public void FixMaxId() => _maxId = _images != null ? _images.Select(t => t.Id).OrderByDescending(t => t).FirstOrDefault() : 1;

        /// <summary>
        /// Stashes backup of list of items.
        /// </summary>
        public void StashBackup() => _stashedImages = _images.Select((item) => new ResImage() { Id = item.Id, url = item.url, }).ToList();

        /// <summary>
        /// Processes given phraase
        /// </summary>
        /// <param name="given"></param>
        /// <returns></returns>
        public string Process(string given)
        {
            // Check entered param
            if (string.IsNullOrWhiteSpace(given)) return null;
            given = given.Trim();
            // Prepare response
            string responseText = null;
            // Process request of inner command to obtain an activity
            if (given.StartsWith("download pic ") && given.Length > 13)
            {
                // Prepare default answer
                responseText = "Couldn't even parse your query.";
                // TODO add cutting filename from given phrase
                var fileName = given.Substring(given.Length - 14);
                var url = given.Substring(13);
                // Prepare default answer
                responseText = $"Couldn't download image from {url}";
                bool downloaded = Download(url);
                // Check outcome of download
                if (downloaded) responseText = $"Downloaded image from {url} to file {fileName}.";
                return responseText;
            }
            // Process commands
            switch (given)
            {
                case "show me net pic":
                    var activity1 = MessageFactory.Attachment(new Attachment[]
                    {
                        new Attachment { ContentUrl = "https://avatars2.githubusercontent.com/u/12435750?s=460&v=4", ContentType = "image/jpg" }
                    });
                    //responseText = "Is there an image?";
                    var uniqueName = FileUtils.GenerateUniqueFileName("jpg");
                    _activities.Add(uniqueName, activity1);
                    responseText = $"imageget.{uniqueName}";
                    //await context.SendActivity(activity1);
                    break;

                case "show me local pic":
                    var activity2 = MessageFactory.Attachment(new Attachment[]
                    {
                        // If file is there, it works
                        new Attachment { ContentUrl = "c:\\warn.jpg", ContentType = "image/jpg" }
                        // TODO Change to relative path
                        //new Attachment { ContentUrl = $"{new SystemUtils(context, _conversationStateProvider).ProjectPath()}imgs\\small-image.png", ContentType = "image/png" }//,
                    });
                    var uniqueName2 = FileUtils.GenerateUniqueFileName("jpg");
                    _activities.Add(uniqueName2, activity2);
                    //responseText = "Is there an image?";
                    responseText = $"imageget.{uniqueName2}";
                    //await context.SendActivity(activity2);
                    break;
                default:
                    responseText = "And what I suppose to do with that image?!";
                    break;
            }

            return responseText;
        }

        public IMessageActivity ShowActivity(string fileName)
        {
            //
            IMessageActivity theActivity = null;
            //
            /*if (given.StartsWith("imageget.") && given.Length > 9)
            {
                // Read unique name
                var theName = given.Split(".")[1];*/
            // Get image from dictionary
                theActivity = _activities[fileName];
                var removal = _activities.Remove(fileName);
                //if (!removal)
                //{
                //    responseText = $"Couldn't remove activity {fileName}.";
                //}
            //}
            return theActivity;
        }

        /// <summary>
        /// Downloads wanted image from given url.
        /// </summary>
        /// <param name="url">given url</param>
        /// <param name="fileName">filename ot save in resources</param>
        /// <returns>true, if downloaded and saved, false otherwise</returns>
        public bool Download(string url, string fileName = null)
        {
            // Check, if given url is not empty
            if (string.IsNullOrWhiteSpace(url)) return false;
            //
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = FileUtils.GenerateUniqueFileName(".png");
            // Download the image
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile(url, fileName);
            }
            return false;
        }

        /// <summary>
        /// Short description of the skill.
        /// </summary>
        public string About => "Will provide images support in processing and passing.";

        /// <summary>
        /// Checks, if list of images is empty.
        /// </summary>
        /// <returns>true means empty, false otherwise</returns>
        public bool IsEmpty() => _images != null ? (_images.Count == 0) : true;

        /// <summary>
        /// Count of kept items in the list.
        /// </summary>
        public int Count => _images != null ? _images.Count : 0;
    }
}
