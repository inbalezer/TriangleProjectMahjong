using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriangleProject.Shared.Models.Editor
{
    public class MatchToUpdate
    {
        public int ID { get; set; }
        public int GameID { get; set; }

        [StringLength(15, ErrorMessage = "")]
        public string FirstMatch { get; set; }

        [StringLength(15, ErrorMessage = "")]
        public string SecondMatch { get; set; }
        public bool FirstIsText { get; set; }
        public bool SecondIsText { get; set; }
    }
}
