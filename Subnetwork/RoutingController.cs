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
            LogClass.WhiteLog("[CC -> RC] RouteTableQuery called between: " + pathBegin + " and: " + pathEnd);
            try
            {
                List<SNPP> scheduled = router.Route(pathBegin, pathEnd, capacity);
                LogClass.WhiteLog("[RC -> CC] RouteTableQuery returned SNPP List");
                return scheduled;
            }
            catch (FormatException e)
            {
                LogClass.WhiteLog("[RC -> CC] RouteTableQuery failed to return SNPP List");
                return null;
            }
        }

        public void DeleteLink(string begin, string end)
        {
            Link linkToBeDeleted = links.Find(x => (x.FirstSNPP.Address == begin && x.SecondSNPP.Address == end));
            if (linkToBeDeleted != null)
            {
                deletedLinks.Add(linkToBeDeleted);
                links.Remove(linkToBeDeleted);
                LogClass.MagentaLog("[RC] Removed link: " + linkToBeDeleted.FirstSNPP.Address + " - " + linkToBeDeleted.SecondSNPP.Address + " from RC.");
            }
        }

        public void RestoreLinks()
        {
            links.ForEach(x => x.ignore = false);
        }

        public void LocalTopologyIn(bool delete, SNP localTopologyUpdate)
        {
            Link link = links.Find(x => x.FirstSNPP.Address == localTopologyUpdate.Address || x.SecondSNPP.Address == localTopologyUpdate.Address);
            SNPP snpp = null;

            if (link.FirstSNPP.Address == localTopologyUpdate.Address)
                snpp = link.FirstSNPP;
            else
                snpp = link.SecondSNPP;


            if (delete)
            {
                snpp.Capacity += localTopologyUpdate.OccupiedCapacity;

                LogClass.MagentaLog("[LRM -> RC] Topology: Deallocated " + localTopologyUpdate.OccupiedCapacity + "Mbit/s from SNPP " + localTopologyUpdate.Address + ".");
            }
            else
            {
                snpp.Capacity -= localTopologyUpdate.OccupiedCapacity;
                LogClass.GreenLog("[LRM -> RC] Topology: Allocated " + localTopologyUpdate.OccupiedCapacity + "Mbit/s to SNPP " + localTopologyUpdate.Address + ".");
            }
            LogClass.WhiteLog("[LRM] " + snpp.Capacity + "Mbit/s left on " + snpp.Address);
        }

        public void IgnoreLink(SNP snp)
        {
            Link link = links.Find(x => (x.FirstSNPP.Address.Equals(snp.Address) || x.SecondSNPP.Address.Equals(snp.Address)));
            if(link!=null)
                link.ignore = true;
        }

        internal void AddSNPP(SNPP sNPP)
        {
            throw new NotImplementedException();
        }
    }
}




