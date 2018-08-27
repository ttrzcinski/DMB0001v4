using DMB0001v4.Mind;
using DMB0001v4;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Moq;
using Xunit;
using Microsoft.Bot.Schema;

namespace Units.UnitTests.Mind
{
    public class DialogUtilsTests
    {
        private readonly Mock<IConversationStateProvider> _provider;
        private readonly Mock<ITurnContext> _context;

        private readonly DialogUtils _utils;

        public DialogUtilsTests()
        {
            _provider = new Mock<IConversationStateProvider>();
            _context = new Mock<ITurnContext>();
            //var brainState = new BrainState();
            //_provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
            //    .Returns(brainState);
            _utils = new DialogUtils(_context.Object, _provider.Object);
        }

        // Cover Author
        [Fact]
        public void AuthorNotNullTest()
        {
            // Arrange
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            var actual = _utils.Author;

            // Assert
            Assert.NotNull(actual);
        }

        [Fact]
        public void AuthorHasRightTypeTest()
        {
            // Arrange
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            var actual = _utils.Author;

            // Assert
            Assert.IsType<Activity>(actual);
        }

        [Theory]
        [InlineData("Tomasz Trzciński <trzcinski.tomasz.1988@gmail.com>")]
        public void AuthorHasTextTest(string author)
        {
            // Arrange
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            var actual = _utils.Author;

            // Assert
            Assert.Equal(author, actual.Text);
        }

        [Fact]
        public void AuthorHasAttachementTest()
        {
            // Arrange
            var url = "https://avatars2.githubusercontent.com/u/12435750?s=460&v=4";
            var contentType = "image/jpg";
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            var actual = _utils.Author;

            // Assert
            Assert.NotNull(actual.Attachments[0]);
            Assert.Equal(contentType, actual.Attachments[0].ContentType);
            Assert.Equal(url, actual.Attachments[0].ContentUrl);
        }
    }
}
