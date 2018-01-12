using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    public class Network
    {
        public List<Vertex> VertexList { get; }
        public List<Edge> EdgeList { get; }
        public List<Path> PathList { get; }
        public int[,] AdjacencyMatrix { get; private set; }
        public int[,] IncidenceMatrix { get; private set; }
        public List<Edge> MST { get; private set; }

        public delegate void calledAlgorithm(Network network, int[,] matrix);
        public event calledAlgorithm endAlgPrima;


       public Network()
        {
            VertexList = new List<Vertex>();
            EdgeList = new List<Edge>();
            PathList = new List<Path>();
            MST = new List<Edge>();
        }

      
        public void addVertex(Vertex vertex)
        {
            VertexList.Add(vertex);
        }

        public void addEdge(int firstVertexId, int secondVertexId, int capacity)
        {
            Vertex first = getVertex(firstVertexId, VertexList);
            Vertex second = getVertex(secondVertexId, VertexList);
            int weight = 1000000 / capacity;
            Edge created = new Edge(firstVertexId, secondVertexId, weight,capacity, first, second, false);
            EdgeList.Add(created);
        }

        public void algPrima()
        {
            int h1;
            int nbOfEndVertexOfTheLightestEdge=0;
            int startvert = 0;
            bool flag = false; 
            int[,] mstMatrix = new int[AdjacencyMatrix.GetLength(1),AdjacencyMatrix.GetLength(1)];
            bool[] IsVisited = new bool[VertexList.Count];
            IsVisited[0] = true;

            for (int a = 0; a < VertexList.Count-1; a++)
            {
                for (int b = 0; b < mstMatrix.GetLength(0); b++)
                {
                    if (IsVisited[b])
                    {
                        h1 = getNbEndVertexOfLightestEdge(b, IsVisited);
                        if (h1 == -1) continue;
                        if (!flag || AdjacencyMatrix[b, h1] < AdjacencyMatrix[startvert, nbOfEndVertexOfTheLightestEdge])
                        {
                            nbOfEndVertexOfTheLightestEdge = h1;
                            startvert = b;
                            flag = true;
                        }
                    }

                }
                mstMatrix[startvert, nbOfEndVertexOfTheLightestEdge] = 1;
                IsVisited[nbOfEndVertexOfTheLightestEdge] = true;
                flag = false;
            }
            endAlgPrima(this, mstMatrix);        
        }

        internal void removeLinksWithLowerCapacity(int capacity)
        {
            for (int i = EdgeList.Count - 1; i >= 0; i--)
                if (EdgeList.ElementAt(i).capacity < capacity)
                    EdgeList.RemoveAt(i);
        }

        public void algFloyda()
        {
        

            int range = AdjacencyMatrix.GetLength(0);
            for (int a = 0; a < range; a++)
                AdjacencyMatrix[a, a] = 0;
            for (int k = 0; k < range; k++)
                for (int i = 0; i < range; i++)
                    for (int j = 0; j < range; j++)
                    {
                        if (AdjacencyMatrix[i, j] > AdjacencyMatrix[i, k] + AdjacencyMatrix[k, j])
                            {
                                IncidenceMatrix[i, j] = IncidenceMatrix[k, j];
                                AdjacencyMatrix[i, j] = AdjacencyMatrix[i, k] + AdjacencyMatrix[k, j];
                            }
                    }           
         }

        #region metody_operujace_na_macierzach 

        public void fillAdjacencyMatrix(int size)
        {
            AdjacencyMatrix = new int[size, size];
            for (int a = 0; a < size; a++)
                for (int b = 0; b < size; b++)
                    AdjacencyMatrix[a, b] = int.MaxValue / 10;
            for (int a = 0; a < EdgeList.Count; a++)
            {
                AdjacencyMatrix[EdgeList[a].startVertex.id - 1, EdgeList[a].endVertex.id - 1] = EdgeList[a].weight;
                if (!EdgeList[a].isDirected)
                    AdjacencyMatrix[EdgeList[a].endVertex.id - 1, EdgeList[a].startVertex.id - 1] = EdgeList[a].weight;
            }
        }

        public void fillIncidenceMatrix(int[,] adjMatrix)
        {
            IncidenceMatrix = new int[adjMatrix.GetLength(0), adjMatrix.GetLength(1)];
            for (int a = 0; a < adjMatrix.GetLength(1); a++)
                for (int b = 0; b < adjMatrix.GetLength(1); b++)
                    if (adjMatrix[a, b] < Int32.MaxValue / 10)
                        IncidenceMatrix[a, b] = a;
                    else IncidenceMatrix[a, b] = -1;


        }

        public string getIndexOfPreviousVertex(string path, int indexfirstvertex, int indexsecondvertex) //rekurencyjna metoda zwracajaca sciezke o poczatku indexfirst i koncu indexsecond
        {
             if (IncidenceMatrix[indexfirstvertex, indexsecondvertex] == indexfirstvertex)
            {
                path += IncidenceMatrix[indexfirstvertex, indexsecondvertex];
                return path;
            }
            else return IncidenceMatrix[indexfirstvertex, indexsecondvertex] +" "+(getIndexOfPreviousVertex(path, indexfirstvertex, IncidenceMatrix[indexfirstvertex, indexsecondvertex]));
       }

        public void createStringBasedPath(string path)  //dodaje sciezke do listy scieżek na podstawie stringa z ciągiem wierzchołków
        {
            if (!String.IsNullOrEmpty(path))
            {
                int b = 0;
                string[] nbsVertices= path.Split(' ');
                PathList.Add(new Path(Int32.Parse(nbsVertices[0])+1, Int32.Parse(nbsVertices[nbsVertices.GetLength(0)-1])+1));
                for (int a=0; a<nbsVertices.GetLength(0)-1; a++)
                {
                    b = a + 1;
                    PathList[PathList.Count - 1].edgesInPath.Add(getEdge(Int32.Parse(nbsVertices[a])+1, Int32.Parse(nbsVertices[b])+1, EdgeList));

                }
            }
        }

        public string getPathFromIncidenceMatrix(string path,int indexfirstvertex, int indexsecondvertex) //uzupełnienie metody getIndexOfPreviousVertex ktora dodaje ostatni wierzchołek
        {
            try
            {
                return indexsecondvertex + " " + getIndexOfPreviousVertex(path, indexfirstvertex, indexsecondvertex);
            }catch(IndexOutOfRangeException e)
            {
                return "";
            }
        }

        public int getNbEndVertexOfLightestEdge(int vertexindex, bool[]isvisited) //zwraca numer wierzchołka do którego koszt dojścia jest najmniejszy i jest jeszcze nieodwiedzony (jeśli nie ma takiego zwraca -1)
        {
            int weight = Int32.MaxValue;
            int nb=-1;
            for(int a=0; a<AdjacencyMatrix.GetLength(1); a++)
                if (weight > AdjacencyMatrix[vertexindex, a] && isvisited[a] != true)
                {
                    weight = AdjacencyMatrix[vertexindex, a];
                    nb = a;
                }
            return nb;
            
        }
        #endregion

        public Edge getEdge(int id) //zwraca krawędź po id z listy wierzchołków
        {
            Edge helpedge = new Edge();
            for (int a = 0; a < EdgeList.Count; a++)
            {
                if (EdgeList[a].getid() == id)
                    return EdgeList[a];
            }
            return helpedge;
        }

        public Vertex getVertex(int id, List<Vertex> verlist) //zwraca wierzchołek po id
        {
            Vertex ver = new Vertex();
            for (int a = 0; a < verlist.Count; a++)
                if (verlist[a].id == id)
                    ver = verlist[a];
                    
            return ver;
        }
        public List<Edge> getPath(int v1id, int v2id)
        {
            addPath(v1id-1, v2id-1); //bo przekazujemy index
            return getEdgeslist(v1id, v2id);          
        }

        public List<Edge> getEdgeslist(int v1id, int v2id) //zwraca liste krawędzi pomiędzy wierzcholkiem v1index i v2index (LISTA MUSI BYC W PATHLIST -TRZEBA JĄ TAM NAJPIERW DODAC INNA METODA) - DO ZMIANY. DZIALA DLA KRAWEDZI NIESKIEROWANYCH
        {
            List<Edge> list = new List<Edge>();
            for (int a = 0; a < PathList.Count; a++)
            {

                if ((v1id == PathList[a].idfrom && v2id == PathList[a].idto)|| (v1id == PathList[a].idto && v2id == PathList[a].idfrom)) //to nie zadziala dla krawedzi skierowanych!!!
                {
                    list = PathList[a].edgesInPath;
                    break;
                }
            }
            return list;
        }

        public Edge getEdge(int idfirstvert, int idsecondvert, List<Edge> edges) //dziala dla skierowanych!!!
        {
            Edge e = new Edge();
            for (int a = 0; a < edges.Count; a++)
            {
                e = edges[a];
                if ((idfirstvert == edges[a].startVertex.id && idsecondvert == edges[a].endVertex.id) || (idfirstvert == edges[a].endVertex.id && idsecondvert == edges[a].startVertex.id))
                    break;
            }
            return e;
        }

        public void addPath(int idStartV, int idEndV) //dodaje sciezke do listy sciezek o zadanych końcach
        {
            string path = null;
            path = getPathFromIncidenceMatrix(path, idStartV, idEndV);
            createStringBasedPath(path);
        }
       


    }

}
