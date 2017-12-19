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

        public RoutingController()
        {
            SNPPList = new List<SNPP>();
        }

        private List<SNPP> RouteTableQuery(string pathBegin, string pathEnd)
        {
            //1. Bierze adresy SNPP, miedzy ktorymi ma zestawić
            //2. Robi jakiegoś Djikstre
            //3. Zwraca wyznaczoną ścieżkę
            return new List<SNPP>();
        }

        private void LocalTopologyIn(SNPP localTopologyUpdate)
        {
            //updatuje sobie SNPP w swojej liscie


        }

        private void NetworkTopologyIn(SNPP localTopologyUpdate)
        {

        }

        private void NetworkTopologyOut(SNPP localTopologyUpdate)
        {

        }

        internal void addSNPP(SNPP sNPP)
        {
            throw new NotImplementedException();
        }
    }
}
