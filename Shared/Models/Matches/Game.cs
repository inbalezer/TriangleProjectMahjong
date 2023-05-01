using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriangleProject.Shared.Models.Matches
{
    public class Game
    {
        public List<SingleMatchDetails> FirstMatchesContent { get; set; } // רשימה של החלק הראשון בהתאמה
        public List<SingleMatchDetails> SecondMatchesContent { get; set; } // רשימה של החלק השני בהתאמה
        public string GameInstruction { get; set; }
    }
}
