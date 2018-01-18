using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CustomSocket;

namespace Subnetwork
{
    class RoutingController
    {
        private List<SNPP> SNPPList;
        Router router;
        private List<SubnetworkAddress> containedSubnetworks;
        private List<Link> links;
        private List<Link> deletedLinks;

        public RoutingController(List<SubnetworkAddress> containedSubnetworks, List<Link> links)
        {
            this.containedSubnetworks = containedSubnetworks;
            this.links = links;
            SNPPList = new List<SNPP>();
            router = new Router(containedSubnetworks, links);
            deletedLinks = new List<Link>();
        }

        public bool RestoreLink(String firstAddress, String secondAddress)
        {
            Link toRestore = deletedLinks.Find((x =>
            ((x.FirstSNPP.Address.Equals(firstAddress) && x.SecondSNPP.Address.Equals(secondAddress))
            || (x.FirstSNPP.Address.Equals(secondAddress) && x.SecondSNPP.Address.Equals(firstAddress)))));
            if (toRestore != null)
            {
                links.Add(toRestore);
                return true;
            }
            else
                return false;
        }

        public List<SNPP> RouteTableQuery(IPAddress pathBegin, IPAddress pathEnd, int capacity)
        {
            try
            {
                List<SNPP> scheduled = router.route(pathBegin, pathEnd, capacity);
                return scheduled;
            }catch(System.FormatException e)
            {
                return new List<SNPP>();
            }
        }

        public void DeleteLink(string begin, string end)
        {
            Link linkToBeDeleted = links.Find(x => (x.FirstSNPP.Address == begin && x.SecondSNPP.Address == end));
            deletedLinks.Add(linkToBeDeleted);
            links.Remove(linkToBeDeleted);
            LogClass.Log("[RC] Removed link: " + linkToBeDeleted.FirstSNPP.Address + " - " + linkToBeDeleted.SecondSNPP.Address + " from RC.");
        }

        public void LocalTopologyIn(bool delete, SNP localTopologyUpdate)
        {
            Link link = links.Find(x => x.FirstSNPP.Address == localTopologyUpdate.Address || x.SecondSNPP.Address==localTopologyUpdate.Address);
            SNPP snpp = null;
            if (link.FirstSNPP.Address == localTopologyUpdate.Address)
                snpp = link.FirstSNPP;
            else
                snpp = link.SecondSNPP;
            if (delete)
            {
                snpp.Capacity += localTopologyUpdate.OccupiedCapacity;
                LogClass.GreenLog("[RC]Received Topology: Added " + localTopologyUpdate.OccupiedCapacity + "Mbit/s to SNPP " + localTopologyUpdate.Address + ".");
            }
            else
            {
                snpp.Capacity -= localTopologyUpdate.OccupiedCapacity;
                LogClass.MagentaLog("[RC]Received Topology: Removed " + localTopologyUpdate.OccupiedCapacity + "Mbit/s from SNPP " + localTopologyUpdate.Address + ".");
            }
        }


        private void NetworkTopologyIn(SNPP localTopologyUpdate)
        {

        }

        private void NetworkTopologyOut(SNPP localTopologyUpdate)
        {

        }

        internal void AddSNPP(SNPP snpp)
        {
            throw new NotImplementedException();
        }

        internal void IgnoreLink(SNP snp)
        {
            Link link=links.Find(x => (x.FirstSNPP.Address.Equals(snp.Address) || x.SecondSNPP.Address.Equals(snp.Address)));
            link.ignore = true;
        }
    }
}




