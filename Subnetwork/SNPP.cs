using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class SNPP
    {
        public string Address { get; set; }
        private int capacity { get; set; }

        public SNPP(String address, int capacity)
        {
            this.Address = address;
            this.capacity = capacity;
        }
        
        override
        public String ToString()
        {
            return "Address: " + Address + ", capacity: " + capacity;
        }
    }

}
