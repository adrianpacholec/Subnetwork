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
        public const char PARAM_SEPARATOR = ' ';
        public const int ADDRESS_POSITION = 0;
        public const int MASK_POSITION = 1;

        private List<CSocket> sockets;
        private string NetworkAddress, ParentNetworkAddress;
        private List<SubnetworkAddress> containedSubnetworksAddresses;
        public ConnectionController()
        {
            NetworkAddress = Config.getProperty("NetworkAddress");
            ParentNetworkAddress = Config.getProperty("ParentNetworkAddress");
            containedSubnetworksAddresses = new List<SubnetworkAddress>();
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

        private string[] loadFile(String fileName)
        {
            string[] fileLines = System.IO.File.ReadAllLines(fileName);
            return fileLines;
        }

        private List<SNPP> RouteTableQuery(string pathBegin, string pathEnd, int capacity)
        {
            //wysyla do RC żądanie listy SNPP, a on odsyła bo jest grzeczny
            return new List<SNPP>();
        }

        private Tuple<SNP, SNP> LinkConnectionRequest(SNPP connectionBegin, SNPP connectionEnd)
        {
            //Wysyła parę SNPP od LRM i czeka na odpowiedź
            return null;
        }

        private Tuple<SNP, SNP> LinkConnectionRequest(SNP connectionBegin, SNPP connectionEnd)
        {
            //Wysyła parę SNPP od LRM i czeka na odpowiedź
            return null;
        }

        private Tuple<SNP, SNP> LinkConnectionRequest(SNPP connectionBegin, SNP connectionEnd)
        {
            //Wysyła parę SNPP od LRM i czeka na odpowiedź
            return null;
        }

        // % % % % % % % % % % % % % % % % % % % % % % % % % // 
        // %%%%%%%%%%%%%%%%% GŁOWNA METODA %%%%%%%%%%%%%%%%% //    
        // % % % % % % % % % % % % % % % % % % % % % % % % % //

        private bool ConnectionRequestFromNCC(string pathBegin, string pathEnd, int capacity)
        {
            List<SNPP> SNPPList = RouteTableQuery(pathBegin, pathEnd, capacity);
            List<SNP> SNPList = new List<SNP>(); //TODO: nazwac to sensownie


            for (int index = 0; index < SNPPList.Count; index +=2)
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
                        Console.WriteLine("Subnetwork Connection set properly");
                    }
                    else
                    {
                        Console.WriteLine("Epic fail");
                        return false;
                    }
                }

            }
            return true;  //Jesli polaczenie zestawiono poprawnie
        }

        private bool ConnectionRequestFromCC(SNP SNPpathBegin, SNP SNPpathEnd)
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
                        Console.WriteLine("Subnetwork Connection set properly");
                    }
                    else
                    {
                        Console.WriteLine("Epic fail");
                        return false;
                    }
                }

            }
            return true;  //Jesli polaczenie zestawiono poprawnie
        }

        private bool IsOnLinkList(SNP SNPstart, SNP SNPend)
        {

            //sprawdza, czy ma taka pare na liscie
            return true;
        }

        private bool ConnectionRequestOut(SNP pathBegin, SNP pathEnd)
        {
            //wysyla do cc poziom niżej wiadomosc connection request
            return true;
        }

        public bool PeerCoordinationIn(SNP SNPPpathBegin, SNPP SNPpathEnd)
        {
            return true;
        }

        private bool PeerCoordinationOut()
        {
            return true;
        }

    }
}
