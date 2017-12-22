using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class RoutingController
    {
        private List<SNPP> SNPPList;
        private Dictionary<string, List<SNP>> SNPsbySNPPaddress;
        private Dictionary<string, List<Tuple<string, string>>> OtherDomainSNPPAddressTranslation;
        private string SubnetworkAddress = null;

        public RoutingController()
        {
            SNPPList = new List<SNPP>();
        }

        private List<SNPP> RouteTableQuery(string pathBegin, string pathEnd, int capacity)
        {
            foreach (string domainAddress in OtherDomainSNPPAddressTranslation.Keys)
            {
                string MASKA_PODSIECI_INNEJ_DOMENY = "255.255.255.0";   //NO TO POPRAWIC

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
                }
                else
                {
                    existingSNPs.Add(localTopologyUpdate);
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




