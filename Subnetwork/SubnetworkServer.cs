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
        public const String OPERATED_SUBNETWORK = "SubnetworkAddress";
        public const String OPERATED_SUBNETWORK_MASK = "SubnetworkMask";
        public const String CONNECTION_REQUEST_FROM_CC = "connectionRequest";
        public const String DELETE_CONNECTION_REQUEST = "deleteRequest";
        public const String DELETE_PEER_COORDINATION = "deletePeerCoordination";
        public const String CALL_TEARDOWN = "callTeardown";
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
        private static CSocket childSubSocket;
        private static CSocket connected;
        private static Dictionary<SubnetworkAddress, CSocket> SocketsByAddress;
        private static Dictionary<SubnetworkAddress, int> SocketsToAnotherDomains;
        private static bool acked;
        private static bool nacked;

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
            LoadPortsToAnotherDomains();
            InitListeningCustomSocket();
        }

        private static void ConnectToParentSubnetwork(IPAddress parentSubnetworkAddress, int parentSubnetworkPort)
        {
            toParentSocket = new CSocket(parentSubnetworkAddress, parentSubnetworkPort, CSocket.CONNECT_FUNCTION);
        }

        private static void SendMySubnetworkInformation()
        {
            object toSend = GetSubnetworkInformation();
            toParentSocket.SendObject(OPERATED_SUBNETWORK, toSend);
            WaitForInputFromSocketInAnotherThread(toParentSocket);

        }

        private static void LoadPortsToAnotherDomains()
        {
            string fileName = Config.getProperty("realPortsToDomains");
            string[] loadedFile = LoadFile(fileName);
            string[] parameters = null;
            LogClass.WhiteLog("Loaded ports to another domains.");

            foreach (string str in loadedFile)
            {
                if (str[0] != '#')
                {
                    parameters = str.Split(PARAM_SEPARATOR);
                    SocketsToAnotherDomains.Add(new SubnetworkAddress(parameters[SUBNETWORK_ADDRESS_POSITION], parameters[SUBNETWORK_MASK_POSITION]), Int32.Parse(parameters[PORT_POSITION]));
                }
            }
        }

        private static string[] LoadFile(String fileName)
        {
            string[] fileLines = System.IO.File.ReadAllLines(fileName);
            return fileLines;
        }

        public static bool SendConnectionRequest(SNP pathBegin, SNP pathEnd, SubnetworkAddress subnetworkAddress)
        {
            Tuple<SNP, SNP> connTuple = new Tuple<SNP, SNP>(pathBegin, pathEnd);
            bool hasValue = SocketsByAddress.TryGetValue(subnetworkAddress, out childSubSocket);
            if (hasValue)
            {
                if (pathBegin.Deleting)
                    childSubSocket.SendObject(DELETE_CONNECTION_REQUEST, connTuple);
                else
                    childSubSocket.SendObject(CONNECTION_REQUEST_FROM_CC, connTuple);

                while (!(acked || nacked))
                    Thread.Sleep(30);

                if (acked)
                {
                    if (pathBegin.Deleting)
                        LogClass.MagentaLog("Subnetwork " + subnetworkAddress.subnetAddress + " deleted connection between: " + pathBegin.Address + " and " + pathEnd.Address);
                    else
                        LogClass.GreenLog("Subnetwork " + subnetworkAddress.subnetAddress + " set connection between: " + pathBegin.Address + " and " + pathEnd.Address);
                    acked = false;
                    return true;
                }
                else if (nacked)
                {
                    if (pathBegin.Deleting)
                        LogClass.Log("Subnetwork " + subnetworkAddress.subnetAddress + " can't delete the connection between: " + pathBegin.Address + " and " + pathEnd.Address);
                    else
                        LogClass.Log("Subnetwork " + subnetworkAddress.subnetAddress + " can't set up the connection between: " + pathBegin.Address + " and " + pathEnd.Address);
                    nacked = false;
                    return false;
                }
            }
            else
            {
                LogClass.Log("Can't find subnetwork: " + subnetworkAddress.ToString());
            }
            return false;
        }

        public static void CallDeleteLinkConnectionRequestInLRM(SNP SNPpathBegin, SNP SNPpathEnd, int capacity)
        {
            linkResourceManager.SNPLinkConnectionRequest(SNPpathBegin, SNPpathEnd, capacity);
        }

        public static void SendDeleteConnectionRequest(string pathBeginAddress, string pathEndAddress, SubnetworkAddress subnetAddress)
        {
            Tuple<string, string> deleteTuple = new Tuple<string, string>(pathBeginAddress, pathEndAddress);
            CSocket childSubSocket;
            bool hasValue = SocketsByAddress.TryGetValue(subnetAddress, out childSubSocket);
            if (hasValue)
            {
                childSubSocket.SendObject(DELETE_CONNECTION_REQUEST, deleteTuple);
            }
            else
            {
                LogClass.Log("Can't find subnetwork: " + subnetAddress.ToString());
            }
        }

        public static bool SendPeerCoordination(SNP SNPpathBegin, string AddressPathEnd, bool val)

        {
            Tuple<SNP, string> peerTuple = new Tuple<SNP, string>(SNPpathBegin, AddressPathEnd);
            CSocket otherDomainSocket = GetSocketToDomain(AddressPathEnd);
            if (val)
            {
                otherDomainSocket.SendObject(null, new Dictionary<string, string>());
                otherDomainSocket.SendObject(PEER_COORDINATION, peerTuple);
            }
            else
            {
                otherDomainSocket.SendObject(null, new Dictionary<string, string>());
                otherDomainSocket.SendObject(DELETE_PEER_COORDINATION, peerTuple);
            }
            string response = otherDomainSocket.ReceiveObject().Item1;
            //otherDomainSocket.Close();
            if (response.Equals(CSocket.ACK_FUNCTION))
                return true;
            else return false;
        }

        public static Tuple<SNP, SNP> callLinkConnectionRequestInLRM(SNPP connectionBegin, SNPP connectionEnd, int capacity)
        {
            Tuple<SNP, SNP> SNPpair = linkResourceManager.SNPLinkConnectionRequest(connectionBegin, connectionEnd, capacity);
            return SNPpair;
        }

        private static bool callConnectionRequest(SNP pathBegin, SNP pathEnd)
        {
            return connectionController.ConnectionRequestFromCC(pathBegin, pathEnd);
        }

        public static CSocket GetSocketToDomain(string address)
        {
            IPAddress ipAddress = IPAddress.Parse(address);
            SubnetworkAddress found = null;
            foreach (SubnetworkAddress domainAddress in SocketsToAnotherDomains.Keys)
            {
                if (IPAddressExtensions.IsInSameSubnet(ipAddress, domainAddress.subnetAddress, domainAddress.subnetMask))
                    found = domainAddress;
            }
            return createSocketToOtherDomain(found);
        }

        public static CSocket createSocketToOtherDomain(SubnetworkAddress address)
        {
            int port = SocketsToAnotherDomains[address];
            CSocket socket = new CSocket(IPAddress.Parse("127.0.0.1"), port, CSocket.CONNECT_FUNCTION);
            return socket;
        }

        public static List<SNPP> callRouteTableQueryInRC(string pathBegin, string pathEnd, int capacity)
        {
            return routingController.RouteTableQuery(IPAddress.Parse(pathBegin), IPAddress.Parse(pathEnd), capacity);
        }

        private static object GetSubnetworkInformation()
        {
            Dictionary<string, string> mySubnetworkInformation = new Dictionary<string, string>();
            string mySubnetworkAddress = Config.getProperty(OPERATED_SUBNETWORK);
            string mySubnetworkMask = Config.getProperty(OPERATED_SUBNETWORK_MASK);
            SubnetworkAddress address = new SubnetworkAddress(mySubnetworkAddress, mySubnetworkMask);
            mySubnetworkInformation.Add(OPERATED_SUBNETWORK, mySubnetworkAddress);
            mySubnetworkInformation.Add(OPERATED_SUBNETWORK_MASK, mySubnetworkMask);
            return mySubnetworkInformation;
        }

        public static void InitListeningCustomSocket()
        {
            IPAddress parentSubnetworkAddress = IPAddress.Parse("127.0.0.1");
            int port = Config.getIntegerProperty("ListeningPort");
            listeningSocket = new CSocket(parentSubnetworkAddress, port, CSocket.LISTENING_FUNCTION);
            listeningSocket.Listen();
            ListenForConnections();
        }

        public static void ListenForConnections()
        {
            InitListenThread();
        }

        private static Thread InitListenThread()
        {
            LogClass.WhiteLog("LISTENING FOR SUBNETWORKS");
            var t = new Thread(() => RealStart());
            t.IsBackground = true;
            t.Start();
            return t;
        }

        private static void RealStart()
        {
            while (true)
            {
                connected = listeningSocket.Accept();
                WaitForInputFromSocketInAnotherThread(connected);
            }

        }

        private static void WaitForInputFromSocketInAnotherThread(CSocket connected)
        {
            var t = new Thread(() => WaitForInput(connected));
            t.Start();
        }

        private static void WaitForInput(CSocket connected)
        {
            if (!connected.Equals(toParentSocket))
                ProcessConnectInformation(connected);
            while (true)
            {
                Tuple<String, Object> received = connected.ReceiveObject();
                String parameter = received.Item1;
                Object receivedObject = received.Item2;
                if (parameter.Equals(SNPP_SUBNETWORK_INFORMATION))
                {
                    InsertSNPPSToRC((List<SNPP>)receivedObject);
                }
                else if (parameter.Equals(CONNECTION_REQUEST_FROM_NCC))
                {
                    MessageParameters parameters = (MessageParameters)receivedObject;
                    String sourceIP = parameters.getFirstParameter();
                    String destinationIP = parameters.getSecondParameter();
                    int capacity = parameters.getCapacity();
                    LogClass.CyanLog("Received CONNECTION REQUEST from NCC.");
                    bool success = connectionController.ConnectionRequestFromNCC(sourceIP, destinationIP, capacity);
                    String parentSubnetworkAddress = "127.0.0.1";
                    CSocket c = new CSocket(IPAddress.Parse(parentSubnetworkAddress), 40000, CSocket.CONNECT_FUNCTION);
                    SendACKorNACK(success, c);

                }
                else if (parameter.Equals(PEER_COORDINATION))
                {
                    Tuple<SNP, string> receivedPair = (Tuple<SNP, string>)receivedObject;
                    LogClass.CyanLog("Received PEER COORDINATION from AS 1");
                    bool success = connectionController.PeerCoordinationIn(receivedPair.Item1, receivedPair.Item2);
                    SendACKorNACK(success, connected);
                }
                else if (parameter.Equals(DELETE_PEER_COORDINATION))
                {
                    Tuple<SNP, string> receivedPair = (Tuple<SNP, string>)receivedObject;
                    LogClass.CyanLog("Received DELETE PEER COORDINATION from AS 1");
                    bool success = connectionController.DeletePeerCoordinationIn(receivedPair.Item1, receivedPair.Item2);
                    SendACKorNACK(success, connected);
                }
                else if (parameter.Equals(NETWORK_TOPOLOGY))
                {

                }
                else if (parameter.Equals(CONNECTION_REQUEST_FROM_CC))
                {
                    Tuple<SNP, SNP> pathToAssign = (Tuple<SNP, SNP>)received.Item2;
                    SNP first = pathToAssign.Item1;
                    SNP second = pathToAssign.Item2;
                    LogClass.CyanLog("Received CONNECTION REQUEST to set connection between " + first.Address + " and " + second.Address);
                    bool response = callConnectionRequest(pathToAssign.Item1, pathToAssign.Item2);
                    SendACKorNACK(response, connected);

                }
                else if (parameter.Equals(CALL_TEARDOWN))
                {
                    MessageParameters parameters = (MessageParameters)receivedObject;
                    String sourceIP = parameters.getFirstParameter();
                    String destinationIP = parameters.getSecondParameter();
                    LogClass.CyanLog("Received TEARDOWN to deallocate connection between " + sourceIP + " and " + destinationIP);
                    bool success = connectionController.DeleteConnection(sourceIP, destinationIP);
                    String parentSubnetworkAddress = "127.0.0.1";
                    CSocket c = new CSocket(IPAddress.Parse(parentSubnetworkAddress), 40000, CSocket.CONNECT_FUNCTION);
                    SendACKorNACK(success, c);
                }
                else if (parameter.Equals(DELETE_CONNECTION_REQUEST))
                {
                    Tuple<SNP, SNP> pathToDelete = (Tuple<SNP, SNP>)received.Item2;
                    string pathBegin = pathToDelete.Item1.Address;
                    string pathEnd = pathToDelete.Item2.Address;
                    LogClass.CyanLog("Received DELETE CONNECTION REQUEST to delete connection between " + pathBegin + " and " + pathEnd);
                    bool success = connectionController.DeleteConnection(pathBegin, pathEnd);
                    SendACKorNACK(success, connected);
                }
                else if (parameter.Equals(CSocket.ACK_FUNCTION))
                {
                    LogClass.CyanLog("Received ACK");
                    acked = true;
                }
                else if (parameter.Equals(CSocket.NACK_FUNCTION))
                {
                    LogClass.CyanLog("Received NACK");
                    nacked = true;
                }
            }
        }

        private static void ProcessConnectInformation(CSocket connectedSocket)
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
                LogClass.Log("Subnetwork " + operatedSubnetwork + " connected.");
            }
        }

        private static void SendACKorNACK(bool success, CSocket connected)
        {
            if (success)
                connected.SendACK();
            else
                connected.SendNACK();
        }

        private static void InsertSNPPSToRC(List<SNPP> receivedList)
        {
            for (int i = 0; i < receivedList.Count; i++)
                routingController.AddSNPP(receivedList.ElementAt(i));
        }

        public static void SendTopologyUpdateToRC(bool delete, SNP localTopologyUpdate)
        {
            routingController.LocalTopologyIn(delete, localTopologyUpdate);
        }

        internal static void callIgnoreLinkInRC(SNP snpPathBegin)
        {
            routingController.IgnoreLink(snpPathBegin);
        }
    }
}