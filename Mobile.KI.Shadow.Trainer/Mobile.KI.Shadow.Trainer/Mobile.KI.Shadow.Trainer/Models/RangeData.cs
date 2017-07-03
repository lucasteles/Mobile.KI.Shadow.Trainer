using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobile.KI.Shadow.Trainer.Models
{
   public class RangeData
    {

        public RangeData(int init, int end)
        {
            Init = init;
            End = end;
        }
        public int Init { get; set; }
        public int End { get; set; }


        public bool InRange(int value) =>
                value >= Init && value <= End;


    }
}
