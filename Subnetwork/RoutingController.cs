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

        public RoutingController(List<SubnetworkAddress> containedSubnetworks, List<Link> links)
        {
            this.containedSubnetworks = containedSubnetworks;
            this.links = links;
            SNPPList = new List<SNPP>();
            router = new Router(containedSubnetworks, links);
        }

        public List<SNPP> RouteTableQuery(IPAddress pathBegin, IPAddress pathEnd, int capacity)
        {
            List<SNPP> scheduled = router.route(pathBegin, pathEnd, capacity);
            return scheduled;
        }

        public void DeleteLink(string begin, string end)
        {
            Link linkToBeDeleted = links.Find(x => (x.FirstSNPP.Address == begin && x.SecondSNPP.Address == end));
            links.Remove(linkToBeDeleted);
            LogClass.Log("Removed link: " + linkToBeDeleted.FirstSNPP.Address + " - " + linkToBeDeleted.SecondSNPP.Address + " from RC.");
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
                LogClass.Log("Removed " + localTopologyUpdate.Address + ".");
            }
            else
            {
                snpp.Capacity -= localTopologyUpdate.OccupiedCapacity;
                LogClass.Log("Added " + localTopologyUpdate.Address + ".");
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
    }
}




