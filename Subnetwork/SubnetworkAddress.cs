using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class SubnetworkAddress
    {
        public IPAddress subnetAddress { get; set; }
        public IPAddress subnetMask { get; set; }

        public SubnetworkAddress(String subnetAddress, String subnetMask)
        {
            this.subnetAddress = IPAddress.Parse(subnetAddress);
            this.subnetMask = IPAddress.Parse(subnetMask);
        }


    }
}
