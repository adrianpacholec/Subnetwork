using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class RoutingController
    {
        private List<SNPP> SNPPList;
        private Dictionary<string, List<SNP>> SNPsbySNPPaddress;
        private Dictionary<string, List<Tuple<string, string>>> domainAddress;

        public RoutingController()
        {
            SNPPList = new List<SNPP>();
        }

        private List<SNPP> RouteTableQuery(string pathBegin, string pathEnd, int capacity)
        {
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
                    Console.WriteLine("Remove " + localTopologyUpdate + " from local topology");
                }
                else
                {
                    existingSNPs.Add(localTopologyUpdate);
                    Console.WriteLine("Add " + localTopologyUpdate + " to local topology");
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
