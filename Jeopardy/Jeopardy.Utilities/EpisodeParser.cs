﻿using HtmlAgilityPack;
using Jeopardy.DataRepository;
using System;
using System.Text.RegularExpressions;

namespace Jeopardy.Utilities
{
    public static class EpisodeParser
    {
        static string SHOWIDREGEX = @"Show #(\d\d?\d?\d?)";
        static string AIRDATEREGEX = @"\d{4}-\d{1,2}-\d{1,2}";

        public static JeopardyEpisode ParseEpisodeFromJarchiveHTML(string html)
        {
            JeopardyEpisode game = new JeopardyEpisode();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            //Get the Airdate from the title
            //The title is in the format: `J!Archive - Show #XXXX, aired 2004-09-16
            var titleNode = doc.DocumentNode.SelectSingleNode("//head/title");

            string titleText = titleNode.InnerText;
            game.AirDate = GetAirDateFromTitleString(titleText);
            game.ShowID = GetShowIDFromTitleString(titleText);

            return game;
        }

        public static DateTime GetAirDateFromTitleString(string HTMLTitle)
        {
            var match = Regex.Match(HTMLTitle, AIRDATEREGEX);

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
        public static int GetShowIDFromTitleString(string HTMLTitle)
        {
            var match = Regex.Match(HTMLTitle, SHOWIDREGEX);

            //There should be exactly one showID in the title. 
            if (match.Success && match.Groups.Count == 1)
            {
                int.TryParse(match.Groups[1].Value, out int showID);
                return showID;
            }
            else
            {
                throw new Exception("Unable to parse showID from html");
            }
        }
    }
}
