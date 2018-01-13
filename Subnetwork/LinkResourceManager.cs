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
        private List<SNPP> myEdgeSNPPs;

        private Dictionary<string, List<SNP>> SNPsbySNPPaddress;

        public const int MASK_POSITION = 1;
        public const int LINK_NODE_A_POSITION = 0;
        public const int LINK_NODE_A_CAPACITY_POSITION = 1;
        public const int LINK_NODE_B_POSITION = 2;
        public const int LINK_NODE_B_CAPACITY_POSITION = 3;
        public List<Link> Links { get; set; }


        public LinkResourceManager()
        {
            myEdgeSNPPs = new List<SNPP>();
            Links = new List<Link>();
            LoadLinks();
            SNPsbySNPPaddress = new Dictionary<string, List<SNP>>();

        }

        public void LoadLinks()
        {
            CustomSocket.LogClass.Log("loading links in subnetwork:");
            string fileName = Config.getProperty("subnetworkLinks");
            string[] loadedFile = LoadFile(fileName);
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
                firstSNPP = new SNPP(firstNodeAddress, firstNodeCapacity);
                secondSNPP = new SNPP(secondNodeAddress, secondNodeCapacity);
                Links.Add(new Link(firstSNPP, secondSNPP));
                myEdgeSNPPs.Add(firstSNPP);
                myEdgeSNPPs.Add(secondSNPP);
                Console.WriteLine(str);
            }
        }

        public void AddEdgeSNPP(SNPP snpp)
        {
            myEdgeSNPPs.Add(snpp);
        }

        private string[] LoadFile(String fileName)
        {
            string[] fileLines = System.IO.File.ReadAllLines(fileName);
            return fileLines;
        }

        private void AddEdgeSNPP(string[] snppParams)
        {
            String snppAddress = snppParams[ADDRESS_POSITION];
            int snppCapacity = Int32.Parse(snppParams[CAPACITY_POSITION]);
            SNPP edgeSNPP = new SNPP(snppAddress, snppCapacity);
            SNPsbySNPPaddress.Add(edgeSNPP.Address, new List<SNP>());
            myEdgeSNPPs.Add(edgeSNPP);
            Console.WriteLine(edgeSNPP.ToString());
        }

        // % % % % % % % % % % % % % % % % % % % % % % % % % // 
        // %%%%%%%%%%%%%%%%% GŁOWNA METODA %%%%%%%%%%%%%%%%% //    
        // % % % % % % % % % % % % % % % % % % % % % % % % % //


        public Tuple<SNP, SNP> SNPLinkConnectionRequest(Object pathBegin, Object pathEnd, int capacity)
        {
            Tuple<SNP, SNP> SNPpair = null;
            //LRM dostaje od CC dwa SNPP do stworzenia SNP lub dwa SNP do usuniecia
            if (pathBegin.GetType() == typeof(SNPP) && pathEnd.GetType() == typeof(SNPP))
            {
                SNPP SNPPpathBegin = (SNPP)pathBegin;
                SNPP SNPPpathEnd = (SNPP)pathEnd;
                SNPpair = AllocateLink(SNPPpathBegin, SNPPpathEnd, capacity);
            }

            //jesli SNPPki są równe to znaczy ze są fejkowe i trzeba znaleźć SNPP o tym adresie
            else if (pathBegin == pathEnd)
            {
                SNPP fakeSNPP = (SNPP)pathBegin;
                string address = fakeSNPP.Address;
                SNPP realSNPP = myEdgeSNPPs.Find(i => i.Address == address);
                if (realSNPP.Capacity > capacity)
                {
                    realSNPP.Capacity -= capacity;
                    //generuje nowy label
                    int potentiallyNewLabel = GimmeNewLabel();

                    //sprawdz, czy wygenerowany label nie wystapil w SNPs - jesli tak, wygeneruj inny label
                    if (!SNPsbySNPPaddress.ContainsKey(realSNPP.Address))
                    {
                        SNPsbySNPPaddress.Add(realSNPP.Address, new List<SNP>());
                    }

                    //tworzenie SNP poczatkowego SNPP
                    SNP createdSNP = new SNP(potentiallyNewLabel, fakeSNPP.Address, capacity); //uses remembered label
                    SNPsbySNPPaddress[fakeSNPP.Address].Add(createdSNP);
                    Topology(createdSNP);

                    SNPpair = new Tuple<SNP, SNP>(createdSNP, createdSNP);
                    return SNPpair;

                }

            }
            else if (pathBegin.GetType() == typeof(SNP) && pathEnd.GetType() == typeof(SNP))
            {
                SNP SNPpathBegin = (SNP)pathBegin;
                SNP SNPpathEnd = (SNP)pathEnd;
                RemoveLink(SNPpathBegin, SNPpathEnd);
            }
            return SNPpair;

        }

        private void RemoveLink(SNP SNPpathBegin, SNP SNPpathEnd)
        {
            List<SNP> existingSNPs = new List<SNP>();

            SNP BeginToBeDeleted = SNPsbySNPPaddress[SNPpathBegin.Address].Find(x => x.Address == SNPpathBegin.Address);
            SNPsbySNPPaddress[SNPpathBegin.Address].Remove(BeginToBeDeleted);
            Topology(SNPpathBegin);

            SNP EndToBeDeleted = SNPsbySNPPaddress[SNPpathEnd.Address].Find(x => x.Address == SNPpathEnd.Address);
            SNPsbySNPPaddress[SNPpathEnd.Address].Remove(EndToBeDeleted);
            Topology(SNPpathEnd);
        }

        private Tuple<SNP, SNP> AllocateLink(SNPP pathBegin, SNPP pathEnd, int capacity)
        {
            List<SNP> existingSNPs = new List<SNP>();
            Tuple<SNP, SNP> SNPpair;
            SNP SNPpathBegin, SNPpathEnd;

            if (!SNPsbySNPPaddress.ContainsKey(pathBegin.Address))
            {
                SNPsbySNPPaddress.Add(pathBegin.Address, new List<SNP>());
                if (pathBegin != pathEnd)
                    SNPsbySNPPaddress.Add(pathEnd.Address, new List<SNP>());
            }

            //generuje nowy label
            int potentiallyNewLabel = GimmeNewLabel();

            //sprawdz, czy wygenerowany label nie wystapil w SNPs - jesli tak, wygeneruj inny label
            existingSNPs = SNPsbySNPPaddress[pathBegin.Address];
            while (existingSNPs.Find(x => x.Label == potentiallyNewLabel) != null) potentiallyNewLabel = GimmeNewLabel();

            //tworzenie SNP poczatkowego SNPP
            SNPpathBegin = new SNP(potentiallyNewLabel, pathBegin.Address, capacity); //uses remembered label
            SNPsbySNPPaddress[pathBegin.Address].Add(SNPpathBegin);
            Topology(SNPpathBegin);

            if (pathBegin != pathEnd)
            {
                //tworzenie SNP koncowego SNPP
                SNPpathEnd = new SNP(potentiallyNewLabel, pathEnd.Address, capacity); //uses generated label
                SNPsbySNPPaddress[pathEnd.Address].Add(SNPpathEnd);
                Topology(SNPpathEnd);

                SNPpair = new Tuple<SNP, SNP>(SNPpathBegin, SNPpathEnd);
            }
            else
            {
                SNPpair = new Tuple<SNP, SNP>(SNPpathBegin, SNPpathBegin);
            }
            return SNPpair;
        }


        private int GimmeNewLabel()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            return random.Next(100);
        }

        private void Topology(SNP localTopologyUpdate)
        {
            //wysyła SNP, które zostało uaktualnione do RC
            //Wywołanie metody serwera, która jeszcze nie jest zrobiona
            SubnetworkServer.SendTopologyUpdateToRC(false, localTopologyUpdate);
        }


    }
}
