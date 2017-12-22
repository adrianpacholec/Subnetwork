using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomSocket;
using System.Net;

namespace Subnetwork
{
    class ConnectionController
    {
        public const char PARAM_SEPARATOR = ' ';
        public const int ADDRESS_POSITION = 0;
        public const int MASK_POSITION = 1;
        public const int FIRST_ADDRESS_POS = 0;
        public const int FIRST_CAPACITY_POS = 1;
        public const int SECOND_ADDRESS_POS = 2;
        public const int SECOND_CAPACITY_POS = 3;

        private List<CSocket> sockets;
        private string NetworkAddress, ParentNetworkAddress; //prawdziwe adresy IP
        private string SubnetworkAddress, SubnetworkMask;    //adres tego subnetworka
        private List<SubnetworkAddress> containedSubnetworksAddresses;
        private List<Link> linkList;

        public ConnectionController()
        {
            NetworkAddress = Config.getProperty("NetworkAddress");
            ParentNetworkAddress = Config.getProperty("ParentNetworkAddress");

            SubnetworkAddress = Config.getProperty("SubnetworkAddress");
            SubnetworkMask = Config.getProperty("SubnetworkMask");

            containedSubnetworksAddresses = new List<SubnetworkAddress>();
            linkList = new List<Link>();
            LoadContainedSubnetworks();
        }

        public void LoadContainedSubnetworks()
        {
            string fileName = Config.getProperty("ContainedSubnetworks");
            string[] loadedFile = loadFile(fileName);
            string[] subnetworkParams = null;
            foreach (string str in loadedFile)
            {
                subnetworkParams = str.Split(PARAM_SEPARATOR);
                containedSubnetworksAddresses.Add(new SubnetworkAddress(subnetworkParams[ADDRESS_POSITION], subnetworkParams[MASK_POSITION]));
                Console.WriteLine(str);
            }
        }
        /*
        public void LoadLinkList()
        {
            string fileName = Config.getProperty("linkList");
            string[] loadedFile = loadFile(fileName);
            string[] linkParams = null;
            foreach (string str in loadedFile)
            {
                linkParams = str.Split(PARAM_SEPARATOR);
                linkList.Add(new Link(new SNPP(linkParams[FIRST_ADDRESS_POS], Int32.Parse(linkParams[FIRST_CAPACITY_POS])),
                                      new SNPP(linkParams[SECOND_ADDRESS_POS], Int32.Parse(linkParams[SECOND_CAPACITY_POS]))));
                Console.WriteLine(str);
            }
        }
        */
        private string[] loadFile(String fileName)
        {
            string[] fileLines = System.IO.File.ReadAllLines(fileName);
            return fileLines;
        }

        private List<SNPP> RouteTableQuery(string pathBegin, string pathEnd, int capacity)
        {
            List<SNPP> SNPPlist = new List<SNPP>();
            //wysyla do RC żądanie listy SNPP, a on odsyła bo jest grzeczny
            //TODO dodac wywolanie metody serwera, zeby wyslal RouteTableQuery

            return SNPPlist;
        }

        private Tuple<SNP, SNP> LinkConnectionRequest(SNPP connectionBegin, SNPP connectionEnd)
        {
            //Wysyła parę SNPP od LRM i czeka na odpowiedź
            Tuple<SNP, SNP> SNPpair = null;
            return SNPpair;
        }

        private Tuple<SNP, SNP> LinkConnectionRequest(SNP connectionBegin, SNPP connectionEnd)
        {
            //Wysyła parę SNPP od LRM i czeka na odpowiedź
            Tuple<SNP, SNP> SNPpair = null;
            return SNPpair;
        }

        private Tuple<SNP, SNP> LinkConnectionRequest(SNPP connectionBegin, SNP connectionEnd)
        {
            //Wysyła parę SNPP od LRM i czeka na odpowiedź
            Tuple<SNP, SNP> SNPpair = null;
            return SNPpair;
        }

        // % % % % % % % % % % % % % % % % % % % % % % % % % // 
        // %%%%%%%%%%%%%%%%% GŁOWNA METODA %%%%%%%%%%%%%%%%% //    
        // % % % % % % % % % % % % % % % % % % % % % % % % % //

        public bool ConnectionRequestFromNCC(string pathBegin, string pathEnd, int capacity)
        {
            string PathEndAddressFromDifferentDomain = null;

            List<SNPP> SNPPList = RouteTableQuery(pathBegin, pathEnd, capacity);

            //sprawdza, czy adres koncowy jest w tej samej domenie
            if (!IPAddressExtensions.IsInSameSubnet(IPAddress.Parse(pathEnd), IPAddress.Parse(SubnetworkAddress), IPAddress.Parse(SubnetworkMask)))
            {
                PathEndAddressFromDifferentDomain = pathEnd;
            }

            List<SNP> SNPList = new List<SNP>(); //TODO: nazwac to sensownie

            for (int index = 0; index < SNPPList.Count; index += 2)
            {
                SNPP SNPPpathBegin = SNPPList[index];
                SNPP SNPPpathEnd = SNPPList[index + 1];
                Tuple<SNP, SNP> SNPpair = LinkConnectionRequest(SNPPpathBegin, SNPPpathEnd);
                SNPList.Add(SNPpair.Item1);
                SNPList.Add(SNPpair.Item2);
            }

            for (int index = 0; index < SNPList.Count; index++)
            {
                SNP SNPpathBegin = SNPList[index];
                SNP SNPpathEnd = SNPList[index + 1];

                if (!IsOnLinkList(SNPpathBegin, SNPpathEnd))
                {
                    if (ConnectionRequestOut(SNPpathBegin, SNPpathEnd))
                    {
                        LogClass.Log("Subnetwork Connection set properly.");
                    }
                    else
                    {
                        LogClass.Log("Epic fail.");
                        return false;
                    }
                }
            }

            if (PathEndAddressFromDifferentDomain != null)
            {
                SNP lastSNPinThisDomain = SNPList.Last();
                if (PeerCoordinationOut(lastSNPinThisDomain, PathEndAddressFromDifferentDomain))
                {
                    LogClass.Log("PeerCoordination OK.");
                }
                else
                {
                    LogClass.Log("PeerCoordination FAIL.");
                };
            }

            return true;  //Jesli polaczenie zestawiono poprawnie
        }

