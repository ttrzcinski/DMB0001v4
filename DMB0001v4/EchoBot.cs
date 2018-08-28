using System;
using System.Linq;
using System.Threading.Tasks;
using DMB0001v4.Mind;
using DMB0001v4.Providers;
using DMB0001v4.Skills;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace DMB0001v4
{
    public class EchoBot : IBot
    {
        /// <summary>
        /// Keeps in one place all known skills.
        /// </summary>
        private SkillFactory _skills;

        private static BrainState _state;

        private readonly IConversationStateProvider _conversationStateProvider;

        public EchoBot(IConversationStateProvider conversationStateProvider) => _conversationStateProvider = conversationStateProvider;

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
            /*if (imageAttachment != null)
            {
                using (var stream = await GetImageStream(connector, imageAttachment))
                {
                    return await this.captionService.GetCaptionAsync(stream);
                }
            }*/

            // This bot is only handling Messages
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Get the conversation state from the turn context
                if (_state == null)
                    _state = _conversationStateProvider.GetConversationState<BrainState>(context);

                // Create new DialogUtils to hide logic in sub-methods
                var dialogUtils = new DialogUtils(context, _conversationStateProvider);
                // Bump the turn count. 
                _state.TurnCount++;
                // Prepare lowercase user's request
                var lowText = context.Activity.Text.ToLower();

                // Prepare response - move to separate class
                string responseText = null;
                // TODO - if ever speech will be on - changestring responseSpeak = null;
                string responseSpeak = null;
                IMessageActivity responseActivity = null;//IActivity
                // Check, if question was asked
                if (_state.RisenQuestion != null)
                {
                    responseText = dialogUtils.Answer(lowText);
                    await context.SendActivity(responseText);
                    return;
                }

                // Check, if phraase can be processed by known skills
                // Init skills factory to call to through common merthods
                _skills = SkillFactory.GetInstance(context, _conversationStateProvider);
                responseText = _skills.Process(context.Activity.Text);
                if (responseText != null)
                {
                    await context.SendActivity(responseText, null, null);
                    return;
                }

                // Check, if it's an image
                if (context.Activity.Attachments != null && context.Activity.Attachments.Any())
                {
                    var imageAttachment = context.Activity.Attachments?.FirstOrDefault(a => a.ContentType.Contains("image"));
                    if (imageAttachment != null)
                    {
                        responseText = "An image.. what I suppose to do with that?";
                        var attachmentUrl = context.Activity.Attachments[0].ContentUrl;
                        //var attachmentData = context.Activity.Attachments[0].Content;
                        //var attachmentType = context.Activity.Attachments[0].ContentType;
                        //if (!string.IsNullOrEmpty(attachmentUrl))
                        //{
                        //    var httpClient = new HttpClient();
                        //    attachmentData = await httpClient.GetByteArrayAsync(attachmentUrl);
                        //}
                        await context.SendActivity(responseText, null, null);
                        return;
                    }
                }

                switch (lowText)
                {
                    case "how old are you?":
                        DateTime dob = new DateTime(2018, 08, 28, 09, 24, 00);
                        DateTime now = DateTime.Now;
                        var days = Math.Ceiling(dob.Subtract(now).TotalDays);
                        responseText = $"I'm {days} days old since {dob.ToString()}.";//21.08.2018 09.24
                        break;

                    case "where are you?":
                        responseText = new SystemUtils(context, _conversationStateProvider).ProjectPath();
                        break;

                    case "who are you?":
                        responseText = _state.BotsName;
                        break;

                    case "what is your name?":
                        responseText = _state.BotsName;
                        break;

                    case "who made you?":
                        responseActivity = dialogUtils.Author;
                        break;

                    case "who created you?":
                        responseActivity = dialogUtils.Author;
                        break;

                    case "who wrote you?":
                        responseActivity = dialogUtils.Author;
                        break;

                    case "what is my name?":
                        responseText = _state.UsersName;
                        break;

                    case "read my name":
                        responseText = new SystemUtils(context, _conversationStateProvider).UserName();
                        break;

                    case "show me a hero card":
                        var activity = MessageFactory.Attachment(
                            new HeroCard(
                                    title: "Some internet image",
                                    images: new CardImage[]
                                    {
                                        new CardImage(url: "https://www.google.pl/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png")
                                    },
                                    buttons: new CardAction[]
                                    {
                                        new CardAction(title: "buy", type: ActionTypes.ImBack, value: "buy")
                                    })
                                .ToAttachment());
                        responseText = "Is there an image?";
                        await context.SendActivity(activity);
                        break;

                    case "show me net pic":
                        var activity22 = MessageFactory.Attachment(new Attachment[]
                        {
                            new Attachment { ContentUrl = "https://avatars2.githubusercontent.com/u/12435750?s=460&v=4", ContentType = "image/jpg" }
                        });
                        responseText = "Is there an image?";
                        await context.SendActivity(activity22);
                        break;

                    case "show me local pic":
                        var activity3 = MessageFactory.Attachment(new Attachment[]
                        {
                            // If file is there, it works
                            new Attachment { ContentUrl = "c:\\warn.jpg", ContentType = "image/jpg" }
                            // TODO Change to relative path
                            //new Attachment { ContentUrl = $"{new SystemUtils(context, _conversationStateProvider).ProjectPath()}imgs\\small-image.png", ContentType = "image/png" }//,
                        });
                        responseText = "Is there an image?";
                        await context.SendActivity(activity3);
                        break;

                    case "pancakes?":
                        responseText = dialogUtils.Question("Do you like pancakes?");
                        break;

                    case "do you like pancakes??":
                        responseText = dialogUtils.Question("Do you like pancakes?");
                        break;

                    case "reset":
                        responseText = dialogUtils.Question("Do you want to reset counter?",
                            null,
                            new[] { "Counter reset..", "I didn't get that.. so?" });
                        break;

                    case "reset counter":
                        responseText = dialogUtils.Question("Do you want to reset counter?",
                            null,
                            new[] { "Counter reset..", "I didn't get that.. so?" });
                        break;

                    case "can you reset counter?":
                        responseText = dialogUtils.Question("Do you want to reset counter?",
                            null,
                            new[] { "Counter reset..", "I didn't get that.. so?" });
                        break;

                    default:
                        // Add it to unknowns
                        var unknowns = Unknowns.Instance(context, _conversationStateProvider);

                        responseText = unknowns.Process(context.Activity.Text);
                        // Mark error on adding unknown
                        if (responseText == null)
                        {
                            // TODO Add here use (with random) of list of 10 possible 'i dun't know's
                            // Presents - 'i dun't know' answer
                            responseText =
                                $"Turn {_state.TurnCount}: I didn't get that, you said: '{context.Activity.Text}'";
                        }
                        else
                        {
                            // After adding nex unknown presents added stack lines
                            responseText = unknowns.AsStackLines();
                        }
                        break;
                }

                // Echo back to the user whatever they typed - responseSpeak, if not null, will be read to user
                if (responseActivity != null)
                {
                    await context.SendActivity(responseActivity);
                }
                if (responseText != null) {
                    await context.SendActivity(responseText, responseSpeak, null);//, "acceptingInput");
                }
            }
        }
    }
}
