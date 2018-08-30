using System;
using Xunit;
using DMB0001v4;
using DMB0001v4.Mind;

namespace Units.UnitTests.Utils
{
    public class FileUtilsTests
    {
        //private readonly Mock<IConversationStateProvider> _provider;
        //private readonly Mock<ITurnContext> _context;

        public FileUtilsTests()
        {
            //_provider = new Mock<IConversationStateProvider>();
            //_context = new Mock<ITurnContext>();
        }

        private void ArrangeByDefault()
        {
            var brainState = new BrainState();
            //_provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
            //    .Returns(brainState);
            //var retorts = Retorts.Instance(_context.Object, _provider.Object);
            //Retorts.ReadOnlyFile = true;
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("aaa", false)]
        [InlineData("aaa.json", false)]
        private void AssureFileTest(string filename, bool outcome)
        {
            // Arrange
            //var brainState = new BrainState();
            //_provider.Setup(p => p.GetConversationState<BrainState>(_context.Object))
            //    .Returns(brainState);
            //var retorts = Retorts.Instance(_context.Object, _provider.Object);

            // Act

            // Assert
            //Assert.NotNull(retorts);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("aaa", false)]
        [InlineData("json", false)]
        private void GenerateUniqueFileName_GeneratesName_Test(string extension, bool outcome)
        {
            // Arrange

            // Act
            var name = FileUtils.GenerateUniqueFileName(extension);

            // Assert
            Assert.Equal(outcome, string.IsNullOrWhiteSpace(name));
        }

        [Theory]
        [InlineData(null, "dafile")]
        [InlineData("", "dafile")]
        [InlineData(" ", "dafile")]
        [InlineData("aaa", "aaa")]
        [InlineData("json", "json")]
        private void GenerateUniqueFileName_UsesExtension_Test(string extension, string outcome)
        {
            // Arrange

            // Act
            var name = FileUtils.GenerateUniqueFileName(extension);

            // Assert
            Assert.EndsWith(outcome, name);
        }

        [Fact]
        private void ProjectCatalog_SomeContent_Tests()
        {
            // Arrange

            // Act
            var path = FileUtils.ProjectCatalog();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(path));
        }

        [Fact]
        private void ProjectCatalog_NoBin_Tests()
        {
            // Arrange

            // Act
            var path = FileUtils.ProjectCatalog();

            // Assert
            Assert.DoesNotContain("\\bin\\", path);
        }

        [Fact]
        private void ResourcesCatalog_SomeContent_Tests()
        {
            // Arrange

            // Act
            var path = FileUtils.ResourcesCatalog();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(path));
        }

        [Fact]
        private void ResourcesCatalog_NoBin_Tests()
        {
            // Arrange

            // Act
            var path = FileUtils.ResourcesCatalog();

            // Assert
            Assert.DoesNotContain("\\bin\\", path);
        }
    }
}
