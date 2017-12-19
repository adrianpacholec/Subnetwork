using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class LinkResourceManager
    {
        public const char PARAM_SEPARATOR = ' ';
        public const int ADDRESS_POSITION = 0;
        public const int CAPACITY_POSITION = 1;
        private List<LRM> LRMlist;
        private List<SNPP> myEdgeSNPPs;
        private List<String> containedSubnetworksAddresses;

        public LinkResourceManager()
        {
            String address = "1234";
            //dla kazdego SNPP z pliku utworz jego LRM
            LRMlist = new List<LRM>();
            myEdgeSNPPs = new List<SNPP>();
            containedSubnetworksAddresses = new List<string>();
            LRMlist.Add(new LRM(address));
            loadEdgeSNPPsFromFile();
            LoadContainedSubnetworks();

        }

        private void loadEdgeSNPPsFromFile()
        {
            string fileName = Config.getProperty("EdgeSNPPsFileName");
            string[] loadedFile = loadFile(fileName);
            string[] snppParams = null;
            foreach(string str in loadedFile)
            {
                snppParams = str.Split(PARAM_SEPARATOR);
                addEdgeSNPP(snppParams);
            }
        }

        private string[] loadFile(String fileName)
        {
            string[] fileLines = System.IO.File.ReadAllLines(fileName);
            return fileLines;
        }

        private void addEdgeSNPP(string[] snppParams)
        {
            String snppAddress = snppParams[ADDRESS_POSITION];
            int snppCapacity = Int32.Parse(snppParams[CAPACITY_POSITION]);
            SNPP edgeSNPP = new Subnetwork.SNPP(snppAddress, snppCapacity);
            myEdgeSNPPs.Add(edgeSNPP);
            Console.WriteLine(edgeSNPP.ToString());
        }

        public void LoadContainedSubnetworks()
        {
            string fileName = Config.getProperty("ContainedSubnetworks");
            string[] loadedFile = loadFile(fileName);
            string[] subnetworkParams = null;
            foreach(string str in loadedFile)
            {
                containedSubnetworksAddresses.Add(str);
                Console.WriteLine(str);
            }
        }


        class LRM
        {
            // posiada połączenie z SNPP
            private string SNPPaddress;
            //jakiś socket?

            public LRM(string SNPPaddress)
            {
                this.SNPPaddress = SNPPaddress;
            }

            public string GetAddress()
            {
                return SNPPaddress;
            }

            public void SetConnection()
            {
                //wyslij do swojego agencika żądanie
            }

        }


        private void SNPLinkConnectionRequest(SNPP SNPPpathBegin, SNPP SNPPpathEnd)
        {
            //Wysyła do funkcji LRM'a odpowiadającego podanym SNPP żądanie zaalokowania SNP
            foreach (LRM lrm in LRMlist)
            {
                if (lrm.GetAddress() == SNPPpathBegin.Address || lrm.GetAddress() == SNPPpathEnd.Address)
                {
                    lrm.SetConnection();
                }
            }
        }

        private bool SNPLinkConnectionDeallocation(string SNPpathBegin, string SNPpathEnd)
        {
            return true;
        }

        private void Topology(SNPP localTopologyUpdate)
        {
            //wysyła SNPP, które zostało uaktualnione do RC
        }


    }
}
