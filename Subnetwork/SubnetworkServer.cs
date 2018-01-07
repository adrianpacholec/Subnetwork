﻿using CustomSocket;
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
        public const String OPERATED_SUBNETWORK_MASK = "operatedSubnetworkMask";
        public const String CONNECTION_REQEST = "connectionRequest";
        public const char PARAM_SEPARATOR = ' ';
        public const int SUBNETWORK_ADDRESS_POSITION = 0;
        public const int SUBNETWORK_MASK_POSITION = 1;
        public const int PORT_POSITION = 2;


        private static ConnectionController connectionController;
        private static RoutingController routingController;
        private static LinkResourceManager linkResourceManager;
        private static CSocket listeningSocket;
        private static CSocket toParentSocket;
        private static CSocket toNCCSocket;
        private static Dictionary<SubnetworkAddress, CSocket> SocketsByAddress;
        private static Dictionary<SubnetworkAddress, int> SocketsToAnotherDomains;

        public static void init(ConnectionController cc, RoutingController rc, LinkResourceManager lrm)
        {
            connectionController = cc;
            routingController = rc;
            linkResourceManager = lrm;
            SocketsByAddress = new Dictionary<SubnetworkAddress, CSocket>();
            SocketsToAnotherDomains = new Dictionary<SubnetworkAddress, int>();
            String parentSubnetworkAddress = Config.getProperty("ParentSubnetworkAddress");
            if (parentSubnetworkAddress != null)
            {
                int parentSubnetworkPort = Config.getIntegerProperty("ParentSubnetworkPort");
                ConnectToParentSubnetwork(IPAddress.Parse(parentSubnetworkAddress), parentSubnetworkPort);
                SendMySubnetworkInformation();
            }
            initListeningCustomSocket();
            LoadPortsToAnotherDomains();
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

        private static void LoadPortsToAnotherDomains()
        {
            string fileName = Config.getProperty("portsToDomains");
            string[] loadedFile = LoadFile(fileName);
            string[] parameters = null;
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss")+" loaded ports to another domains");
            foreach (string str in loadedFile)
            {
                parameters = str.Split(PARAM_SEPARATOR);
                SocketsToAnotherDomains.Add(new SubnetworkAddress(parameters[SUBNETWORK_ADDRESS_POSITION], parameters[SUBNETWORK_MASK_POSITION]), Int32.Parse(parameters[PORT_POSITION]));
            }
        }

        private static string[] LoadFile(String fileName)
        {
            string[] fileLines = System.IO.File.ReadAllLines(fileName);
            return fileLines;
        }

        public static void SendConnectionRequest(SNP pathBegin, SNP pathEnd, SubnetworkAddress subnetworkAddress)
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
            CSocket otherDomainSocket = GetSocketToDomain(AddressPathEnd);
            otherDomainSocket.SendObject(PEER_COORDINATION,peerTuple);
            object zrobictuzebyodbieraltruealbofalse=otherDomainSocket.ReceiveObject();
            otherDomainSocket.Close();
        }

        public static CSocket GetSocketToDomain(string address)
        {
            IPAddress ipAddress = IPAddress.Parse(address);
            SubnetworkAddress found= null;
            foreach (SubnetworkAddress domainAddress in SocketsByAddress.Keys)
            {
                if (IPAddressExtensions.IsInSameSubnet(ipAddress, domainAddress.subnetAddress, domainAddress.subnetMask))
                    found = domainAddress;
            }
            return createSocketToOtherDomain(found);
        }

        public static CSocket createSocketToOtherDomain(SubnetworkAddress address)
        {
            int port=SocketsToAnotherDomains[address];
            CSocket socket = new CSocket(IPAddress.Parse("localhost"), port, CSocket.CONNECT_FUNCTION);
            return socket;
        }

        private static object getSubnetworkInformation()
        {
            Dictionary<string, string> mySubnetworkInformation = new Dictionary<string, string>();
            string mySubnetworkAddress = Config.getProperty(OPERATED_SUBNETWORK);
            string mySubnetworkMask = Config.getProperty(OPERATED_SUBNETWORK_MASK);
            SubnetworkAddress address = new SubnetworkAddress(mySubnetworkAddress, mySubnetworkMask);
            mySubnetworkInformation.Add(OPERATED_SUBNETWORK, mySubnetworkAddress);
            mySubnetworkInformation.Add(OPERATED_SUBNETWORK_MASK, mySubnetworkMask);
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
                    Tuple<SNP, string> receivedPair = (Tuple<SNP, string>)receivedObject;
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
                String operatedSubnetworkMask = receivedInformation[OPERATED_SUBNETWORK_MASK];
                SubnetworkAddress connectedSubnetAddress = new SubnetworkAddress(operatedSubnetwork, operatedSubnetworkMask);
                SocketsByAddress.Add(connectedSubnetAddress, connectedSocket);
                LogClass.Log("Subnetwork " + operatedSubnetwork + " connected");
            }
        }

        private static void insertSNPPSToRC(List<SNPP> receivedList)
        {
            for (int i = 0; i < receivedList.Count; i++)
                routingController.AddSNPP(receivedList.ElementAt(i));
        }

            public static void SendTopologyUpdateToRC(bool delete, SNP localTopologyUpdate)
        {
            // Maciek zrób tu co chcesz xD
        }
    }
}