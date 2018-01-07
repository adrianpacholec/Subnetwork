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
        public const String CONNECTION_REQUEST_FROM_NCC = "connectionRequestFromNCC";
        public const String PEER_COORDINATION = "peerCoordination";
        public const String SNPP_SUBNETWORK_INFORMATION = "snppSubnetworkInformation";
        public const String NETWORK_TOPOLOGY = "networkTopology";
        public const String OPERATED_SUBNETWORK = "operatedSubnetwork";
        public const String CONNECTION_REQEST = "connectionRequest";


        private static ConnectionController connectionController;
        private static RoutingController routingController;
        private static LinkResourceManager linkResourceManager;
        private static CSocket listeningSocket;
        private static CSocket toParentSocket;
        private static CSocket toNCCSocket;
        private static Dictionary<String, CSocket> SocketsByAddress;

        public static void init(ConnectionController cc, RoutingController rc, LinkResourceManager lrm)
        {
            connectionController = cc;
            routingController = rc;
            linkResourceManager = lrm;
            SocketsByAddress = new Dictionary<string, CSocket>();
            String parentSubnetworkAddress = Config.getProperty("ParentSubnetworkAddress");
            if (parentSubnetworkAddress != null)
            {
                int parentSubnetworkPort = Config.getIntegerProperty("ParentSubnetworkPort");
                ConnectToParentSubnetwork(IPAddress.Parse(parentSubnetworkAddress), parentSubnetworkPort);
                SendMySubnetworkInformation();
            }
            initListeningCustomSocket();
        }

        private static void ConnectToParentSubnetwork(IPAddress parentSubnetworkAddress, int parentSubnetworkPort)
        {
            toParentSocket = new CSocket(parentSubnetworkAddress, parentSubnetworkPort, CSocket.CONNECT_FUNCTION);
        }

        private static void SendMySubnetworkInformation()
        {
            object toSend = getSubnetworkInformation();
            toParentSocket.SendObject(OPERATED_SUBNETWORK, toSend);

        }

        public static void SendConnectionRequest(SNP pathBegin, SNP pathEnd, string subnetworkAddress)
        {
            Tuple<SNP, SNP> connTuple = new Tuple<SNP, SNP>(pathBegin, pathEnd);
            CSocket childSubSocket;
            bool hasValue = SocketsByAddress.TryGetValue(subnetworkAddress, out childSubSocket);
            if (hasValue)
            {
                childSubSocket.SendObject(CONNECTION_REQEST, connTuple);
            }
        }

        public static void SendPeerCoordination(SNP SNPpathBegin, string AddressPathEnd)
        {
            //zakładam, że serwer subnetworka z drugiej domeny podepnie się analogicznie 
            //jak serwer podsieci w tej domenie i zostanie zapamiętany jego socket w słowniku.
            //i teraz dam tak, że w słoniku po tym adresie on wyszuka ten socket, potem się to zmieni

            Tuple<SNP, string> peerTuple = new Tuple<SNP, string>(SNPpathBegin, AddressPathEnd);
            CSocket otherDomainSocket;
            bool hasValue = SocketsByAddress.TryGetValue(AddressPathEnd, out otherDomainSocket);
            if (hasValue)
            {
                otherDomainSocket.SendObject(CONNECTION_REQEST, peerTuple);
            }
        }

        private static object getSubnetworkInformation()
        {
            Dictionary<string, string> mySubnetworkInformation = new Dictionary<string, string>();
            string mySubnetworkAddress = Config.getProperty(OPERATED_SUBNETWORK);
            mySubnetworkInformation.Add(OPERATED_SUBNETWORK, mySubnetworkAddress);
            return mySubnetworkInformation;
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
            LogClass.Log("Listening for subnetwork connections");
            var t = new Thread(() => RealStart());
            t.IsBackground = true;
            t.Start();
            return t;
        }

        private static void RealStart()
        {
            while (true)
            {
                CSocket connected = listeningSocket.Accept();
                LogClass.Log("Connected.");
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
            ProcessConnectInformations(connected);
            while (true)
            {
                Tuple<String, Object> received = connected.ReceiveObject();
                String parameter = received.Item1;
                Object receivedObject = received.Item2;
                if (parameter.Equals(SNPP_SUBNETWORK_INFORMATION))
                {
                    insertSNPPSToRC((List<SNPP>)receivedObject);
                }
                else if (parameter.Equals(CONNECTION_REQUEST_FROM_NCC))
                {
                    MessageParameters parameters = (MessageParameters)receivedObject;
                    String sourceIP = parameters.getFirstParameter();
                    String destinationIP = parameters.getSecondParameter();
                    int capacity = parameters.getCapacity();
                    LogClass.Log("Received CONNECTION REQUEST from NCC.");
                    connectionController.ConnectionRequestFromNCC(sourceIP, destinationIP, capacity);

                }
                else if (parameter.Equals(PEER_COORDINATION))
                {
                    Tuple<SNP, SNPP> receivedPair = (Tuple<SNP, SNPP>)receivedObject;
                    connectionController.PeerCoordinationIn(receivedPair.Item1, receivedPair.Item2);
                    LogClass.Log("Received PEER COORDINATION from AS_1");
                }
                else if (parameter.Equals(NETWORK_TOPOLOGY))
                {

                }
            }
        }

        private static void ProcessConnectInformations(CSocket connectedSocket)
        {
            Tuple<string, object> received = connectedSocket.ReceiveObject();
            Dictionary<string, string> receivedInformation = (Dictionary<string, string>)received.Item2;
            if (receivedInformation.Count == 0)
            {
                toNCCSocket = connectedSocket;

            }
            else
            {
                String operatedSubnetwork = receivedInformation[OPERATED_SUBNETWORK];
                SocketsByAddress.Add(operatedSubnetwork, connectedSocket);
                LogClass.Log("Subnetwork " + operatedSubnetwork + " connected");
            }
        }

        private static void insertSNPPSToRC(List<SNPP> receivedList)
        {
            for (int i = 0; i < receivedList.Count; i++)
                routingController.AddSNPP(receivedList.ElementAt(i));
        }


    }
}