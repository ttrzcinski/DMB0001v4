using DMB0001v4.Skills;
using DMB0001v4.Providers;
using Microsoft.Bot.Builder;
using Moq;
using Xunit;
using DMB0001v4;

namespace Units.UnitTests.Skills
{
    public class RetortsTests
    {
        private readonly Mock<IConversationStateProvider> _provider;
        private readonly Mock<ITurnContext> _context;

        public RetortsTests()
        {
            _provider = new Mock<IConversationStateProvider>();
            _context = new Mock<ITurnContext>();
        }

        private void ArrangeByDefault()
        {
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);
            var retorts = Retorts.Instance(_context.Object, _provider.Object);
            Retorts.ReadOnlyFile = true;
        }

        // Cover get instance, if there was none
        [Fact]
        public void GetInstanceTest()
        {
            // Arrange
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);
            var retorts = Retorts.Instance(_context.Object, _provider.Object);

            // Act

            // Assert
            Assert.NotNull(retorts);
        }

        // Cover obtaining the retorts skill
        [Fact]
        public void GetInstanceThroughFactoryTest()
        {
            // Arrange
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);
            var retorts = SkillFactory.GetInstance().GetSkill("retorts",_context.Object, _provider.Object);

            // Act

            // Assert
            Assert.NotNull(retorts);
        }

        // Cover About
        [Fact]
        public void AboutTest()
        {
            // Arrange
            var expected = "Contains set of quick responses from easily extendable file.";
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);
            var retorts = SkillFactory.GetInstance().GetSkill("retorts", _context.Object, _provider.Object);

            // Act
            var actual = retorts.About;

            // Assert
            Assert.NotNull(actual);
            Assert.NotEmpty(actual);
            Assert.Equal(expected, actual);
        }

        // TODO Cover Processing existing element
        [Theory]
        [InlineData("hi", "hello")]
        [InlineData("wth", null)]
        public void ProcessTest(string request, string expected)
        {
            // Arrange
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);
            var skills = SkillFactory.GetInstance(_context.Object, _provider.Object);
            var retorts = SkillFactory.GetInstance().GetSkill("retorts", _context.Object, _provider.Object);
            // TODO add call to load retorts from file
            var resultOfAdd = Retorts.Add("hi", "hello");

            // Act
            var actual = retorts.Process(request);
            Retorts.Remove("hi");

            // Assert
            Assert.Equal(expected, actual);
        }

        // Cover Clear on empty
        // Cover Processing non-existing element
        [Fact]
        public void ClearEmptyTest()
        {
            // Arrange
            ArrangeByDefault();

            // Act
            Retorts.Clear();

            // Assert
            Assert.True(Retorts.IsEmpty());
        }

        // TODO Cover Clear on full
        [Fact]
        public void ClearFullTest()
        {
            // Arrange
            ArrangeByDefault();
            // TODO add call to load retorts from file

            // Act
            Retorts.Clear();

            // Assert
            Assert.True(Retorts.IsEmpty());
        }

        // Cover Count after init
        [Fact]
        public void ClearCountAfterInitTest()
        {
            // Arrange
            var expected = 0;
            ArrangeByDefault();

            // Act
            var actual = Retorts.GetCount();

            // Assert
            Assert.Equal(expected, actual);
        }

        // TODO Cover Count after load
        [Fact]
        public void ClearCountAfterLoadTest()
        {
            // Arrange
            ArrangeByDefault();

            // Act
            var actual = Retorts.GetCount();

            // Assert
            Assert.True(actual > 0);
        }

        // COVER ADD null, empty, wrong with empty key, wrong with empty value, proper with non-existing key, proper with existing key
        [Theory]
        [InlineData(null, null, false)]
        [InlineData(null, "", false)]
        [InlineData(null, "  ", false)]
        [InlineData(null, "val1", false)]
        [InlineData("", "", false)]
        [InlineData("", null, false)]
        [InlineData("", "  ", false)]
        [InlineData("", "val1", false)]
        [InlineData("  ", null, false)]
        [InlineData("  ", "", false)]
        [InlineData("  ", "  ", false)]
        [InlineData("  ", "val1", false)]
        [InlineData("key1", null, false)]
        [InlineData("key1", "", false)]
        [InlineData("key1", "  ", false)]
        [InlineData("key1", "val1", true)]
        public void AddTest(string given_key, string given_value, bool expected)
        {
            // Arrange
            ArrangeByDefault();

            // Act
            Retorts.Clear();
            var actual = Retorts.Add(given_key, given_value);
            Retorts.Clear();

            // Assert
            Assert.Equal(expected, actual);
        }

        // TODO COVER ADDAll null, empty, wrong with empty key, wrong with empty value, proper with non-existing key, proper with existing key

        // TODO COVER REMOVE null, empty, wrong with empty key, wrong with empty value, proper with non-existing key, proper with existing key
        [Theory]
        [InlineData(null, null, false)]
        [InlineData(null, "", false)]
        [InlineData(null, "  ", false)]
        [InlineData(null, "val1", false)]
        [InlineData("", "", false)]
        [InlineData("", null, false)]
        [InlineData("", "  ", false)]
        [InlineData("", "val1", false)]
        [InlineData("  ", null, false)]
        [InlineData("  ", "", false)]
        [InlineData("  ", "  ", false)]
        [InlineData("  ", "val1", false)]
        [InlineData("key1", null, false)]
        [InlineData("key1", "", false)]
        [InlineData("key1", "  ", false)]
        [InlineData("key1", "val1", true)]
        public void RemoveTest(string given_key, string given_value, bool expected)
        {
            // Arrange
            ArrangeByDefault();

            // Act
            Retorts.Clear();
            var added = Retorts.Add(given_key, given_value);
            var removal = Retorts.Remove(given_key);

            // Assert
            Assert.Equal(expected, removal);
        }

        // TODO COVER REMOVEAll null, empty, wrong with empty key, wrong with empty value, proper with non-existing key, proper with existing key
    }
}
