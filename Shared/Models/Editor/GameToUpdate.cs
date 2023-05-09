using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriangleProject.Shared.Models.Editor
{
    public class GameToUpdate
    {
        public int ID { get; set; }
        public string GameFullName { get; set; }
        public string PublishStatus { get; set; }
        public string GameInstruction { get; set; }

    }
}
