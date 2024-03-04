using Arctic.Puzzlers.Objects.Misc;
using Arctic.Puzzlers.Parsers.CompetitionParsers;
using Arctic.Puzzlers.Stores.MemoryStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestArctic.Puzzlers.Parsers.CompetitionParsers
{
    public class SpeedPuzzlingParserTests
    {
        [Fact]
        public async Task ParsePdfFromUrl_ReturnNameOfCompetition()
        {
            var memoryStore = new MemoryCompetitionStore(new ConfigurationBuilder().Build());
            var parser = new SpeedPuzzlingParser(memoryStore, new HttpClient(), new NullLogger<SpeedPuzzlingParser>());
            await parser.ParsePdf("https://www.speedpuzzling.com/uploads/3/7/7/0/37705857/sp178s.pdf");
            var result = await memoryStore.GetAll();
            var firstName = result.First().Name;
            Assert.Equal("Speed Puzzling #178", firstName);
        }

        [Fact]
        public async Task ParsePdfFromUrl_ReturnCorrectAmountOfParticipants()
        {
            var memoryStore = new MemoryCompetitionStore(new ConfigurationBuilder().Build());
            var parser = new SpeedPuzzlingParser(memoryStore, new HttpClient(), new NullLogger<SpeedPuzzlingParser>());
            await parser.ParsePdf("https://www.speedpuzzling.com/uploads/3/7/7/0/37705857/sp178s.pdf");
            var result = await memoryStore.GetAll();
            var firstRound = result.First().CompetitionGroups.First().Rounds.First();
            Assert.Equal(39, firstRound.Participants.Count());
        }

        [Fact]
        public async Task ParsePdfFromUrl_ReturnNameOnAll()
        {
            var memoryStore = new MemoryCompetitionStore(new ConfigurationBuilder().Build());
            var parser = new SpeedPuzzlingParser(memoryStore, new HttpClient(), new NullLogger<SpeedPuzzlingParser>());
            await parser.ParsePdf("https://www.speedpuzzling.com/uploads/3/7/7/0/37705857/sp178s.pdf");
            var result = await memoryStore.GetAll();
            var firstRound = result.First().CompetitionGroups.First().Rounds.First();
            Assert.Equal(39, firstRound.Participants.Count());
            Assert.True(firstRound.Participants.All(t => t.Participants.Count == 1));
            Assert.True(firstRound.Participants.Any(t => t.Participants.Any(k => k.Country == Countries.BEL)));
        }

        [Fact]
        public async Task ParsePdfFromUrl_TeamReturnedOk()
        {
            var memoryStore = new MemoryCompetitionStore(new ConfigurationBuilder().Build());
            var parser = new SpeedPuzzlingParser(memoryStore, new HttpClient(), new NullLogger<SpeedPuzzlingParser>());
            await parser.ParsePdf("https://www.speedpuzzling.com/uploads/3/7/7/0/37705857/sp170t_results.pdf");
            var result = await memoryStore.GetAll();
            var firstRound = result.First().CompetitionGroups.First().Rounds.First();
            Assert.Equal(11, firstRound.Participants.Count());
            Assert.True(firstRound.Participants.All(t => t.Participants.Count == 4));
        }

        
    }
}
