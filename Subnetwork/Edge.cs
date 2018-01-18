using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Subnetwork
{
    public class Edge
    {
        private int id;
        static int countEdge=0;
        public int weight { get; }
        public int capacity { get; set; }
        public Vertex startVertex { get; private set; }
        public Vertex endVertex { get; private set; }
        public bool isDirected { get; }
        public bool ignore;

        public Edge()
        {
            id = 0;
            weight = Int32.MaxValue;
        }

        public static void clearEdgeCount()
        {
            countEdge = 0;
        }

        public Edge(int idstart, int idend, int weight,int capacity, Vertex v1, Vertex v2, bool directing, bool ignore)
        {
            
            id=++countEdge;
            startVertex = v1;
            endVertex = v2;
            this.weight = weight;
            isDirected = directing;
            this.capacity = capacity;
            this.ignore = ignore;

        }
        public int getid()
        {
            return id;
        }
   
    }
}
