using Arctic.Puzzlers.Parsers.CompetitionParsers;
using Arctic.Puzzlers.Stores.MemoryStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Assert.Equal("Speed Puzzling #178", firstRound.Participants.Count());
        }
    }
}
