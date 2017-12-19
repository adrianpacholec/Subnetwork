using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    public class SNPP
    {
        public string Address { get; set; }
        public int Capacity { get; set; }
        public SNPP(String address, int capacity)
        {
            Address = address;
            Capacity = capacity;
        }

        override
        public String ToString()
        {
            return "Address: " + Address + ", capacity: " + Capacity;
        }
    }

}
