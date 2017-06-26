using System;
using System.Collections.Generic;
using System.Text;

namespace Mobile.KI.Shadow.Models
{
   public class Move
    {
        public string Name { get; set; }
        public string VideoSrc { get; set; }
        public int StartGap { get; set; }
        public int Freeze { get; set; }
        public IEnumerable<Range> Ranges { get; set; }
    }
}
