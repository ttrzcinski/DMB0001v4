using DMB0001v4.Providers;
using DMB0001v4.Resources;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace DMB0001v4.Mind
{
    /// <summary>
    /// Keeps common methods and forms used in conversation, so basic dialog logic wouldn't be so long.
    /// </summary>
    public class DialogUtils
    {
        /// <summary>
        /// State of currently remembered facts and knowledge.
        /// </summary>
        private BrainState _state;

        private static Dictionary<string, string> _responses;

        /// <summary>
        /// Creates new instance of Utils for Dialogs.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provider for passing the state from context</param>>
        public DialogUtils(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            _state = conversationStateProvider.GetConversationState<BrainState>(context);
            //
            initLocalizedAnswers();
        }

        private void initLocalizedAnswers()
        {
            if (_responses == null)
            {
                _responses = new Dictionary<string, string>();
                try
                {
                    _responses.Add("response_greet_hello", Phrases.response_greet_hello);
                    _responses.Add("response_greet_weve", Phrases.response_greet_weve);
                    _responses.Add("response_bye_goodbye", Phrases.response_bye_goodbye);
                    _responses.Add("response_bye_weve", Phrases.response_bye_weve);
                }
                catch (Exception exception_1)
                {
                    Console.WriteLine("Couldn't obtain Phrases - used defaults from EN.");
                    _responses.Add("response_greet_hello", "Hello You..");
                    _responses.Add("response_greet_weve", "We've already greet before..");
                    _responses.Add("response_bye_goodbye", "Goodbye.");
                    _responses.Add("response_bye_weve", "We've already said goodbye..");
                }
            }
        }

        /// <summary>
        /// Greets user after being greeted by user.
        /// </summary>
        /// <returns>Greeting</returns>
        public string Greeting()
        {
            if (_state == null)
            {
                throw new ArgumentNullException("_state is null.");
            }
            var response = _state.SaidHi == false || (_state.SaidByeAfter == true && _state.SaidHi == true)
                ? _responses["response_greet_hello"]
                : _responses["response_greet_weve"];
            if (_state.SaidHi == false)
            {
                _state.SaidHi = true;
                _state.SaidByeAfter = false;
            }
            return response;
        }

        /// <summary>
        /// Returns a valediction to user after finishing conversation.
        /// </summary>
        /// <returns>Valediction</returns>
        public string Valediction()
        {
            if (_state == null)
            {
                throw new ArgumentNullException("_state is null.");
            }
            var response = _state.SaidHi == false || (_state.SaidByeAfter == true && _state.SaidHi == true)
                ? _responses["response_bye_goodbye"] //Phrases..response_bye_goodbye
                : _responses["response_bye_weve"]; //Phrases.response_bye_weve;
            if (_state.SaidHi == false)
            {
                _state.SaidHi = true;
                _state.SaidByeAfter = false;
            }
            return response;
        }

        /// <summary>
        /// Lets keep in mind currently asked question.
        /// </summary>
        /// <param name="question">Content of the question</param>
        /// <param name="answers">Possible answers (currently only 2 are usable)</param>
        /// <param name="responses">Possible responses (currently only 2 are usable)</param>
        /// <returns>Question as a prompt</returns>
        public string Question(string question, string[] answers = null, string[] responses = null) 
            => TheQuestion(question, answers, responses);

        /// <summary>
        /// Inner call for method Question in order to keep param names simpler.
        /// </summary>
        /// <param name="theQuestion">Content of the question</param>
        /// <param name="theAnswers">Possible answers (currently only 2 are usable)</param>
        /// <param name="theResponses">Possible responses (currently only 2 are usable)</param>
        /// <returns>Question as a prompt</returns>
        private string TheQuestion(string theQuestion, string[] theAnswers, string[] theResponses)
        {
            // Process null parameters to not nulls
            theAnswers = theAnswers ?? new[] {Phrases.btn_quest_yes, Phrases.btn_quest_no};
            theResponses = theResponses ?? new[] {Phrases.response_after_good, Phrases.response_after_ididnt};
            // Keep question in BrainState
            var question1 = new Ask
            {
                // TODO Generate id maybe
                Id = 1, 
                Question = theQuestion,
                Answers = theAnswers,
                Responses = theResponses
            };
            _state.RisenQuestion = question1;
            // Prepare first response
            var stringBuilder = new StringBuilder(question1.Question);
            for (var i = 0; i < question1.Answers.Length; i++)
                stringBuilder.Append($"\n\t{i + 1}) {question1.Answers[i]}");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Passes user's answer to currently risen question.
        /// </summary>
        /// <param name="answer">user's answer</param>
        /// <returns>response to choosen answer</returns>
        public string Answer(string answer)
        {
            var response = Phrases.response_after_noquestion;
            // Check, if question was risen
            if (_state.RisenQuestion == null) return response;
            // Fix param to not null
            answer = (answer ?? "").Trim();
            // Check, if responses are delivered
            if (!string.IsNullOrEmpty(answer))
                for (var i = 0; i < _state.RisenQuestion.Answers.Length; i++)
                {
                    var possibleAnswer = _state.RisenQuestion.Answers[i].ToLower();
                    if (possibleAnswer != answer) continue;
                    // TODO Fix to use all the given answers
                    response = _state.RisenQuestion.Responses[i == 0 ? 0 : 1];
                    // TODO Add processing of an answer in knowledge - don't ask twice about the same
                    _state.RisenQuestion = null;
                    break;
                }
            else
                response = Phrases.response_after_noanswer;
            return response;
        }

        /// <summary>
        /// Shows information about the author of this bot.
        /// </summary>
        /// <returns>activity with infomration about author</returns>
        public IMessageActivity Author 
            => MessageFactory.Attachment(new Attachment[]
            {
                new Attachment { ContentUrl = "https://avatars2.githubusercontent.com/u/12435750?s=460&v=4", ContentType = "image/jpg" }
            }, text: "Tomasz Trzciński <trzcinski.tomasz.1988@gmail.com>");
    }
}
