using System;
using System.Collections.Generic;
using System.Text;

namespace Mobile.KI.Shadow.Models
{
    public class Character
    {
        public string Name { get; set; }
        public string Thumb { get; set; }
        public IEnumerable<Move> Moves { get; set; }
    }
}
