using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using System.Text;

namespace DMB0001v4.Mind
{
    public class DialogUtils
    {
        private ITurnContext _context;
        private BrainState _state;

        public DialogUtils(ITurnContext context)
        {
            _context = context;
            _state = context.GetConversationState<BrainState>();
        }

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

        public string Benediction()
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

        public string Question(string question, string[] answers, string[] responses)
        {
            return TheQuestion(question, answers, responses);
        }

        private string TheQuestion(string theQuestion, string[] theAnswers, string[] theResponses)
        {
            // Process null parameters to not nulls
            theAnswers = theAnswers ?? new string[] {"Yes", "No"};
            theResponses = theResponses ?? new string[] {"Good to know..", "I didn't get that.. so?"};

            //response = "Do you like pancakces\n1) Yes\n2) No";
            _state.RisenQuestion = true;
            var question1 = new Question
            {
                // TODO Generate it maybe
                id = 1, 
                question = theQuestion,
                answers = theAnswers,
                responses = theResponses
            };
            // Prepare first response
            StringBuilder stringBuilder = new StringBuilder(question1.question);
            for (int i = 0; i < question1.answers.Length; i++)
            {
                var answer = question1.answers[i];
                stringBuilder.Append($"\n\t{i + 1}) {answer}");
            }
            // TODO RAISE FLAG OF QUESTIONS - NEXT SENT REQUEST WILL CONTAIN THE ANSWER
            return stringBuilder.ToString();
        }
    }
}
