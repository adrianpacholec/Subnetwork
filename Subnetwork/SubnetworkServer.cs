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
        private static CSocket listeningSocket;
        private static CSocket toParentSocket;

        public static void init(ConnectionController cc, RoutingController rc, LinkResourceManager lrm)
        {
            connectionController = cc;
            routingController = rc;
            linkResourceManager = lrm;
            String parentSubnetworkAddress = Config.getProperty("ParentSubnetworkAddress");
            if (parentSubnetworkAddress != null)
            {
                int parentSubnetworkPort = Config.getIntegerProperty("ParentSubnetworkPort");
                ConnectToParentSubnetwork(IPAddress.Parse(parentSubnetworkAddress), parentSubnetworkPort);
            }
            initListeningCustomSocket();
        }

        private static void ConnectToParentSubnetwork(IPAddress parentSubnetworkAddress, int parentSubnetworkPort)
        {
            toParentSocket = new CSocket(parentSubnetworkAddress, parentSubnetworkPort);
        }

        public static void initListeningCustomSocket()
        {
            IPAddress parentSubnetworkAddress = IPAddress.Parse("127.0.0.1");
            int port = Config.getIntegerProperty("ListeningPort");
            listeningSocket = new CSocket(parentSubnetworkAddress, port);
            listeningSocket.Listen();
            ListenForConnections();
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
            CSocket connected=listeningSocket.Accept();
       
        }

        private static void waitForInputFromSocketInAnotherThread(Socket connected)
        {
            var t = new Thread(() => waitForInput());
            t.Start();
        }

        private static void waitForInput()
        {
            Tuple<String, Object> received = listeningSocket.ReceiveObject();
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
