using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using System.Text;

namespace DMB0001v4.Mind
{
    /// <summary>
    /// Keeps common methods and forms used in conversation, so basic dialog logic wouldn't be so long.
    /// </summary>
    public class DialogUtils
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
        /// Creates new instance of Utils for Dialogs.
        /// </summary>
        /// <param name="context">current dialog context</param>
        /// <param name="conversationStateProvider">provder for passing the state from context</param>>
        public DialogUtils(ITurnContext context, IConversationStateProvider conversationStateProvider)
        {
            _context = context; // TODO Maybe remove it from class variables - state is the only important one
            _state = conversationStateProvider.GetConversationState<BrainState>(context);
        }

        /// <summary>
        /// Greets user after being greeted by user.
        /// </summary>
        /// <returns>Greeting</returns>
        public string Greeting()
        {
            var response = _state.SaidHi == false || (_state.SaidByeAfter == true && _state.SaidHi == true) ?
                "Hello You.." :
                "We've already greet before..";
            if (_state.SaidHi == false)
            {
                _state.SaidHi = true;
                _state.SaidByeAfter = false;
            }
            return response;
        }

        /// <summary>
        /// Sais a benediction to user after finishing conversation.
        /// </summary>
        /// <returns>Benediction</returns>
        public string Benediction()
        {
            var response = _state.SaidHi == false || (_state.SaidByeAfter == true && _state.SaidHi == true) ?
                "Hello You.." :
                "We've already said Good Bye..";
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
        {
            return TheQuestion(question, answers, responses);
        }

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
            theAnswers = theAnswers ?? new string[] {"Yes", "No"};
            theResponses = theResponses ?? new string[] {"Good to know..", "I didn't get that.. so?"};
            // Keep question in BrainState
            var question1 = new Question
            {
                // TODO Generate it maybe
                id = 1, 
                question = theQuestion,
                answers = theAnswers,
                responses = theResponses
            };
            _state.RisenQuestion = question1;
            // Prepare first response
            StringBuilder stringBuilder = new StringBuilder(question1.question);
            for (int i = 0; i < question1.answers.Length; i++)
            {
                var answer = question1.answers[i];
                stringBuilder.Append($"\n\t{i + 1}) {answer}");
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Given answer to currently risen question.
        /// </summary>
        /// <param name="answer">given answer</param>
        /// <returns></returns>
        public string Answer(string answer)
        {
            var response = "There was no question asked.";
            // Check, if question was risen
            if (_state.RisenQuestion == null)
            {
                return response;
            }

            // Fix param to not null
            answer = (answer ?? "").Trim();
            //
            // TODO check, if responses are delivered
            if (!string.IsNullOrEmpty(answer))
            {
                for (int i = 0; i < _state.RisenQuestion.answers.Length; i++)
                {
                    var possibleAnswer = _state.RisenQuestion.answers[i].ToLower();
                    if (possibleAnswer == answer)
                    {
                        // TODO Fix to use all the given answers
                        response = _state.RisenQuestion.responses[i == 0 ? 0 : 1];
                        // TODO Add processing of an answer in knowledge - don't ask twice about the same
                        //_state.RisenQuestion.Processed = true;
                        _state.RisenQuestion = null;
                        break;
                    }
                }
            }
            else
            {
                response = "No answer is not an answer in that case..";
            }
            return response;
        }
    }
}
