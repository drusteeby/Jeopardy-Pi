using System;
using System.Collections.Generic;

namespace Jeopardy.Model
{
    public class JeopardyEpisode
    {
        public DateTime AirDate { get; set; }
        public int ShowID { get; set; }
        public string GameComments { get; set; }

        public List<Contestant> Contestants { get; set; }

        public Round JeopardyRound { get; set; }
        public Round DoubleJeopardyRound { get; set; }
        public Round FinalJeopardyRound { get; set; }
    }
}
