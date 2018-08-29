using DMB0001v4;
using DMB0001v4.Providers;
using DMB0001v4.Skills;
using Microsoft.Bot.Builder;
using Moq;
using Xunit;

namespace Units.UnitTests.Skills
{
    public class SkillsFactoryTests
    {
        private readonly Mock<IConversationStateProvider> _provider;
        private readonly Mock<ITurnContext> _context;
        private SkillFactory _factory;

        public SkillsFactoryTests()
        {
            _provider = new Mock<IConversationStateProvider>();
            _context = new Mock<ITurnContext>();
            //var brainState = new BrainState();
            //_provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
            //    .Returns(brainState);
            _factory = SkillFactory.GetInstance();
        }

        // Cover get instance, if there was none
        [Fact]
        public void GetInstanceTest()
        {
            // Arrange
            var factory = SkillFactory.GetInstance();

            // Act

            // Assert
            Assert.NotNull(factory);
        }

        // Cover return of existing skill for the first time
        [Fact]
        public void GetSkillExistingTest()
        {
            // Arrange
            var factory = SkillFactory.GetInstance();
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            var skill = factory.GetSkill("greetings", _context.Object, _provider.Object);

            // Assert
            Assert.NotNull(skill);
        }

        // Cover return of existing skill for the 2nd time
        [Fact]
        public void GetSkillExistingAgainTest()
        {
            // Arrange
            var factory = SkillFactory.GetInstance();
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            var skill = factory.GetSkill("greetings", _context.Object, _provider.Object);
            var skill2 = factory.GetSkill("greetings", _context.Object, _provider.Object);

            // Assert
            Assert.Equal(skill, skill2);
        }

        // Cover return of nonexisting skill
        [Fact]
        public void GetSkillNonexistingTest()
        {
            // Arrange
            var factory = SkillFactory.GetInstance();
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            var skill = factory.GetSkill("greetings_like_you_test_it", _context.Object, _provider.Object);

            // Assert
            Assert.Null(skill);
        }

        // Cover count after init
        [Theory]
        [InlineData(2)]
        public void CountAfterInitTest(int expected)
        {
            // Arrange
            var factory = SkillFactory.GetInstance();
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            var actual = SkillFactory.Count;

            // Assert
            Assert.Equal(expected, actual);
        }

        // Cover count after init of a single skill
        [Theory]
        [InlineData(1)]
        public void CountAfterInitSingleSkillTest(int expected)
        {
            // Arrange
            var factory = SkillFactory.GetInstance();
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            SkillFactory.Clear();
            var skill = factory.GetSkill("greetings", _context.Object, _provider.Object);
            var actual = SkillFactory.Count;

            // Assert
            Assert.Equal(expected, actual);
        }

        // Cover clear after init
        [Theory]
        [InlineData(0)]
        public void CountAfterClearTest(int expected)
        {
            // Arrange
            var factory = SkillFactory.GetInstance();
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            SkillFactory.Clear();
            var actual = SkillFactory.Count;

            // Assert
            Assert.Equal(expected, actual);
        }

        // Cover return of existing skill for the 2nd time
        [Fact]
        public void CountSkillsExistingAgainTest()
        {
            // Arrange
            var factory = SkillFactory.GetInstance();
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            SkillFactory.Clear();
            var skill = factory.GetSkill("greetings", _context.Object, _provider.Object);
            var skill2 = factory.GetSkill("greetings", _context.Object, _provider.Object);
            var count = SkillFactory.Count;

            // Assert
            Assert.Equal(1, count);
        }

        // Cover clear after init of a single skill
        [Theory]
        [InlineData(0)]
        public void ClearAfterInitSingleSkillTest(int expected)
        {
            // Arrange
            var factory = SkillFactory.GetInstance();
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            SkillFactory.Clear();
            var skill = factory.GetSkill("greetings", _context.Object, _provider.Object);
            SkillFactory.Clear();
            var actual = SkillFactory.Count;

            // Assert
            Assert.Equal(expected, actual);
        }

        // Cover process of null string, empty string, not found phrase, found phrase
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(" unknown phrase ")]
        [InlineData("hi")]
        public void ProcessTest(string given)
        {
            // Arrange
            var factory = SkillFactory.GetInstance(_context.Object, _provider.Object);
            var brainState = new BrainState();
            _provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
                .Returns(brainState);

            // Act
            var skill = factory.GetSkill("greetings", _context.Object, _provider.Object);
            var actual = factory.Process(given);

            // Assert
            if (given != null && given.Equals("hi"))
                Assert.NotNull(actual);
            else
                Assert.Null(actual);
        }
    }
}
