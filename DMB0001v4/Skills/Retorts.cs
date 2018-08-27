using System;
using System.Collections.Generic;
using System.Linq;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System.IO;
using DMB0001v4.Model;

namespace DMB0001v4.Skills
{
    /// <summary>
    /// Represents short responses to known queries.
    /// </summary>
    public class Retorts : ISkill
    {
        /// <summary>
        /// Returns current timestamp.
        /// </summary>
        /// <returns>current timestamp</returns>
        private static string Now()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssffff");
        }

        /// <summary>
        /// Backups retorts file in order not to loose all those retorts.
        /// </summary>
        /// <returns>true means backup was created, false otherwise</returns>
        private bool BackupRetorts()
        {
            // Prepare name of backup file
            var backupPath = RetortsFullPath.Replace(".json", $"_{Now()}.json");
            // Copy current file to backup
            File.Copy(RetortsFullPath, backupPath);
            // Check, if file exists
            return File.Exists(backupPath);
        }

        /// <summary>
        /// Processes given request.
        /// </summary>
        /// <param name="given">request</param>
        /// <returns>value means processed, null means 'not my thing'</returns>
        public string Process(string given)
        {
            // Assures init of list of retorts
            assureRetorts();
            // Obtain retorts, if are not loaded
            if (_retorts == null)
                LoadRetorts();
            // Prepare response variable
            string response = null;
            // TODO covert it to lambda expression
            // response = _retorts.Find(item => item.Question.ToLower().Equals(question)).Answer;
            foreach (var retort in _retorts)
                if (retort.Question.ToLower().Equals(given))
                {
                    response = retort.Answer;
                    break;
                }
            // Returns found response from retort's answer
            return response;
        }

