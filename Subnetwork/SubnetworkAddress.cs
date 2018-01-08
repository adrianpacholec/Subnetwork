using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    public class SubnetworkAddress
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
        public override String ToString()
        {
            return subnetAddress.ToString() + " " + subnetMask.ToString();
        }

        public bool Equals(SubnetworkAddress other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.subnetAddress.Equals(subnetAddress) && other.subnetMask.Equals(subnetMask);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(SubnetworkAddress)) return false;
            return Equals((SubnetworkAddress)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = HashString(subnetMask.ToString()) + HashString(subnetAddress.ToString());
                return hash;
            }
        }

        public int HashString(string text)
        {
            // TODO: Determine nullity policy.

            unchecked
            {
                int hash = 0;
                foreach (char c in text)
                {
                    hash = c;
                }
                return hash;
            }
        }
    }
}
