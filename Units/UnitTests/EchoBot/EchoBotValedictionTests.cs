﻿using System.Threading.Tasks;
using DMB0001v4;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;

namespace Units.UnitTests.EchoBot
{
    public class EchoBotValedictionTests
    {       
        private const string StrQuestFarewell  = "Farewell";
        private const string StrQuestBye = "Bye";
        private const string StrQuestGoodbye = "Goodbye";

        private const string StrAnswerGoodbye = "Goodbye";
        private const string StrAnswerWeVeAlready = "We've already said goodbye..";
        
        private readonly Mock<IConversationStateProvider> _conversationStateProviderMock;
        private readonly Mock<ITurnContext> _turnContextMock;
        private readonly DMB0001v4.EchoBot _echoBot;

        public EchoBotValedictionTests()
        {
            //EchoBot
            _conversationStateProviderMock = new Mock<IConversationStateProvider>();
            _turnContextMock = new Mock<ITurnContext>();
            _echoBot = new DMB0001v4.EchoBot(_conversationStateProviderMock.Object);
        }

        [Theory]
        [InlineData(StrQuestGoodbye, StrAnswerGoodbye)]
        [InlineData(StrQuestBye, StrAnswerGoodbye)]
        [InlineData(StrQuestFarewell, StrAnswerGoodbye)]
        public async Task OnTurn_Valediction_Async_Once_Test(string request, string expectedResponse)
        {
            // Arrange
            var activity = new Activity { Type = ActivityTypes.Message, Text = request };
            var brainState = new BrainState();

            _turnContextMock.Setup(p => p.Activity).Returns(activity);
            _conversationStateProviderMock.Setup(p => p.GetConversationState<BrainState>(_turnContextMock.Object))
                .Returns(brainState);

            // Act
            await _echoBot.OnTurn(_turnContextMock.Object);

            // Assert
            _turnContextMock.Verify(p => p.Activity, Times.AtLeastOnce);
            _turnContextMock.Verify(p => p.SendActivity(expectedResponse, null, null), Times.Never);
        }

        [Theory]
        [InlineData(StrQuestGoodbye, StrAnswerGoodbye, StrQuestGoodbye, StrAnswerWeVeAlready)]
        [InlineData(StrQuestGoodbye, StrAnswerGoodbye, StrQuestBye, StrAnswerWeVeAlready)]
        [InlineData(StrQuestGoodbye, StrAnswerGoodbye, StrQuestFarewell, StrAnswerWeVeAlready)]
        [InlineData(StrQuestBye, StrAnswerGoodbye, StrQuestGoodbye, StrAnswerWeVeAlready)]
        [InlineData(StrQuestBye, StrAnswerGoodbye, StrQuestBye, StrAnswerWeVeAlready)]
        [InlineData(StrQuestBye, StrAnswerGoodbye, StrQuestFarewell, StrAnswerWeVeAlready)]
        [InlineData(StrQuestFarewell, StrAnswerGoodbye, StrQuestGoodbye, StrAnswerWeVeAlready)]
        [InlineData(StrQuestFarewell, StrAnswerGoodbye, StrQuestBye, StrAnswerWeVeAlready)]
        [InlineData(StrQuestFarewell, StrAnswerGoodbye, StrQuestFarewell, StrAnswerWeVeAlready)]
        public async Task OnTurn_Valediction_Async_Twice_Test(string request1, string expectedResponse1, string request2, string expectedResponse2)
        {
            // Arrange
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
            turnContextMock1.Verify(p => p.SendActivity(expectedResponse1, null, null), Times.Never);
            turnContextMock2.Verify(p => p.Activity, Times.AtLeastOnce);
            turnContextMock2.Verify(p => p.SendActivity(expectedResponse2, null, null), Times.AtMostOnce);
        }
    }
}