        /// <summary>
        /// Adds new retort to set.
        /// </summary>
        /// <param name="key">request of retort</param>
        /// <param name="value">response of retort</param>
        /// <returns>true means added, false otherwise</returns>
        public static bool Add(string key, string value)
        {
            // Check, if params have content
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                return false;
            // Assures, that NPE wont happen
            assureRetorts();
            // Prepare return falg
            bool result = false;
            // Check previous count to confirm, that an element was added
            int beforeCount = _retorts.Count;
            if (!ContainsKey(key))
            {
                var added = new Retort { Id = _retortsMaxId + 1, Question = key, Answer = value };
                _retorts.Add(added);
                result = beforeCount < _retorts.Count;
                if (result)
                {
                    // TODO Persist added value in file and in storages
                    ;//result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Adds given retorts to kept set.
        /// </summary>
        /// <param name="elements">retorts to add</param>
        /// <returns>true means added, false otherwise</returns>
        public static bool AddAll(IDictionary<string, string> elements)
        {
            // Assures, that NPE wont happen
            assureRetorts();
            // Prepare return falg
            bool result = false;
            int beforeCount = _retorts.Count;
            foreach (KeyValuePair<string, string> element in elements)
            {
                // Check element before adding it
                if (!string.IsNullOrEmpty(element.Key))
                {
                    var added = new Retort { Id = _retortsMaxId + 1, Question = element.Key, Answer = element.Value };
                    _retorts.Add(added);
                }
            }
            result = beforeCount < _retorts.Count;
            if (result)
            {
                // TODO Persist added value in file and in storages
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Removes pointed retort from set.
        /// </summary>
        /// <param name="key">request of retort</param>
        /// <returns>true means removed, false otherwise</returns>
        public static bool Remove(string key)
        {
            // Assures, that NPE wont happen
            assureRetorts();
            // Prepare return falg
            bool result = false;
            // Check element before try to remove
            if (!string.IsNullOrEmpty(key))
            {
                int beforeCount = _retorts.Count;
                result = ContainsKey(key);
                for (int i = 0; i < _retorts.Count; i++)
                {
                    var retort = _retorts[i];
                    if (retort.Question.Equals(key))
                    {
                        result = _retorts.Remove(retort);
                    }
                }
                int afterCount = _retorts.Count;
                result = beforeCount > afterCount;
                if (result)
                {
                    ; // TODO Persist removed value in file and in storages
                }
            }

            return result;
        }

        // TODO ADD REMOVE ALL
        /// <summary>
        /// Removed wanted list of retorts from kept set.
        /// </summary>
        /// <param name="elements">retorts to remove</param>
        /// <returns>true means added, false otherwise</returns>
        public static bool RemoveAll(IDictionary<string, string> elements)
        {
            // Assures, that NPE wont happen
            assureRetorts();
            // Prepare return falg
            bool result = false;
            int beforeCount = _retorts.Count;
            foreach (KeyValuePair<string, string> element in elements)
            {
                // Check element before try to remove
                if (!string.IsNullOrEmpty(element.Key))
                {
                    bool tempResult = ContainsKey(element.Key);
                    for (int i = 0; i < _retorts.Count; i++)
                    {
                        var retort = _retorts[i];
                        if (retort.Question.Equals(element.Key))
                        {
                           tempResult = _retorts.Remove(retort);
                        }
                    }
                }
            }
            result = beforeCount > _retorts.Count;
            if (result)
            {
                // TODO Persist removed value in file and in storages
                result = false;
            }
            return result;
        }

        // --- ---- --- LEAVE EVERYTHING BELOW THIS LINE AS IT IS - IT'S OK --- ----- ---

        /// <summary>
        /// State of currently remembered facts and knowledge.
        /// </summary>
        private BrainState _state;
        /// <summary>
        /// Kept instance of retorts.
        /// </summary>
        private static Retorts _instance;
        /// <summary>
        /// Serves as safety lock in creating instance of singleton.
        /// </summary>
        private static readonly object padlock = new object();

        // TODO MAKE IT RELATIVE TO PROJECT'S DIR
        /// <summary>
        /// Hardcoded path to fast retorts file.
        /// </summary>
        private const string RetortsFullPath = "C:\\vsproj\\DMB0001v4\\DMB0001v4\\DMB0001v4\\Resources\\fast_retorts.json";

        /// <summary>
        /// Retorts as quick responses to questions.
        /// </summary>
        private static List<Retort> _retorts;

        /// <summary>
        /// The highest id of retorts.
        /// </summary>
        private static int _retortsMaxId = -1;

        /// <summary>
        /// Blocked empty constructors - this skills is a snigleton.
        /// </summary>
        private Retorts()
        { }

        /// <summary>
        /// Creates new instance of retorts skill to process given phrase.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        private Retorts(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            _state = conversationStateProvider.GetConversationState<BrainState>(context);
            // Create new DialogUtils to hide logic in sub-methods
            assureRetorts();
        }

        /// <summary>
        /// Creates new instance of skill.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of retorts</returns>
        public ISkill GetInstance(ITurnContext context, IConversationStateProvider conversationStateProvider)
            => Instance(context, conversationStateProvider);

        /// <summary>
        /// Creates new instance of skill.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        /// <returns>instance of retorts</returns>
        public static Retorts Instance(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            if (_instance == null)
                lock (padlock)
                    if (_instance == null)
                        _instance = new Retorts(context, conversationStateProvider);
            return _instance;
        }

        /// <summary>
        /// Assures, that retorts are present and are not null.
        /// </summary>
        private static void assureRetorts()
        {
            if (_retorts == null)
            {
                _retorts = new List<Retort>();
                // Read from file retorts
                LoadRetorts();
            }
        }

        /// <summary>
        /// Reads JSON file with retorts.
        /// </summary>
        public static void LoadRetorts()
        {
            // TODO: Change to relative path

            using (var reader = new StreamReader(RetortsFullPath))
            {
                var json = reader.ReadToEnd();
                var items = JsonConvert.DeserializeObject<List<Retort>>(json);
                _retorts = items;

                // TODO: Fix the path top search from within the project
                FindMaxRetortsId();
            }
        }

        /// <summary>
        /// Finds the highest id from retorts.
        /// </summary>
        private static void FindMaxRetortsId()
        {
            if (_retorts != null) _retortsMaxId = _retorts.Select(t => t.Id).OrderByDescending(t => t).FirstOrDefault();
        }

        /// <summary>
        /// Returns top id of all retorts.
        /// </summary>
        /// <returns>top id, if there are some retorts, on null or empty returns 0</returns>
        public int RetortsMaxId()
        {
            return _retortsMaxId;
        }

        /// <summary>
        /// Returns count of all kept retorts.
        /// </summary>
        /// <returns>count of all kept retorts</returns>
        public static int GetCount()
        {
            return (_retorts ?? new List<Retort>()).Count;
        }

        /// <summary>
        /// Clears pool of kept instances.
        /// </summary>
        public static void Clear() => (_retorts ?? new List<Retort>()).Clear();

        /// <summary>
        /// Retursn flag, if retorts set is empty.
        /// </summary>
        /// <returns>true means empty, false otherwise</returns>
        public static bool IsEmpty() => GetCount() == 0;

        /// <summary>
        /// Checks, if key is present in retorts.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true means presence, false otherwise</returns>
        public static bool ContainsKey(string key)
        {
            var result = false;
            foreach (var retort in _retorts)
                if (retort.Question.Equals(key))
                {
                    result = true;
                    break;
                }
            return result;
        }

        /// <summary>
        /// Gives short description about the skill, what it does.
        /// </summary>
        /// <returns>short description</returns>
        public string About => "Contains set of quick responses from easily extendable file.";

        /// <summary>
        /// Returns count of kept retorts.
        /// </summary>
        /// <returns>Count of retorts</returns>
        public int Count => _retorts != null ? _retorts.Count : 0;
    }
}
