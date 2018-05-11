using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jeopardy.Models
{
    /// <summary>
    /// Contains
    /// </summary>
    public class Clue
    {
        /// <summary>
        /// The point value that the clue is worth.
        /// </summary>
        public int ClueValue { get; set; }

        /// <summary>
        /// The Correct Response of the Question
        /// </summary>
        public string CorrectResponse { get; set; }

        /// <summary>
        /// The text used to ask the question
        /// </summary>
        public string ClueText { get; set; }
    }
}