        public bool ConnectionRequestFromCC(SNP SNPpathBegin, SNP SNPpathEnd)
        {
            List<SNPP> SNPPList = RouteTableQuery(SNPpathBegin.Address, SNPpathEnd.Address, SNPpathBegin.OccupiedCapacity);
            List<SNP> SNPList = new List<SNP>(); //TODO: nazwac to sensownie


            for (int index = 0; index < SNPPList.Count; index += 2)
            {
                SNPP SNPPpathBegin = SNPPList[index];
                SNPP SNPPpathEnd = SNPPList[index + 1];
                Tuple<SNP, SNP> SNPpair = null;

                if (index == 0)
                {
                    SNPpair = LinkConnectionRequest(SNPpathBegin, SNPPpathEnd);
                }
                else if (index == SNPPList.Count - 2)
                {
                    SNPpair = LinkConnectionRequest(SNPPpathBegin, SNPpathEnd);
                }
                else
                {
                    SNPpair = LinkConnectionRequest(SNPPpathBegin, SNPPpathEnd);
                }

                SNPList.Add(SNPpair.Item1);
                SNPList.Add(SNPpair.Item2);
            }

            for (int index = 0; index < SNPList.Count; index++)
            {
                SNP pathBegin = SNPList[index];
                SNP pathEnd = SNPList[index + 1];

                if (!IsOnLinkList(pathBegin, pathEnd))
                {
                    if (ConnectionRequestOut(pathBegin, pathEnd))
                    {
                        LogClass.Log("Subnetwork Connection set properly");
                    }
                    else
                    {
                        LogClass.Log("Epic fail.");
                        return false;
                    }
                }

            }
            return true;  //Jesli polaczenie zestawiono poprawnie
        }

        private bool IsOnLinkList(SNP SNPstart, SNP SNPend)
        {
            //sprawdza, czy ma taka pare na liscie 
            foreach (Link link in linkList)
            {
                if (link.FirstSNPP.Address == SNPstart.Address && link.SecondSNPP.Address == SNPend.Address)
                    return true;
            }
            return false;
        }

        private bool ConnectionRequestOut(SNP pathBegin, SNP pathEnd)
        {
            //wysyla do cc poziom niżej wiadomosc connection request
            IPAddress subnetworkAddress = null;
            IPAddress subnetworkAddressMask = null;

            foreach (SubnetworkAddress sub in containedSubnetworksAddresses)
            {
                if (IPAddressExtensions.IsInSameSubnet(sub.subnetAddress, IPAddress.Parse(pathBegin.Address), sub.subnetMask))
                {
                    subnetworkAddress = sub.subnetAddress;
                    subnetworkAddressMask = sub.subnetMask;
                }
            }
            SubnetworkAddress subnetAddress = new SubnetworkAddress(subnetworkAddress.ToString(), subnetworkAddressMask.ToString());
            SubnetworkServer.SendConnectionRequest(pathBegin, pathEnd, subnetAddress);
            return true;
        }

        public bool PeerCoordinationIn(SNP pathBegin, string pathEnd)
        {
            List<SNPP> SNPPList = RouteTableQuery(pathBegin.Address, pathEnd, pathBegin.OccupiedCapacity);
            List<SNP> SNPList = new List<SNP>(); //TODO: nazwac to sensownie

            for (int index = 0; index < SNPPList.Count; index += 2)
            {

                SNPP SNPPpathBegin = SNPPList[index];
                SNPP SNPPpathEnd = SNPPList[index + 1];
                Tuple<SNP, SNP> SNPpair = null;
                if (index == 0)
                {
                    SNPpair = LinkConnectionRequest(pathBegin, SNPPpathEnd);
                }
                else
                {
                    SNPpair = LinkConnectionRequest(SNPPpathBegin, SNPPpathEnd);
                }

                SNPList.Add(SNPpair.Item1);
                SNPList.Add(SNPpair.Item2);
            }

            for (int index = 0; index < SNPList.Count; index++)
            {
                SNP SNPpathBegin = SNPList[index];
                SNP SNPpathEnd = SNPList[index + 1];

                if (!IsOnLinkList(SNPpathBegin, SNPpathEnd))
                {
                    if (ConnectionRequestOut(SNPpathBegin, SNPpathEnd))
                    {
                        LogClass.Log("Subnetwork Connection set properly");
                    }
                    else
                    {
                        LogClass.Log("Epic fail");
                        return false;
                    }
                }
            }

            return true;  //Jesli polaczenie zestawiono poprawnie
        }

        private bool PeerCoordinationOut(SNP SNPpathBegin, string AddressPathEnd)
        {
            SubnetworkServer.SendPeerCoordination(SNPpathBegin, AddressPathEnd);
            return true;
        }

    }
}
