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

       /* public override bool Equals(Object obj)
        {
            SubnetworkAddress address = (SubnetworkAddress)obj;
            if (subnetAddress.Equals(address.subnetAddress) && subnetAddress.Equals(address.subnetMask))
                return true;
            else return false;
        }

        public override int GetHashCode()
        {
            return subnetAddress.GetHashCode() + subnetMask.GetHashCode();
        }
        */

    }
}
