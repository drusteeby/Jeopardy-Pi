using System;
using System.Collections.Generic;
using System.Text;

namespace Jeopardy.Model
{
    public class Round
    {
        /// <summary>
        /// List of Categories ordered by their number
        /// </summary>
        public Dictionary<int,Category> Categories { get; set; } = new Dictionary<int,Category>();
    }
}
