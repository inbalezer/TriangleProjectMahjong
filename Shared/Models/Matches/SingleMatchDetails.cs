using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriangleProject.Shared.Models.Matches
{
    public class SingleMatchDetails
    {
        public string MatchText { get; set; } // חלק אחד בהתאמה
        public string MatchImg { get; set; } //  חלק שני בהתאמה 
        public bool IsText { get; set; }
    }
}
