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
        private List<String> containedSubnetworksAddresses;
        private Dictionary<SNPP, List<SNP>> SNPsbySNPP;
        private int rememberedLabel;

        public LinkResourceManager()
        {
            myEdgeSNPPs = new List<SNPP>();
            containedSubnetworksAddresses = new List<string>();
            loadEdgeSNPPsFromFile();
            LoadContainedSubnetworks();
            rememberedLabel = GimmeNewLabel();

        }

        private void loadEdgeSNPPsFromFile()
        {
            string fileName = Config.getProperty("EdgeSNPPsFileName");
            string[] loadedFile = loadFile(fileName);
            string[] snppParams = null;
            foreach (string str in loadedFile)
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
            SNPsbySNPP.Add(edgeSNPP, new List<SNP>());
            myEdgeSNPPs.Add(edgeSNPP);
            Console.WriteLine(edgeSNPP.ToString());
        }

        public void LoadContainedSubnetworks()
        {
            string fileName = Config.getProperty("ContainedSubnetworks");
            string[] loadedFile = loadFile(fileName);
            string[] subnetworkParams = null;
            foreach (string str in loadedFile)
            {
                containedSubnetworksAddresses.Add(str);
                Console.WriteLine(str);
            }
        }


        // % % % % % % % % % % % % % % % % % % % % % % % % % // 
        // %%%%%%%%%%%%%%%%% GŁOWNA METODA %%%%%%%%%%%%%%%%% //    
        // % % % % % % % % % % % % % % % % % % % % % % % % % //

        private Tuple<SNP, SNP> SNPLinkConnectionRequest(Object pathBegin, Object pathEnd, int capacity)
        {
            Tuple<SNP, SNP> SNPpair;
            //LRM dostaje od CC dwa SNPP do stworzenia SNP
            if (pathBegin.GetType == instanceOf(SNPP) && pathEnd.GetType == instanceOf(SNPP))
            {

                SNPP SNPPpathBegin = (SNPP)pathBegin;
                SNPP SNPPpathEnd = (SNPP)pathEnd;

                SNPpair = SNPPandSNPP(SNPPpathBegin, SNPPpathEnd, capacity);

            }

            //LRM dostaje od CC SNP i SNPP, uzywa otrzymane SNP i tworzy nowe SNP dla SNPP
            else if (pathBegin.GetType == instanceOf(SNP) && pathEnd.GetType == instanceOf(SNPP))
            {
                SNP SNPpathBegin = (SNP)pathBegin;
                SNPP SNPPpathEnd = (SNPP)pathEnd;

                SNPpair = SNPandSNPP(SNPpathBegin, SNPPpathEnd, capacity);

            }
            //LRM dostaje od CC SNPP i SNP, uzywa otrzymane SNP i tworzy nowe SNP dla SNPP
            else if (pathBegin.GetType == instanceOf(SNPP) && pathEnd.GetType == instanceOf(SNPP))
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
            SNP SNPpathBegin;
            SNP SNPpathEnd;

            //petla po wszystkich elementach slownika                                                                                                     
            foreach (SNPP snpp in SNPsbySNPP)
            {
                //jezeli znajdzie wpis o adresie poczatkowego to
                if (snpp.Address == SNPPpathBegin.Address)
                {
                    //tworzy SNP poczatkowe i dodaje do listy w slowniku
                    SNPpathBegin = SNP(rememberedLabel, SNPPpathBegin.Address, capacity); //uses remembered label
                    SNPpair.Item1 = SNPpathBegin;
                    SNPsbySNPP[snpp.Address].Add(SNPpathBegin);
                }
                //jezeli znajdzie wpis o adresie koncowego to
                else if (snpp.Address == SNPPpathEnd.Address)
                {
                    //pobiera liste SNP tego SNPP
                    existingSNPs = SNPsbySNPP[snpp.Address];
                    //generuje nowy label
                    int potentiallyNewLabel = GimmeNewLabel();
                    //sprawdza, czy wygenerowany label juz nie wystapil 
                    while (true)
                    {
                        foreach (SNP snp in existingSNPs)
                        {
                            if (snp.Label == potentiallyNewLabel)
                            {
                                //jezeli wystapil, to generuje nowy i sprawdza jeszcze raz
                                potentiallyNewLabel = GimmeNewLabel();
                                continue;
                            }
                            //jezeli nie wystapil to tworzy SNP koncowe i dodaje je do listy w slowniku
                            SNPpathEnd = SNP(potentiallyNewLabel, SNPPpathEnd.Address, capacity); //uses generated label
                            SNPpair.Item2 = SNPpathEnd;
                            SNPsbySNPP[snpp.Address].Add(SNPpathBegin);
                            continue;
                        }
                    }
                }
            }
        }

        private Tuple<SNP, SNP> SNPandSNPP(SNP SNPpathBegin, SNPP pathEnd, int capacity)
        {
            List<SNP> existingSNPs = new List<SNP>();
            Tuple<SNP, SNP> SNPpair;
            SNP SNPpathEnd;

            //petla po wszystkich elementach slownika                                                                                                     
            foreach (SNPP snpp in SNPsbySNPP)
            {
                //jezeli znajdzie wpis o adresie poczatkowego to
                if (snpp.Address == SNPPpathBegin.Address)
                {
                    SNPpair.Item1 = SNPpathBegin;
                    //dodaje otrzymane SNP do listy w slowniku
                    SNPsbySNPP[snpp.Address].Add(SNPpathBegin);
                }
                //jezeli znajdzie wpis o adresie koncowego to
                else if (snpp.Address == SNPPpathEnd.Address)
                {
                    //pobiera liste SNP tego SNPP
                    existingSNPs = SNPsbySNPP[snpp.Address];
                    //generuje nowy label
                    int potentiallyNewLabel = GimmeNewLabel();
                    //sprawdza, czy wygenerowany label juz nie wystapil 
                    while (true)
                    {
                        foreach (SNP snp in existingSNPs)
                        {
                            if (snp.Label == potentiallyNewLabel)
                            {
                                //jezeli wystapil, to generuje nowy i sprawdza jeszcze raz
                                potentiallyNewLabel = GimmeNewLabel();
                                continue;
                            }
                            //jezeli nie wystapil to tworzy SNP koncowe i dodaje je do listy w slowniku
                            SNPpathEnd = SNP(potentiallyNewLabel, SNPPpathEnd.Address, capacity); //uses generated label
                            SNPpair.Item2 = SNPpathEnd;
                            SNPsbySNPP[snpp.Address].Add(SNPpathBegin);
                            continue;
                        }
                    }
                }
            }
        }

        private Tuple<SNP, SNP> SNPPandSNP(SNPP pathBegin, SNP SNPpathEnd, int capacity)
        {
            List<SNP> existingSNPs = new List<SNP>();
            Tuple<SNP, SNP> SNPpair;
            SNP SNPpathBegin;

            //petla po wszystkich elementach slownika                                                                                                     
            foreach (SNPP snpp in SNPsbySNPP)
            {
                //jezeli znajdzie wpis o adresie poczatkowego to
                if (snpp.Address == SNPPpathBegin.Address)
                {
                    //tworzy SNP poczatkowe i dodaje do listy w slowniku
                    SNPpathBegin = SNP(rememberedLabel, SNPPpathBegin.Address, capacity); //uses remembered label
                    SNPpair.Item1 = SNPpathBegin;
                    SNPsbySNPP[snpp.Address].Add(SNPpathBegin);
                }
                //jezeli znajdzie wpis o adresie koncowego to
                else if (snpp.Address == SNPPpathEnd.Address)
                {
                    SNPpair.Item2 = SNPpathEnd;
                    //dodaje otrzymane SNP do listy w slowniku
                    SNPsbySNPP[snpp.Address].Add(SNPpathEnd);
                }
            }
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

        private void Topology(SNPP localTopologyUpdate)
        {
            //wysyła SNPP, które zostało uaktualnione do RC
        }


    }
}
