using DMB0001v4;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using DMB0001v4.Providers;

namespace Units
{
    public class EchoBotValedictionTests
    {
        private readonly Mock<IConversationStateProvider> _conversationStateProviderMock;
        private readonly Mock<ITurnContext> _turnContextMock;
        private readonly EchoBot _echoBot;

        public EchoBotValedictionTests()
        {
            //EchoBot
            _conversationStateProviderMock = new Mock<IConversationStateProvider>();
            _turnContextMock = new Mock<ITurnContext>();
            _echoBot = new EchoBot(_conversationStateProviderMock.Object);
        }

        [Theory]
        [InlineData("Bye", "Goodbye.")]
        [InlineData("Goodbye", "Goodbye.")]
        [InlineData("Farewell", "Goodbye.")]
        public async Task OnTurn_Valediction_Async_Once_Test(string request, string expectedResponse)
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = request };
            var dataStore = new MemoryStorage();
            var brainState = new BrainState();

            _turnContextMock.Setup(p => p.Activity).Returns(activity);
            _conversationStateProviderMock.Setup(p => p.GetConversationState<BrainState>(_turnContextMock.Object))
                .Returns(brainState);

            // Act
            await _echoBot.OnTurn(_turnContextMock.Object);

            // Assert
            _turnContextMock.Verify(p => p.Activity, Times.AtLeastOnce);
            _turnContextMock.Verify(p => p.SendActivity(expectedResponse, null, null), Times.Once);
        }

        [Theory]
        [InlineData("Bye", "Goodbye.", "Bye", "We've already said goodbye..")]
        [InlineData("Bye", "Goodbye.", "Goodbye", "We've already said goodbye..")]
        [InlineData("Bye", "Goodbye.", "Farewell", "We've already said goodbye..")]
        [InlineData("Goodbye", "Goodbye.", "Bye", "We've already said goodbye..")]
        [InlineData("Goodbye", "Goodbye.", "Goodbye", "We've already said goodbye..")]
        [InlineData("Goodbye", "Goodbye.", "Farewell", "We've already said goodbye..")]
        [InlineData("Farewell", "Goodbye.", "Bye", "We've already said goodbye..")]
        [InlineData("Farewell", "Goodbye.", "Goodbye", "We've already said goodbye..")]
        [InlineData("Farewell", "Goodbye.", "Farewell", "We've already said goodbye..")]
        public async Task OnTurn_Valediction_Async_Twice_Test(string request1, string expectedResponse1, string request2, string expectedResponse2)
        {
            // Arrange
            var dataStore = new MemoryStorage();
            var brainState = new BrainState();

            var turnContextMock1 = new Mock<ITurnContext>();
            var activity1 = new Activity { Type = ActivityTypes.Message, Text = request1 };
            turnContextMock1.Setup(p => p.Activity).Returns(activity1);
            _conversationStateProviderMock.Setup(p => p.GetConversationState<BrainState>(turnContextMock1.Object))
                .Returns(brainState);

            var turnContextMock2 = new Mock<ITurnContext>();
            var activity2 = new Activity { Type = ActivityTypes.Message, Text = request2 };
            turnContextMock2.Setup(p => p.Activity).Returns(activity2);
            _conversationStateProviderMock.Setup(p => p.GetConversationState<BrainState>(turnContextMock2.Object))
                .Returns(brainState);

            // Act
            await _echoBot.OnTurn(turnContextMock1.Object);
            await _echoBot.OnTurn(turnContextMock2.Object);

            // Assert
            turnContextMock1.Verify(p => p.Activity, Times.AtLeastOnce);
            turnContextMock1.Verify(p => p.SendActivity(expectedResponse1, null, null), Times.Once);
            turnContextMock2.Verify(p => p.Activity, Times.AtLeastOnce);
            turnContextMock2.Verify(p => p.SendActivity(expectedResponse2, null, null), Times.Once);
        }
    }
}
