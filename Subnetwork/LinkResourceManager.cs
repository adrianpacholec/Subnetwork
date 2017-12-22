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
        private int rememberedLabel;

        public const int MASK_POSITION = 1;
        public const int LINK_NODE_A_POSITION = 0;
        public const int LINK_NODE_A_CAPACITY_POSITION = 1;
        public const int LINK_NODE_B_POSITION = 2;
        public const int LINK_NODE_B_CAPACITY_POSITION = 3;
        private List<Link> links;


        public LinkResourceManager()
        {
            myEdgeSNPPs = new List<SNPP>();
            rememberedLabel = GimmeNewLabel();
            links = new List<Link>();
            LoadLinks();

        }

        /*
        private void LoadEdgeSNPPsFromFile()
        {
            string fileName = Config.getProperty("EdgeSNPPsFileName");
            string[] loadedFile = LoadFile(fileName);
            string[] snppParams = null;
            foreach (string str in loadedFile)
            {
                snppParams = str.Split(PARAM_SEPARATOR);
                AddEdgeSNPP(snppParams);
            }
        }
        */

        private string[] LoadFile(String fileName)
        {
            string[] fileLines = System.IO.File.ReadAllLines(fileName);
            return fileLines;
        }

        private void AddEdgeSNPP(string[] snppParams)
        {
            String snppAddress = snppParams[ADDRESS_POSITION];
            int snppCapacity = Int32.Parse(snppParams[CAPACITY_POSITION]);
            SNPP edgeSNPP = new Subnetwork.SNPP(snppAddress, snppCapacity);
            SNPsbySNPPaddress.Add(edgeSNPP.Address, new List<SNP>());
            myEdgeSNPPs.Add(edgeSNPP);
            Console.WriteLine(edgeSNPP.ToString());
        }


        public void LoadLinks()
        {
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
                firstSNPP = new Subnetwork.SNPP(firstNodeAddress, firstNodeCapacity);
                secondSNPP = new SNPP(secondNodeAddress, secondNodeCapacity);
                links.Add(new Link(firstSNPP, secondSNPP));
                myEdgeSNPPs.Add(firstSNPP);
                myEdgeSNPPs.Add(secondSNPP);
                Console.WriteLine(str);
            }
        }


        // % % % % % % % % % % % % % % % % % % % % % % % % % // 
        // %%%%%%%%%%%%%%%%% GŁOWNA METODA %%%%%%%%%%%%%%%%% //    
        // % % % % % % % % % % % % % % % % % % % % % % % % % //


        private Tuple<SNP, SNP> SNPLinkConnectionRequest(Object pathBegin, Object pathEnd, int capacity)
        {
            Tuple<SNP, SNP> SNPpair = null;
            //LRM dostaje od CC dwa SNPP do stworzenia SNP
            if (pathBegin.GetType() == typeof(SNPP) && pathEnd.GetType() == typeof(SNPP))
            {

                SNPP SNPPpathBegin = (SNPP)pathBegin;
                SNPP SNPPpathEnd = (SNPP)pathEnd;

                SNPpair = SNPPandSNPP(SNPPpathBegin, SNPPpathEnd, capacity);

            }

            //LRM dostaje od CC SNP i SNPP, uzywa otrzymane SNP i tworzy nowe SNP dla SNPP
            else if (pathBegin.GetType() == typeof(SNP) && pathEnd.GetType() == typeof(SNPP))
            {
                SNP SNPpathBegin = (SNP)pathBegin;
                SNPP SNPPpathEnd = (SNPP)pathEnd;

                SNPpair = SNPandSNPP(SNPpathBegin, SNPPpathEnd, capacity);

            }
            //LRM dostaje od CC SNPP i SNP, uzywa otrzymane SNP i tworzy nowe SNP dla SNPP
            else if (pathBegin.GetType() == typeof(SNPP) && pathEnd.GetType() == typeof(SNPP))
            {
                SNPP SNPPpathBegin = (SNPP)pathBegin;
                SNP SNPpathEnd = (SNP)pathEnd;

                SNPpair = SNPPandSNP(SNPPpathBegin, SNPpathEnd, capacity);
            }
            return SNPpair;
        }
 
        private Tuple<SNP, SNP> SNPPandSNPP(SNPP pathBegin, SNPP pathEnd, int capacity)
        {
            List<SNP> existingSNPs = new List<SNP>();
            Tuple<SNP, SNP> SNPpair;
            SNP SNPpathBegin, SNPpathEnd;

            //sprawdz, czy label z kieszonki nie wystapil w SNPs - jesli tak, wygeneruj inny label
            existingSNPs = SNPsbySNPPaddress[pathBegin.Address];
            while (existingSNPs.Find(x => x.Label == rememberedLabel) != null) rememberedLabel = GimmeNewLabel();            
            //tworzenie SNP poczatkowego SNPP
            SNPpathBegin = new SNP(rememberedLabel, pathBegin.Address, capacity); //uses remembered label
            SNPsbySNPPaddress[pathBegin.Address].Add(SNPpathBegin);
            Topology(SNPpathBegin);

            //generuje nowy label
            int potentiallyNewLabel = GimmeNewLabel();
            //sprawdz, czy wygenerowany label nie wystapil w SNPs - jesli tak, wygeneruj inny label
            existingSNPs = SNPsbySNPPaddress[pathEnd.Address];
            while (existingSNPs.Find(x => x.Label == potentiallyNewLabel) != null) potentiallyNewLabel = GimmeNewLabel();
            //pobiera liste SNP koncowego SNPP
            existingSNPs = SNPsbySNPPaddress[pathEnd.Address];
            //tworzenie SNP koncowego SNPP
            SNPpathEnd = new SNP(potentiallyNewLabel, pathEnd.Address, capacity); //uses generated label
            SNPsbySNPPaddress[pathEnd.Address].Add(SNPpathEnd);
            Topology(SNPpathEnd);

            SNPpair = new Tuple<SNP, SNP>(SNPpathBegin, SNPpathEnd);
            return SNPpair;
        }

        private Tuple<SNP, SNP> SNPandSNPP(SNP pathBegin, SNPP pathEnd, int capacity)
        {
            List<SNP> existingSNPs = new List<SNP>();
            Tuple<SNP, SNP> SNPpair;
            SNP SNPpathEnd;

            //dodaje otrzymane SNP do odpowiadajacego mu SNPP
            SNPsbySNPPaddress[pathBegin.Address].Add(pathBegin);
            Topology(pathBegin);

            //generuje nowy label
            int potentiallyNewLabel = GimmeNewLabel();
            //sprawdz, czy wygenerowany label nie wystapil w SNPs - jesli tak, wygeneruj inny label
            existingSNPs = SNPsbySNPPaddress[pathEnd.Address];
            while (existingSNPs.Find(x => x.Label == potentiallyNewLabel) != null) potentiallyNewLabel = GimmeNewLabel();
            //pobiera liste SNP koncowego SNPP
            existingSNPs = SNPsbySNPPaddress[pathEnd.Address];
            //tworzenie SNP koncowego SNPP
            SNPpathEnd = new SNP(potentiallyNewLabel, pathEnd.Address, capacity); //uses generated label
            SNPsbySNPPaddress[pathEnd.Address].Add(SNPpathEnd);
            Topology(SNPpathEnd);
         
            SNPpair = new Tuple<SNP, SNP>(pathBegin, SNPpathEnd);
            return SNPpair;
        }

        private Tuple<SNP, SNP> SNPPandSNP(SNPP pathBegin, SNP pathEnd, int capacity)
        {
            List<SNP> existingSNPs = new List<SNP>();
            Tuple<SNP, SNP> SNPpair;
            SNP SNPpathBegin;

            //sprawdz, czy label z kieszonki nie wystapil w SNPs - jesli tak, wygeneruj inny label
            existingSNPs = SNPsbySNPPaddress[pathBegin.Address];
            while (existingSNPs.Find(x => x.Label == rememberedLabel) != null) rememberedLabel = GimmeNewLabel();
            //tworzenie SNP poczatkowego SNPP
            SNPpathBegin = new SNP(rememberedLabel, pathBegin.Address, capacity); //uses remembered label
            SNPsbySNPPaddress[pathBegin.Address].Add(SNPpathBegin);
            Topology(SNPpathBegin);

            //dodaje otrzymane SNP do odpowiadajacego mu SNPP
            SNPsbySNPPaddress[pathEnd.Address].Add(pathEnd);
            Topology(pathEnd);

            SNPpair = new Tuple<SNP, SNP>(SNPpathBegin, pathEnd);
            return SNPpair;
        }


        private int GimmeNewLabel()
        {
            Random random = new Random();
            return random.Next(100);
        }

        private bool SNPLinkConnectionDeallocation(string SNPpathBegin, string SNPpathEnd)
        {
            return true;
        }

        private void Topology(SNP localTopologyUpdate)
        {
            //wysyła SNP, które zostało uaktualnione do RC
            //Wywołanie metody serwera, która jeszcze nie jest zrobiona
            SubnetworkServer.SendTopologyUpdateToRC(false, localTopologyUpdate);
        }


    }
}
