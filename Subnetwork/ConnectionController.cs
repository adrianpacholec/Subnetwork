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

        private List<SNPP> RouteTableQuery(string pathBegin, string pathEnd)
        {
            //wysyla do RC żądanie listy SNPP, a on odsyła bo jest grzeczny
            return new List<SNPP>();
        }

        private Tuple<SNPP, SNPP> LinkConnectionRequest(SNPP connectionBegin, SNPP connectionEnd)
        {

            //Wysyła parę SNPP od LRM i czeka na odpowiedź

            return null;

        }

        //#1
        private bool ConnectionRequestIn(string pathBegin, string pathEnd)
        {
            List<SNPP> SNPPList = RouteTableQuery(pathBegin, pathEnd);

            for (int index = 1; index < SNPPList.Count; index++)
            {
                SNPP SNPPpathBegin = SNPPList[index - 1];
                SNPP SNPPpathEnd = SNPPList[index];

                if (CheckLinkSetPossible(SNPPpathBegin, SNPPpathEnd))
                {
                    //Tuple<SNP, SNP> link = LinkConnectionRequest(SNPPpathBegin, SNPPpathEnd);
                }
                else
                {
                    if (ConnectionRequestOut(SNPPpathBegin, SNPPpathEnd))
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

        private bool CheckLinkSetPossible(SNPP SNPPstart, SNPP SNPPend)
        {

            //sprawdza, czy ma taka pare na liscie
            return true;
        }

        private bool ConnectionRequestOut(SNPP pathBegin, SNPP pathEnd)
        {
            //wysyla do cc poziom niżej wiadomosc connection request
            //skąd wie, do którego cc ma wyslac?

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
