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
        public const String OPERATED_SUBNETWORK = "operatedSubnetwork";


        private static ConnectionController connectionController;
        private static RoutingController routingController;
        private static LinkResourceManager linkResourceManager;
        private static CSocket listeningSocket;
        private static CSocket toParentSocket;
        private static Dictionary<String, CSocket> SocketsByAddress;

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
            toParentSocket = new CSocket(parentSubnetworkAddress, parentSubnetworkPort, CSocket.CONNECT_FUNCTION);
        }

        public static void initListeningCustomSocket()
        {
            IPAddress parentSubnetworkAddress = IPAddress.Parse("127.0.0.1");
            int port = Config.getIntegerProperty("ListeningPort");
            listeningSocket = new CSocket(parentSubnetworkAddress, port, CSocket.LISTENING_FUNCTION);
            listeningSocket.Listen();
            ListenForConnections();
        }

        public static void ListenForConnections()
        {
            initListenThread();
        }

        private static Thread initListenThread()
        {
            Console.WriteLine("Listening for subnetwork connections");
            var t = new Thread(() => RealStart());
            t.IsBackground = true;
            t.Start();
            return t;
        }

        private static void RealStart()
        {
            while (true)
            {
                CSocket connected=listeningSocket.Accept();
                Console.WriteLine("connected");
                waitForInputFromSocketInAnotherThread(connected);
            }
       
        }

        private static void waitForInputFromSocketInAnotherThread(CSocket connected)
        {
            var t = new Thread(() => waitForInput(connected));
            t.Start();
        }

        private static void waitForInput(CSocket connected)
        {
            ProcessSubnetworkInformations(connected);
            while (true)
            {
                Tuple<String, Object> received = connected.ReceiveObject();
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
        }

        private static void ProcessSubnetworkInformations(CSocket connectedSocket)
        {
            Tuple<string, object>received = connectedSocket.ReceiveObject();
            Dictionary<string, string> receivedInformation = (Dictionary<string, string>)received.Item2;
            String operatedSubnetwork = receivedInformation[OPERATED_SUBNETWORK];
            SocketsByAddress.Add(operatedSubnetwork, connectedSocket);
        }

        private static void insertSNPPSToRC(List<SNPP> receivedList)
        {
            for (int i = 0; i < receivedList.Count; i++)
                routingController.addSNPP(receivedList.ElementAt(i));
        }    

    }
}
