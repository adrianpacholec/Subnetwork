using CustomSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Subnetwork
{
    class SubnetworkServer
    {
        public const String CONNECTION_REQUEST = "connectionRequest";
        public const String PEER_COORDINATION = "peerCoordination";
        public const String SNPP_SUBNETWORK_INFORMATION = "snppSubnetworkInformation";
        public const String NETWORK_TOPOLOGY = "networkTopology";

        private static ConnectionController connectionController;
        private static RoutingController routingController;
        private static LinkResourceManager linkResourceManager;
        private static CSocket csocket;

        public static void init(ConnectionController cc, RoutingController rc, LinkResourceManager lrm)
        {
            connectionController = cc;
            routingController = rc;
            linkResourceManager = lrm;
            initListeningCustomSocket();
        }

        public static void initListeningCustomSocket()
        {
            String parentAddress = Config.getProperty("ParentSubnetworkAddress");
            IPAddress parentSubnetworkAddress = IPAddress.Parse(parentAddress);
            int port = Config.getIntegerProperty("port");
            csocket = new CSocket(parentSubnetworkAddress, port);
        }

        public static void ListenForConnections()
        {
            initListenThread();
        }

        private static Thread initListenThread()
        {
            var t = new Thread(() => RealStart());
            t.IsBackground = true;
            t.Start();
            return t;
        }

        private static void RealStart()
        {
            Socket connected=csocket.Accept();
       
        }

        private static void waitForInputFromSocketInAnotherThread(Socket connected)
        {
            var t = new Thread(() => waitForInput());
            t.Start();
        }

        private static void waitForInput()
        {
            Tuple<String, Object> received = csocket.ReceiveObject();
            String parameter = received.Item1;
            Object receivedObject = received.Item2;
            if (parameter.Equals(SNPP_SUBNETWORK_INFORMATION))
            {
                insertSNPPSToRC((List<SNPP>)receivedObject);
            }
            else if (parameter.Equals(CONNECTION_REQUEST))
            {

            }
            else if (parameter.Equals(PEER_COORDINATION))
            {

            }
            else if (parameter.Equals(NETWORK_TOPOLOGY))
            {

            }

        }

        private static void insertSNPPSToRC(List<SNPP> receivedList)
        {
            for (int i = 0; i < receivedList.Count; i++)
                routingController.addSNPP(receivedList.ElementAt(i));
        }
        
    }
}
