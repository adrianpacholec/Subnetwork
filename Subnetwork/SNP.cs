using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    [Serializable]
    public class SNP
    { 
        public int Label { set; get; }
        public string Address { get; set; }
        public int OccupiedCapacity { get; set; }
        public string PathBegin { get; set; }
        public string PathEnd { get; set; }
        public bool Deleting { get; set; }

        public SNP(int label, string address, int capacity)
        {
            Label = label;
            Address = address;
            OccupiedCapacity = capacity;
            PathBegin = null;
            PathEnd = null;
            Deleting = false;
        }

        public SNP(int label, string address, int capacity, string pathBegin, string pathEnd)
        {
            Label = label;
            Address = address;
            OccupiedCapacity = capacity;
            PathBegin = pathBegin;
            PathEnd = pathEnd;
            Deleting = false;
        }
    }
}
