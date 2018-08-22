using System.Threading.Tasks;
using DMB0001v4.Mind;
using DMB0001v4.Providers;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace DMB0001v4
{
    public class EchoBot : IBot
    {
        private readonly IConversationStateProvider _conversationStateProvider;

        public EchoBot(IConversationStateProvider conversationStateProvider)
        {
            _conversationStateProvider = conversationStateProvider;
        }

        /// <summary>
        /// Every Conversation turn for our EchoBot will call this method. In here
        /// the bot checks the Activity type to verify it's a message, bumps the 
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
                var state = _conversationStateProvider.GetConversationState<BrainState>(context);

                // Create new DialogUtils to hide logic in sub-methods
                var dialogUtils = new DialogUtils(context, _conversationStateProvider);

                // Bump the turn count. 
                state.TurnCount++;

                // Prepare lowercase user's request
                var lowText = context.Activity.Text.ToLower();

                // Prepare response - move to separate class
                string responseText;
                // TODO - if ever speech will be on - string responseSpeak = null;
                // Check, if question was asked
                if (state.RisenQuestion != null)
                {
                    responseText = dialogUtils.Answer(lowText);
                    await context.SendActivity(responseText);
                    return;
                }

                switch (lowText)
                {
                    case "where are you?"://ResourceManager.GetString("request_where_are_you").//
                        responseText = new SystemUtils(context, _conversationStateProvider).ProjectPath();
                        break;

                    case "what is your name?":
                        responseText = state.BotsName;
                        break;

                    case "what is my name?":
                        responseText = state.UsersName;
                        break;

                    case "read my name":
                        responseText = new SystemUtils(context, _conversationStateProvider).UserName();
                        break;

                    case "show me net pic":
                        var activity = MessageFactory.Attachment(
                            new HeroCard(
                                    title: "Some internet image",
                                    images: new CardImage[] { new CardImage(url: "https://www.google.pl/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png") },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "buy", type: ActionTypes.ImBack, value: "buy")
                                    })
                                .ToAttachment());
                        responseText = "Is there an image?";
                        await context.SendActivity(activity);
                        break;

                    case "show me local pic":
                        var activity2 = MessageFactory.Attachment(new Attachment[]
                        {
                            //If file is there, it works
                            new Attachment { ContentUrl = "c:\\warn.jpg", ContentType = "image/jpg" }
                            // TODO Change to relative path
                            //new Attachment { ContentUrl = $"{new SystemUtils(context, _conversationStateProvider).ProjectPath()}imgs\\small-image.png", ContentType = "image/png" }//,
                        });
                        responseText = "Is there an image?";
                        await context.SendActivity(activity2);
                        break;

                    case "hi":
                        responseText = dialogUtils.Greeting();
                        break;

                    case "hello":
                        responseText = dialogUtils.Greeting();
                        break;

                    case "welcome":
                        responseText = dialogUtils.Greeting();
                        break;

                    case "bye":
                        responseText = dialogUtils.Valediction();
                        break;

                    case "goodbye":
                        responseText = dialogUtils.Valediction();
                        break;

                    case "farewell":
                        responseText = dialogUtils.Valediction();
                        break;

                    case "pancakes?":
                        responseText = dialogUtils.Question("Do you like pancakes?");
                        break;

                    case "reset":
                        responseText = dialogUtils.Question("Do you want to reset counter?",
                            null,
                            new[] { "Counter reset..", "I didn't get that.. so?" });
                        break;

                    default:
                        responseText = $"Turn {state.TurnCount}: I didn't get that, you said: '{context.Activity.Text}'";
                        // TODO save the command in storage or some resource DB
                        break;
                }

                // Echo back to the user whatever they typed - responseSpeak, if not null, will be read to user
                await context.SendActivity(responseText, null, null);//, "acceptingInput");
            }
        }
    }
}
