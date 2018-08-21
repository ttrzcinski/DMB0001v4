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
                "We've already said Good Bye=..";
            if (_state.SaidHi == false)
            {
                _state.SaidHi = true;
                _state.SaidByeAfter = false;
            }
            return response;
        }

        public string Question(string question, string[] answers = null, string[] responses = null)
        {
            return TheQuestion(question, answers, responses);
        }

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

        public string Answer(string answer)
        {
            // TODO Check, if question was risen

            // Fix param to not null
            answer = (answer ?? "").Trim();
            //
            // TODO check, if responses are delivered
            var response = "..Some error just happen..";
            if (!string.IsNullOrEmpty(answer))
            {
                for (int i = 0; i < _state.RisenQuestion.answers.Length; i++)
                {
                    var possibleAnswer = _state.RisenQuestion.answers[i].ToLower();
                    if (possibleAnswer == answer)
                    {
                        response = _state.RisenQuestion.responses[i == 0 ? 0 : 1];
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
