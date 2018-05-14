using Jeopardy.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Jeopardy.Tests.Services
{
    [TestClass]
    public class GameParserTests
    {

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
            for (int i = 0; i < 10000; i++)
            {
                DateTime startDate = new DateTime(year: 1986, month: 1, day: 1);
                string html = GetHTMLTitle(i, startDate.AddDays(i));

                var date = EpisodeParser.GetAirDateFromTitleString(html);

                Assert.IsNotNull(date);
            }
        }

        [TestMethod]
        public void GetAirDateFromTitleStringThrowsExceptionWhenCannotParse()
        {
            Assert.ThrowsException<Exception>(() => EpisodeParser.GetAirDateFromTitleString(""));
        }

        [TestMethod]
        public void GameParserPopulatesShowNumberFromHTMLTitle()
        {
            for (int i = 0; i < 10000; i++)
            {
                DateTime startDate = new DateTime(year: 1986, month: 1, day: 1);
                string html = GetHTMLTitle(i, startDate.AddDays(i));

                var date = EpisodeParser.GetAirDateFromTitleString(html);

                Assert.IsNotNull(date);
            }
        }

        [TestMethod]
        public void GetShowIDFromTitleStringThrowsExceptionWhenCannotParse()
        {
            Assert.ThrowsException<Exception>(() => EpisodeParser.GetAirDateFromTitleString(""));
        }
    }
}
