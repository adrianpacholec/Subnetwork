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

        private List<SNPP> RouteTableQuery(string pathBegin, string pathEnd)
        {
            //wysyla do RC żądanie listy SNPP, a on odsyła bo jest grzeczny
            return new List<SNPP>();
        }

        private Tuple<SNP, SNP> LinkConnectionRequest(SNPP connectionBegin, SNPP connectionEnd)
        {

            //Wysyła parę SNPP od LRM i czeka na odpowiedź
            return new Tuple<SNP, SNP>(new SNP(), new SNP());

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
                    Tuple<SNP, SNP> link = LinkConnectionRequest(SNPPpathBegin, SNPPpathEnd);
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

        private bool PeerCoordinationIn(SNPP SNPPpathBegin, SNP SNPpathEnd)
        {
            return true;
        }

        private bool PeerCoordinationOut()
        {
            return true;
        }

    }
}
