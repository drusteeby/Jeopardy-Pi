using System;
using System.Collections.Generic;
using System.Text;

namespace Jeopardy.Model
{
    public class Category
    {
        public string Name { get; set; }
        public string Comments { get; set; }
        public Dictionary<int,Clue> Clues { get; set; } = new Dictionary<int,Clue>();
    }
}
