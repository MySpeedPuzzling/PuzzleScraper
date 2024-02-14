using Arctic.Puzzlers.Parsers;

namespace TestArctic.Puzzlers.Parsers
{
    public class CleanUpStringExtensionsTests
    {        
        [Theory]
        [InlineData("theory 100p", "theory")]
        [InlineData("theory 100 p", "theory")]
        [InlineData("theory 2x48 p", "theory")]        
        [InlineData("theory 2x48p", "theory")]
        [InlineData("theory 100pc", "theory")]
        [InlineData("theory 100 pc", "theory")]
        [InlineData("theory 2x48 pc", "theory")]
        [InlineData("theory 2x48pc", "theory")]
        [InlineData("theory 100 pcs", "theory")]
        [InlineData("theory 100pcs", "theory")]
        [InlineData("theory 2x48pcs", "theory")]
        [InlineData("theory 2x48 pcs", "theory")]
        public void CleanupName_RemoveData(string name, string expected)
        {
            name = name.CleanUpName();
            Assert.Equal(expected, name);
        }
    }
}