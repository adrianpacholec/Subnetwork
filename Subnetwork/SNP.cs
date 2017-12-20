using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    public class SNP
    { 
        public int Label { set; get; }
        public string Address { get; set; }
        public int OccupiedCapacity { get; set; }

        public SNP(int label, string address, int capacity)
        {
            Label = label;
            Address = address;
            OccupiedCapacity = capacity;
        }
       
    }
}
