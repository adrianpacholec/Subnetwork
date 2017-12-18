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

        private (SNPP begin, SNPP end) LinkConnectionRequest(SNPP connectionBegin, SNPP connectionEnd)
        {

            //Wysyła parę SNPP od LRM i czeka na odpowiedź
            return (new SNP(), new SNP());
       
        }

        //#1
        private bool ConnectionRequestIn(string pathBegin, string pathEnd)
        {
            List<SNPP> SNPPList = RouteTableQuery(pathBegin, pathEnd);

            for (int index = 1; index < SNPPList.Count; index++)
            {

                var link = LinkConnectionRequest(SNPPList[index - 1], SNPPList[index]);
                if ((link.begin.GetType() == typeof(SNPP)) && (link.end.GetType() == typeof(SNPP)))
                {
                    //trzeba nizej zestawic tego linka
                    if (ConnectionRequestOut(link.begin, link.end))
                    {
                        Console.WriteLine("Subnetwork Connection set properly");
                        // polaczenie nizej zestawione poprawnie
                    }
                    else
                    {
                        Console.WriteLine("Epic fail");
                        // gromki fail
                    }

                }
                else if ((link.begin.GetType() == typeof(SNP)) && (link.end.GetType() == typeof(SNP)))
                {
                    //zestawiono pięknie
                }
            }

            return true;  //Jesli polaczenie zestawiono poprawnie
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
