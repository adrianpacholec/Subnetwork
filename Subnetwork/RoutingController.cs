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
        private Dictionary<string, List<SNP>> SNPsbySNPPaddress;
        private Dictionary<SubnetworkAddress, List<Tuple<IPAddress, IPAddress>>> OtherDomainSNPPAddressTranslation;
        Router router;

        private List<SubnetworkAddress> containedSubnetworks;
        private List<Link> links;
        
        internal void testRouting()
        {
            RouteTableQuery(IPAddress.Parse("10.1.64.0"), IPAddress.Parse("10.1.196.0"), 20);
        }

        public RoutingController(List<SubnetworkAddress> containedSubnetworks, List<Link> links)
        {
            this.containedSubnetworks = containedSubnetworks;
            this.links = links;
            SNPPList = new List<SNPP>();
            router = new Router(containedSubnetworks, links);
            OtherDomainSNPPAddressTranslation = new Dictionary<Subnetwork.SubnetworkAddress, List<Tuple<IPAddress, IPAddress>>>();
            SNPsbySNPPaddress = new Dictionary<string, List<SNP>>();
        }

        

        private List<SNPP> RouteTableQuery(IPAddress pathBegin, IPAddress pathEnd, int capacity)
        {
            foreach (SubnetworkAddress domainAddress in OtherDomainSNPPAddressTranslation.Keys)
            {
                //sprawdza, z ktorej domeny przyszedl SNP i podmienia jego adres na adres swojego SNPP brzegowego
                if (IPAddressExtensions.IsInSameSubnet(pathBegin, domainAddress.subnetAddress, domainAddress.subnetMask))
                {
                    Tuple<IPAddress, IPAddress> foundTranslation = OtherDomainSNPPAddressTranslation[domainAddress].Find(x => x.Item1 == pathBegin);
                    IPAddress translatedAddress = foundTranslation.Item2;
                    pathBegin = translatedAddress;
                }
                else if (IPAddressExtensions.IsInSameSubnet(pathEnd, domainAddress.subnetAddress, domainAddress.subnetMask))
                {
                    Random random = new Random();
                    List<Tuple<IPAddress, IPAddress>> translationsList = OtherDomainSNPPAddressTranslation[domainAddress];
                    Tuple<IPAddress, IPAddress> foundTranslation = translationsList[random.Next(translationsList.Count)];
                    IPAddress translatedAddress = foundTranslation.Item1;
                    pathEnd = translatedAddress;
                }
            }

            //1. Bierze adresy SNPP, miedzy ktorymi ma zestawić
            //2. Robi jakiegoś Djikstre, u nas Floyda bo Komando pozwolił
            //3. Zwraca wyznaczoną ścieżkę

            List<SNPP> scheduled = router.route(pathBegin, pathEnd);
            return scheduled;
        }

        private void LocalTopologyIn(bool delete, SNP localTopologyUpdate)
        {

            List<SNP> existingSNPs = SNPsbySNPPaddress[localTopologyUpdate.Address];
            //sprawdz czy istnieje taki SNP
            while (existingSNPs.Find(x => x.Label == localTopologyUpdate.Label) != null)
            {
                // jeśli TRUE to usuwamy z List<SNP>, jeśli FALSE to dodajemy
                if (delete)
                {
                    existingSNPs.Remove(localTopologyUpdate);
                    LogClass.Log("Remove " + localTopologyUpdate + " from local topology");
                }
                else
                {
                    existingSNPs.Add(localTopologyUpdate);
                    LogClass.Log("Add " + localTopologyUpdate + " to local topology");
                }

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




