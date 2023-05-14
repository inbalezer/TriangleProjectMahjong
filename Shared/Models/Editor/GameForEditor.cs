using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriangleProject.Shared.Models.Matches;

namespace TriangleProject.Shared.Models.Editor
{
    public class GameForEditor
    {
        public int ID { get; set; }
        public string GameFullName { get; set; }
        public string PublishStatus { get; set; }
        public int GameCode { get; set; }
        public List <MatchToUpdate> Matches { get; set; }

    }
}
