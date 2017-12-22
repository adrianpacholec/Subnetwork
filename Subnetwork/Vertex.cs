using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Subnetwork
{
    public class Vertex
    {
        public static int vertexcount = 0;
        public int id { get; }
        
        public Vertex()
        {
            id = 0;
        }
  
        public Vertex(int id)
        {
            this.id = id;
            vertexcount++;
        }

      

      

    }
}


























