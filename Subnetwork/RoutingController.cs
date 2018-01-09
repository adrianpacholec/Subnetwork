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
        private Dictionary<string, List<Tuple<string, string>>> OtherDomainSNPPAddressTranslation;
        private string SubnetworkAddress = null;
        Router router;

        //nie wiem czy to potrzebne, ale przekazuje to do routera wiec moze tez tu sie przyda
        private List<SubnetworkAddress> containedSubnetworks;
        private List<Link> links;
        
        internal void testRouting()
        {
            RouteTableQuery("10.1.64.0", "10.1.196.0", 20);
        }

        public RoutingController(List<SubnetworkAddress> containedSubnetworks, List<Link> links)
        {
            this.containedSubnetworks = containedSubnetworks;
            this.links = links;
            SNPPList = new List<SNPP>();
            router = new Router(containedSubnetworks, links);
            OtherDomainSNPPAddressTranslation = new Dictionary<string, List<Tuple<string, string>>>();
            SNPsbySNPPaddress = new Dictionary<string, List<SNP>>();
        }

        private List<SNPP> RouteTableQuery(string pathBegin, string pathEnd, int capacity)
        {
            foreach (string domainAddress in OtherDomainSNPPAddressTranslation.Keys)
            {
                string MASKA_PODSIECI_INNEJ_DOMENY = "255.255.0.0";   //NO TO POPRAWIC

                //sprawdza, z ktorej domeny przyszedl SNP i podmienia jego adres na adres swojego SNPP brzegowego
                if (IPAddressExtensions.IsInSameSubnet(IPAddress.Parse(pathBegin), IPAddress.Parse(domainAddress), IPAddress.Parse(MASKA_PODSIECI_INNEJ_DOMENY)))
                {
                    Tuple<string, string> foundTranslation = OtherDomainSNPPAddressTranslation[domainAddress].Find(x => x.Item1 == pathBegin);
                    string translatedAddress = foundTranslation.Item2;
                    pathBegin = translatedAddress;
                }
                else if (IPAddressExtensions.IsInSameSubnet(IPAddress.Parse(pathEnd), IPAddress.Parse(domainAddress), IPAddress.Parse(MASKA_PODSIECI_INNEJ_DOMENY)))
                {
                    Random random = new Random();
                    List<Tuple<string, string>> translationsList = OtherDomainSNPPAddressTranslation[domainAddress];
                    Tuple<string, string> foundTranslation = translationsList[random.Next(translationsList.Count)];
                    string translatedAddress = foundTranslation.Item1;
                    pathEnd = translatedAddress;
                }
            }

            //1. Bierze adresy SNPP, miedzy ktorymi ma zestawić
            //2. Robi jakiegoś Djikstre, u nas Floyda bo Komando pozwolił
            //3. Zwraca wyznaczoną ścieżkę

            List<SNPP> scheduled = router.route(IPAddress.Parse(pathBegin), IPAddress.Parse(pathEnd));
            return new List<SNPP>();
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




