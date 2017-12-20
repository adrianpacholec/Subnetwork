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
        public const int MASK_POSITION = 1;
        public const int LINK_NODE_A_POSITION = 0;
        public const int LINK_NODE_A_CAPACITY_POSITION = 1;
        public const int LINK_NODE_B_POSITION = 2;
        public const int LINK_NODE_B_CAPACITY_POSITION = 3;
        private List<LRM> LRMlist;
        private List<SNPP> myEdgeSNPPs;

        private List<Link> links;

        public LinkResourceManager()
        {
            String address = "1234";
            //dla kazdego SNPP z pliku utworz jego LRM
            LRMlist = new List<LRM>();
            myEdgeSNPPs = new List<SNPP>();
            links = new List<Link>();
            LoadLinks();

        }


        private string[] loadFile(String fileName)
        {
            string[] fileLines = System.IO.File.ReadAllLines(fileName);
            return fileLines;
        }


        public void LoadLinks()
        {
            string fileName = Config.getProperty("subnetworkLinks");
            string[] loadedFile = loadFile(fileName);
            string[] subnetworkParams = null;
            string firstNodeAddress = null;
            int firstNodeCapacity;
            string secondNodeAddress = null;
            int secondNodeCapacity;
            SNPP firstSNPP = null;
            SNPP secondSNPP = null;
            foreach (string str in loadedFile)
            {
                subnetworkParams = str.Split(PARAM_SEPARATOR);
                firstNodeAddress = subnetworkParams[LINK_NODE_A_POSITION];
                firstNodeCapacity = Int32.Parse(subnetworkParams[LINK_NODE_A_CAPACITY_POSITION]);
                secondNodeAddress = subnetworkParams[LINK_NODE_B_POSITION];
                secondNodeCapacity = Int32.Parse(subnetworkParams[LINK_NODE_B_CAPACITY_POSITION]);
                firstSNPP = new Subnetwork.SNPP(firstNodeAddress, firstNodeCapacity);
                secondSNPP = new SNPP(secondNodeAddress, secondNodeCapacity);
                links.Add(new Link(firstSNPP, secondSNPP));
                myEdgeSNPPs.Add(firstSNPP);
                myEdgeSNPPs.Add(secondSNPP);
                Console.WriteLine(str);
            }
        }

        class LRM
        {
            // posiada połączenie z SNPP
            private SNPP mySNPP;
            //jakiś socket?

            public LRM(string SNPPaddress, int capacity)
            {
                mySNPP = new Subnetwork.SNPP(SNPPaddress, capacity);
            }

            public string GetAddress()
            {
                return mySNPP.Address;
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
