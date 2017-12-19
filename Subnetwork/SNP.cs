using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class SNP : SNPP
    {
        private int label;

        public SNP(String address, int capacity, int label):base(address, capacity)
        {
            this.label = label;

        }

    }
}
