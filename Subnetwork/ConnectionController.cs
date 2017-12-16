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


        private List<SNPP> RouteTableQuery()
        {
            return new List<SNPP>();
        }

        private void LinkConnectionRequest()
        {


        }

        private void ConnectionRequestIn()
        {


        }

        private void ConnectionRequestOut()
        {

        }

        private void PeerCoordinationIn()
        {

        }

        private void PeerCoordinationOut()
        {

        }

    }
}
