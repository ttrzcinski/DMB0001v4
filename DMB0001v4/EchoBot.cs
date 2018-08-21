using System.Text;
using System.Threading.Tasks;
using DMB0001v4.Mind;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace DMB0001v4
{
    public class EchoBot : IBot
    {
        /// <summary>
        /// Every Conversation turn for our EchoBot will call this method. In here
        /// the bot checks the Activty type to verify it's a message, bumps the 
        /// turn conversation 'Turn' count, and then echoes the users typing
        /// back to them. 
        /// </summary>
        /// <param name="context">Turn scoped context containing all the data needed
        /// for processing this conversation turn. </param>        
        public async Task OnTurn(ITurnContext context)
        {
            // This bot is only handling Messages
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Get the conversation state from the turn context
                var state = context.GetConversationState<BrainState>();

                // Create new DialogUtils to hide logic in submethods
                var dialogUtils = new DialogUtils(context);

                // Bump the turn count. 
                state.TurnCount++;

                // Prepare lowercase user's request
                var lowText = context.Activity.Text.ToLower();

                // Prepare response - move to separate class
                var response = "..";

                switch (lowText)
                {
                    case "hi":
                        response = state.SaidHi == false || (state.SaidByeAfter == true && state.SaidHi == true) ? 
                            "Hello You.." : 
                            "We've already greet before..";
                        if (state.SaidHi == false) {
                            state.SaidHi = true;
                            state.SaidByeAfter = false;
                        }
                        break;

                    case "hello":
                        response = state.SaidHi == false || (state.SaidByeAfter == true && state.SaidHi == true) ?
                            "Hello You.." :
                            "We've already greet before..";
                        if (state.SaidHi == false)
                        {
                            state.SaidHi = true;
                            state.SaidByeAfter = false;
                        }
                        break;

                    case "bye":
                        response = (state.SaidByeAfter == false && state.SaidHi == true) || state.SaidByeAfter == true ?
                            "Bye to You.." :
                            "We've already said bye..";
                        if (state.SaidByeAfter == false)
                        {
                            state.SaidHi = false;
                            state.SaidByeAfter = false;
                        }
                        break;

                    case "pancakes?":
                        // Prepare first response = "Do you like pancakces\n1) Yes\n2) No";
                        response = dialogUtils.Question("Do you like pancakes?");
                        // TODO RAISE FLAG OF QUESTIONS - NEXT SENT REQUEST WILL CONTAIN THE ANSWER
                        break;

                    case "reset":
                        //response = "Do you want to reset counter\n1) Yes\n2) No";
                        response = dialogUtils.Question("Do you want to reset counter?", null, new string[] { "Counter reset..", "I didn't get that.. so?"});
                        // TODO RAISE FLAG OF QUESTIONS - NEXT SENT REQUEST WILL CONTAIN THE ANSWER
                        break;

                    default:
                        response = $"Turn {state.TurnCount}: I didn't get that, you said: '{context.Activity.Text}'";
                        // TODO save the command in storage or some resource DB
                        break;
                }

                // Echo back to the user whatever they typed.
                await context.SendActivity(response);
            }
        }
    }
}
