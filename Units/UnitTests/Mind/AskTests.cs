using DMB0001v4.Mind;
using Xunit;

namespace Units.UnitTests.Mind
{
    public class AskTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData(0, 0)]
        [InlineData(-1, -1)]
        [InlineData(int.MaxValue, int.MaxValue)]
        public void Id_Test(int given, int expected)
        {
            // Arrange
            var ask = new Ask {Id = given};

            // Act
            var actual = ask.Id;

            // Assert
            Assert.Equal(expected, actual);
        }
        
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData("val1", "val1")]
        public void Question_Test(string given, string expected)
        {
            // Arrange
            var ask = new Ask {Question = given};

            // Act
            var actual = ask.Question;

            // Assert
            Assert.Equal(expected, actual);
        }
        
        [Theory]
        [InlineData(null, null)]
        [InlineData(new[] { "", "" }, new[] { "", "" })]
        [InlineData(new[] { " ", " " }, new[] { " ", " " })]
        [InlineData(new[] { "Yes", "No" }, new[] { "Yes", "No" })]
        public void Answers_Test(string[] given, string[] expected)
        {
            // Arrange
            var ask = new Ask {Answers = given};

            // Act
            var actual = ask.Answers;

            // Assert
            Assert.Equal(expected, actual);
        }
        
        [Theory]
        [InlineData(null, null)]
        [InlineData(new[] { "", "" }, new[] { "", "" })]
        [InlineData(new[] { " ", " " }, new[] { " ", " " })]
        [InlineData(new[] { "Yay", "Nah" }, new[] { "Yay", "Nah" })]
        public void Responses_Test(string[] given, string[] expected)
        {
            // Arrange
            var ask = new Ask {Responses = given};

            // Act
            var actual = ask.Responses;

            // Assert
            Assert.Equal(expected, actual);
        }
        
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData("Yup", "Yup")]
        public void FinalAnswer_Test(string given, string expected)
        {
            // Arrange
            var ask = new Ask {FinalAnswer = given};

            // Act
            var actual = ask.FinalAnswer;

            // Assert
            Assert.Equal(expected, actual);
        }
        
        [Theory]
        [InlineData(null, null)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Processed_Test(bool given, bool expected)
        {
            // Arrange
            var ask = new Ask {Processed = given};

            // Act
            var actual = ask.Processed;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}