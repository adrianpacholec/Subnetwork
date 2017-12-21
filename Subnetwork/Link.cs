using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class Link
    {
        private SNPP FirstSNPP { get; set; }
        private SNPP SecondSNPP { get; set; }

        public Link(SNPP first, SNPP second)
        {
            FirstSNPP = first;
            SecondSNPP = second;
        }
        
    }
}
