using DMB0001v4.Mind;
using DMB0001v4;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Moq;
using Xunit;

namespace Units.UnitTests.Mind
{
    public class SystemUtilsTests
    {
        private readonly Mock<IConversationStateProvider> _provider;
        private readonly Mock<ITurnContext> _context;
  
        private readonly SystemUtils _utils;

        public SystemUtilsTests()
        {
            _provider = new Mock<IConversationStateProvider>();
            _context = new Mock<ITurnContext>();
            //var brainState = new BrainState();
            //_provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
            //    .Returns(brainState);
            _utils = new SystemUtils(_context.Object, _provider.Object);
        }

        // Cover Project path
        [Fact]
        public void ProjectPathTest()
        {
            // Arrange
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            var actual = _utils.ProjectPath();

            // Assert
            Assert.NotNull(actual);
        }

        // Cover User Name
        [Fact]
        public void UserNameTest()
        {
            // Arrange
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            var actual = _utils.UserName();

            // Assert
            Assert.NotNull(actual);
        }
    }
}
