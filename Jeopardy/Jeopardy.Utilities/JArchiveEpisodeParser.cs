using HtmlAgilityPack;
using Jeopardy.Model;
using Jeopardy.Model.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Jeopardy.Utilities
{
    public class JArchiveEpisodeParser
    {
        static string SHOWIDREGEX = @"Show #(\d\d?\d?\d?)";
        static string AIRDATEREGEX = @"\d{4}-\d{1,2}-\d{1,2}";
        static string RESPONSESREGEX = @"toggle\('clue_(?:J|DJ|FJ)(?:_[1-6]_[1-6])?', 'clue_(?:J|DJ|FJ)(?:_[1-6]_[1-6])?_stuck', (.*)";



        public static JeopardyEpisode ParseEpisodeFromJarchiveHTML(string html)
        {
            JeopardyEpisode game = new JeopardyEpisode();

            game.AirDate = ParseAirDate(html);
            game.ShowID = ParseShowID(html);
            game.GameComments = ParseGameComments(html);

            return game;
        }

        private static HtmlDocument LoadHTMLDocument(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }

        private static string GetTitleFromHTML(string html)
        {
            HtmlDocument doc = LoadHTMLDocument(html);

            //Get the Airdate from the title
            //The title is in the format: `J!Archive - Show #XXXX, aired 2004-09-16
            var titleNode = doc.DocumentNode.SelectSingleNode("//head/title");

            return titleNode.InnerText;
        }

        public static string ParseGameComments(string html)
        {
            HtmlDocument doc = LoadHTMLDocument(html);
            //Get the comments for the Episode if they exist            
            var gameCommentsNode = doc.GetElementbyId("game_comments");
            return gameCommentsNode?.InnerText;
        }

        public static DateTime ParseAirDate(string html)
        {
            string title = GetTitleFromHTML(html);

            var match = Regex.Match(title, AIRDATEREGEX);

            //There should be exactly one airdate in the title. 
            if (match.Success && match.Groups.Count == 1)
            {
                DateTime.TryParse(match.Groups[1].Value, out DateTime airDate);
                return airDate;
            }
            else
            {
                throw new Exception("Unable to parse show airdate from html");
            }

        }

        public static int ParseShowID(string html)
        {
            string title = GetTitleFromHTML(html);
            var match = Regex.Match(title, SHOWIDREGEX);

            //There should be exactly one showID in the title. 
            if (match.Success && match.Groups.Count == 2)
            {
                int.TryParse(match.Groups[1].Value, out int showID);
                return showID;
            }
            else
            {
                throw new Exception("Unable to parse showID from html");
            }
        }

        public static List<Contestant> ParseGameContestants(string html)
        {
            List<Contestant> ParsedContestants = new List<Contestant>();
            HtmlDocument doc = LoadHTMLDocument(html);

            var contestantsTableNode = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[2]/div[3]/table[1]/tr[1]/td[2]");

            foreach (var paragraphNode in contestantsTableNode.ChildNodes)
            {
                if (paragraphNode.Name == "p")
                {
                    //string is in the format: 
                    //Ken Jennings, a software engineer from Salt Lake City, Utah (whose 38-day cash winnings total $1,321,660)
                    List<string> playerData = new List<string>(paragraphNode.InnerText.Split(','));
                    string playerName = playerData[0]; //Get the first split then concat the rest back together
                    playerData.RemoveAt(0);
                    string playerDescription = String.Concat(playerData);

                    ParsedContestants.Add(new Contestant { Name = playerName, Description = playerDescription });
                }
            }

            return ParsedContestants;

        }

        public static Round ParseRound(string html, RoundEnum RoundToParse)
        {
            Round ParsedRound = new Round();
            HtmlDocument doc = LoadHTMLDocument(html);
            HtmlNode roundNode;

            if (RoundToParse == RoundEnum.Jeopardy)
                roundNode = doc.GetElementbyId("jeopardy_round");
            else if (RoundToParse == RoundEnum.DoubleJeopardy)
                roundNode = doc.GetElementbyId("double_jeopardy_round");
            else if (RoundToParse == RoundEnum.FinalJeopardy)
                roundNode = doc.DocumentNode.SelectSingleNode("//table[contains(@class, 'final_round')]");
            else
                throw new ArgumentOutOfRangeException("RoundToParse");


            //Get all the category names and comments
            var categoryNodes = roundNode.SelectNodes(".//td[contains(@class, 'category_name')]");
            var categoryComments = roundNode.SelectNodes(".//td[contains(@class, 'category_comments')]");

            if (categoryComments.Count != categoryNodes.Count)
                throw new Exception("Number of 'Category_Name' Nodes does not match number of 'Category_Comment' Nodes");

            //Assume the categories are listed in order
            for (int categoryNumber = 0; categoryNumber < categoryComments.Count; categoryNumber++)
            {
                var category = new Category
                {
                    Name = categoryNodes[categoryNumber].InnerText,
                    Comments = categoryComments[categoryNumber].InnerText
                };

                //convert the categories to 1 based from the 0 based loop
                ParsedRound.Categories.Add(categoryNumber + 1, category);
            }

            //Get all the clues
            var cluesNodes = roundNode.SelectNodes(".//td[@class='clue']");

            foreach (var clueNode in cluesNodes)
            {              

                var clueTextNode = clueNode.SelectSingleNode(".//td[@class='clue_text']");
                
                //if the clue was not revealed it won't have any data. Move to the next clue
                if (clueTextNode == null)
                    continue;

                //Get the correct response
                //Final Jeopardy "clue_text" class does not contain the mouseover, it is on the outside.
                var ResponsesNode = RoundToParse == RoundEnum.FinalJeopardy ?
                    roundNode.SelectSingleNode(".//div[@onmouseover]") :
                    clueNode.SelectSingleNode(".//div[@onmouseover]");

                string ResponsesText = ResponsesNode.GetAttributeValue("onmouseover", "NONE");

                var responsesMatch = Regex.Match(ResponsesText, RESPONSESREGEX);

                if (!responsesMatch.Success || !(responsesMatch.Groups.Count == 2))
                    throw new Exception("No Match for responses");

                //Make it into workable HTML                      
                string htmlText = HtmlEntity.DeEntitize(responsesMatch.Groups[1].Value);
                //New HtmlDocument
                HtmlDocument htmlInsideToggle = new HtmlDocument();
                htmlInsideToggle.LoadHtml(htmlText);

                var CorrectAnswerNode = htmlInsideToggle.DocumentNode.SelectSingleNode(".//em[contains(@class,'correct_response')]");


                //Get the clue coordinates, value, and order for regular Jeopardy rounds
                string ClueValueString = "0";
                int clueOrderNumber = -1,clueNumber = 1,categoryNumber = 1;
                if (RoundToParse == RoundEnum.Jeopardy || RoundToParse == RoundEnum.DoubleJeopardy)
                {
                    //ID is in format 'clue_round_categoryNum_clueNum' eg: 'clue_J_1_1" or 'clue_DJ_2_3' or 'clue_FJ'
                    List<string> clueCoordinates = new List<string>(clueTextNode.Id.Split('_'));

                    if (clueCoordinates.Count != 4)
                        throw new Exception("Well, something ain't right with that boy.");

                    clueNumber = int.Parse(clueCoordinates[3]);
                    categoryNumber = int.Parse(clueCoordinates[2]);

                    //Get the clue value, will catch clue_value or clue_value_daily_double
                    var clueValueNode = clueNode.SelectSingleNode(".//td[contains(@class, 'clue_value')]");
                    if (clueValueNode == null)
                        throw new Exception("Couldn't find the clue_value node");

                    //Remove the DD modifier and the dollar sign
                    ClueValueString = clueValueNode.InnerText.Replace("DD", "").Replace("$", "").Replace(":", "").Trim();

                    //Get the clue order
                    var clueOrderNode = clueNode.SelectSingleNode(".//td[@class='clue_order_number']");
                    
                    if (clueOrderNode != null)
                        clueOrderNumber = int.Parse(clueOrderNode.InnerText);
                }

                var clue = new Clue
                {
                    ClueText = HtmlEntity.DeEntitize(clueTextNode.InnerText),
                    ClueValue = int.Parse(ClueValueString, NumberStyles.AllowThousands| NumberStyles.AllowCurrencySymbol),
                    CorrectResponse = HtmlEntity.DeEntitize(CorrectAnswerNode.InnerText),
                    ClueOrder = clueOrderNumber
                };

                ParsedRound.Categories[categoryNumber].Clues.Add(clueNumber, clue);
            }



            return ParsedRound;
        }

    }
}


