using Jeopardy.Model;
using Jeopardy.Model.Enums;
using Jeopardy.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Jeopardy.Tests.Services
{
    [TestClass]
    public class GameParserTests
    {
        Dictionary<int, string> _htmlFilesByShowID;
        const int NUM_FILES = 7000;

        [TestInitialize]
        public void InitializeTests()
        {
            _htmlFilesByShowID = new Dictionary<int, string>();

            for (int i = 1; i <= NUM_FILES; i++)
            {
                string contents = GetFileContents(i);

                if (!String.IsNullOrEmpty(contents))
                    _htmlFilesByShowID.Add(i, contents);
            }

            string GetFileContents(int showID)
            {
                var asm = GetType().GetTypeInfo().Assembly;
                using (var stream = asm.GetManifestResourceStream($"Jeopardy.Tests.HtmlFiles.{showID}.html"))
                {
                    if (stream != null)
                    {
                        var reader = new StreamReader(stream);
                        return reader.ReadToEnd();
                    }
                }
                return string.Empty;
            }
        }

        string GetHTMLTitle(int showNum, DateTime date)
        {
            return $@"
                  <head>
                    <title> J!Archive - Show #{showNum}, aired {date:yyyy-mm-dd}</title>                       
                  </head>";
        }

        [TestMethod]
        public void GetAirDateFromTitleStringReturnsAirDateWhenFormatIsValid()
        {
            //for (int i = 0; i < 10000; i++)
            //{
            //    DateTime startDate = new DateTime(year: 1986, month: 1, day: 1);
            //    string html = GetHTMLTitle(i, startDate.AddDays(i));

            //    var date = EpisodeParser.GetAirDateFromTitleString(html);
            //}

            foreach (var show in _htmlFilesByShowID)
            {
                string html = show.Value;
                var date = JArchiveEpisodeParser.ParseAirDate(html);

                Assert.IsNotNull(date);
            }
        }

        [TestMethod]
        public void GetAirDateFromTitleStringThrowsExceptionWhenCannotParse()
        {
            Exception expectedExcetpion = null;

            try
            {
                JArchiveEpisodeParser.ParseAirDate("");
            }
            catch (Exception ex)
            {
                expectedExcetpion = ex;
            }

            Assert.IsNotNull(expectedExcetpion);
        }

        [TestMethod]
        public void GetShowIDFromTitleStringGetsPopulated()
        {
            foreach (var show in _htmlFilesByShowID)
            {
                string html = show.Value;
                var showID = JArchiveEpisodeParser.ParseShowID(html);

                Assert.IsNotNull(showID);
            }
               
        }

        [TestMethod]
        public void GetShowIDFromTitleStringThrowsExceptionWhenCannotParse()
        {
            Exception expectedExcetpion = null;

            try
            {
                JArchiveEpisodeParser.ParseShowID("");
            }
            catch(Exception ex)
            {
                expectedExcetpion = ex;
            }

            Assert.IsNotNull(expectedExcetpion);
            
        }

        [TestMethod]
        public void ParseGameComments_WithVaildHTML_ReturnsExpectedString()
        {
            var gameComments = JArchiveEpisodeParser.ParseGameComments(_htmlFilesByShowID[1]);
            Assert.AreEqual(gameComments, "Ken Jennings game 39.\r\r\nFirst game of Season 21.  New title graphics.");
        }

        [TestMethod]       
        public void ParseGameComments_WithInvaildHTML_ReturnsEmptyString()
        {
            var gameComments = JArchiveEpisodeParser.ParseGameComments(_htmlFilesByShowID[5965]);
            Assert.AreEqual(gameComments, String.Empty);
        }

        [TestMethod]
        public void ParseGameContestants_WithVaildHTML_ReturnsThreePlayersWithDescription()
        {
            foreach (var show in _htmlFilesByShowID)
            {
                List<Contestant> playerList = JArchiveEpisodeParser.ParseGameContestants(show.Value);
                Assert.AreEqual(playerList.Count, 3);

                foreach (var player in playerList)
                {
                    Assert.IsNotNull(player.Name);
                    Assert.IsNotNull(player.Description);
                }
            }
        }
        

        [TestMethod]
        public void ParseJeopardyRound_WithVaildHTML_ReturnsSixCategories()
        {
            foreach (var show in _htmlFilesByShowID)
            {
                Round roundData = JArchiveEpisodeParser.ParseRound(show.Value, RoundEnum.Jeopardy);
                Assert.AreEqual(roundData.Categories.Count, 6);
            }
        }
        [TestMethod]
        public void ParseDoubleJeopardyRound_WithVaildHTML_ReturnsSixCategories()
        {
            foreach (var show in _htmlFilesByShowID)
            {
                var roundData = JArchiveEpisodeParser.ParseRound(show.Value, RoundEnum.DoubleJeopardy);
                Assert.AreEqual(roundData.Categories.Count, 6);
            }
        }
        [TestMethod]
        public void ParseFinalJeopardyRound_WithVaildHTML_ReturnsOneCategoryWithOneClue()
        {
            foreach (var show in _htmlFilesByShowID)
            {
                var roundData = JArchiveEpisodeParser.ParseRound(show.Value, RoundEnum.FinalJeopardy);
                Assert.AreEqual(roundData.Categories.Count, 1);
                Assert.AreEqual(roundData.Categories[1].Clues.Count, 1);
                Assert.AreEqual(roundData.Categories[1].Clues[1].ClueValue, 0);
            }
        }
      

    }
}
