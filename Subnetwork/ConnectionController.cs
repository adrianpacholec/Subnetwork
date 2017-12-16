using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomSocket;


namespace Subnetwork
{
    class ConnectionController
    {

        private List<CSocket> sockets;
        private string NetworkAddress, ParentNetworkAddress;
        public ConnectionController()
        {
            NetworkAddress = Config.getProperty("NetworkAddress");
            ParentNetworkAddress = Config.getProperty("ParentNetworkAddress");

        }


        private List<SNPP> RouteTableQuery(string pathBegin, string pathEnd)
        {
            return new List<SNPP>();
        }

        private void LinkConnectionRequest(SNP SNPpathBegin, SNP SNPpathEnd)
        {
          

        }

        private bool ConnectionRequestIn(string pathBegin, string pathEnd)
        {

            return true;  //Jesli polaczenie zestawiono poprawnie
        }

        private bool ConnectionRequestOut(string pathBegin, string pathEnd)
        {
            return true; //Jesli polaczenie zestawiono poprawnie
        }

        private bool PeerCoordinationIn(SNPP SNPPpathBegin, SNP SNPpathEnd)
        {
            return true;
        }

        private bool PeerCoordinationOut()
        {
            return true;
        }

    }
}
