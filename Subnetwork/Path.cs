using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    public class Path
    {
        public List<Edge> edgesInPath { get; private set; }
        public int idfrom { get; }
        public int idto   {get;} 

        public Path()
        {
            idfrom = 0;
            idto = 0;
            edgesInPath = new List<Edge>();
        }
        public Path(int from, int to)
        {
            idfrom = from;
            idto = to;
            edgesInPath = new List<Edge>();
        }
        
    }
}
