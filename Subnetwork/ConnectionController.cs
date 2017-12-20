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

        // % % % % % % % % % % % % % % % % % % % % % % % % % // 
        // %%%%%%%%%%%%%%%%% GŁOWNA METODA %%%%%%%%%%%%%%%%% //    
        // % % % % % % % % % % % % % % % % % % % % % % % % % //

        private bool ConnectionRequestFromNCC(string pathBegin, string pathEnd, int capacity)
        {
            List<SNPP> SNPPList = RouteTableQuery(pathBegin, pathEnd, capacity);
            List<SNP> SNPList; //TODO: nazwac to sensownie


            for (int index = 0; index < SNPPList.Count; index + 2)
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
            List<SNPP> SNPPList = RouteTableQuery(pathBegin, pathEnd, capacity);
            List<SNP> SNPList; //TODO: nazwac to sensownie


            for (int index = 0; index < SNPPList.Count; index + 2)
            {
                SNPP SNPPpathBegin = SNPPList[index];
                SNPP SNPPpathEnd = SNPPList[index + 1];
                
                if (index == 0)
                {
                    Tuple<SNP, SNP> SNPpair = LinkConnectionRequest(SNPpathBegin, SNPPpathEnd);
                }
                else if (index == SNPPList.Count - 2)
                {
                    Tuple<SNP, SNP> SNPpair = LinkConnectionRequest(SNPPpathBegin, SNPpathEnd);
                }
                else
                {
                    Tuple<SNP, SNP> SNPpair = LinkConnectionRequest(SNPPpathBegin, SNPPpathEnd);
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
