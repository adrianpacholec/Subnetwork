using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class LinkResourceManager
    {
        private List<LRM> LRMlist;
        public LinkResourceManager()
        {
            String address = "1234";
            //dla kazdego SNPP z pliku utworz jego LRM
            LRMlist.Add(new LRM(address));
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
