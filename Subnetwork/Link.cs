using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class Link
    {
        public SNPP FirstSNPP { get; set; }
        public SNPP SecondSNPP { get; set; }
        public bool ignore;

        public Link(SNPP first, SNPP second)
        {
            FirstSNPP = first;
            SecondSNPP = second;
            ignore = false;
        }
        
    }
}
