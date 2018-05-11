using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jeopardy.Models
{
    /// <summary>
    /// This class contains a complete game of Jeopardy and all related data
    /// </summary>
    public class Game
    {
        public DateTime AirDate { get; set; }
        public int ShowID { get; set; }
    }
}
