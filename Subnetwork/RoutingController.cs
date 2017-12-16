using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class RoutingController
    {
        List<SNPP> snpps;
      
        public RoutingController()
        {
            snpps = new List<SNPP>();
        }

        public void addSNPP(SNPP snpp)
        {
            snpps.Add(snpp);
        }



    }
}
