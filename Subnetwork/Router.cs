using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class Router
    {
        private Network subnetwork;
        private List<SubnetworkAddress> subnetworks;
        private List<Link> links;
        private Dictionary<SubnetworkAddress, int> subnetworkToVertexId;
        private Dictionary<Link, int> linkAndEdgeIdCorrelation;
        private Dictionary<int, SubnetworkAddress> vertexIdToSubnetwork;


        public Router(List<SubnetworkAddress> subnetworks, List<Link> links)
        {
            this.subnetworks = subnetworks;
            this.links = links;
            TranslateSubnetworkData();
            subnetwork.fillAdjacencyMatrix(vertexIdToSubnetwork.Count);
            subnetwork.fillIncidenceMatrix(subnetwork.AdjacencyMatrix);
        }

        private void TranslateSubnetworkData()
        {
            subnetwork = new Network();
            Edge.clearEdgeCount();
            subnetworkToVertexId = new Dictionary<SubnetworkAddress, int>();
            linkAndEdgeIdCorrelation = new Dictionary<Link, int>();
            vertexIdToSubnetwork = new Dictionary<int, SubnetworkAddress>();
            translateSubnetworksToNodes();
            translateLinksToEdges();

        }

        public List<SNPP> route(IPAddress sourceAddress, IPAddress destinationAddress, int capacity)
        {
            TranslateSubnetworkData();
            refreshSubnetwork(capacity);
            List<SNPP> path = new List<SNPP>();
            List<Edge> edges = new List<Edge>();
            List<SNPP> translated = new List<SNPP>();
            SNPP sourceSNPP = findSNPPbyAddress(sourceAddress);
            SNPP destinationSNPP = findSNPPbyAddress(destinationAddress);
            subnetwork.algFloyda();
            SubnetworkAddress source = findSubnetworkWhereIsContained(sourceAddress);
            SubnetworkAddress destination = findSubnetworkWhereIsContained(destinationAddress);
            int sourceVertexId = subnetworkToVertexId[source];
            int destinationVertexId = subnetworkToVertexId[destination];
            try
            {
                if (!(sourceVertexId == destinationVertexId))
                {
                    edges = subnetwork.getPath(sourceVertexId, destinationVertexId);
                    translated = translateEdgesToSNPPs(edges);
                }
            }
            catch (FormatException e)
            {
                return new List<SNPP>();
            }           
            return translated;
        }

        private void refreshSubnetwork(int capacity)
        {
            subnetwork.removeLinksWithLowerCapacity(capacity);
            subnetwork.fillAdjacencyMatrix(vertexIdToSubnetwork.Count);
            subnetwork.fillIncidenceMatrix(subnetwork.AdjacencyMatrix);
        }

        private List<SNPP> translateEdgesToSNPPs(List<Edge> edges)
        {
            int edgeId;
            SNPP first;
            SNPP second;
            Link converted;
            List<SNPP> translated = new List<SNPP>();
            foreach (Edge edge in edges)
            {
                edgeId = edge.getid();
                converted = getLinkByEdgeId(edgeId);
                first = converted.FirstSNPP;
                second = converted.SecondSNPP;
                translated.Add(first);
                translated.Add(second);
            }
            return translated;
        }

        private Link getLinkByEdgeId(int edgeId)
        {
            foreach (KeyValuePair<Link, int> entry in linkAndEdgeIdCorrelation)
            {
                if (entry.Value == edgeId)
                    return entry.Key;
            }
            return null;
        }

        public void translateSubnetworksToNodes()
        {

            int counter = 1;
            foreach (SubnetworkAddress subnetAddress in subnetworks)
            {
                subnetworkToVertexId.Add(subnetAddress, counter);
                vertexIdToSubnetwork.Add(counter, subnetAddress);
                Vertex vertex = new Vertex(counter);
                subnetwork.addVertex(vertex);
                counter++;
            }
        }

        public void translateLinksToEdges()
        {
            List<Vertex> vertices = subnetwork.VertexList;
            int counter = 1;
            String firstSNPPAddress = null;
            String secondSNPPAddress = null;
            SubnetworkAddress firstSubnetworkAddress = null;
            SubnetworkAddress secondSubnetworkAddress = null;
            foreach (Link link in links)
            {
                linkAndEdgeIdCorrelation.Add(link, counter);
                firstSNPPAddress = link.FirstSNPP.Address;
                secondSNPPAddress = link.SecondSNPP.Address;
                firstSubnetworkAddress = findSubnetworkWhereIsContained(IPAddress.Parse(firstSNPPAddress));
                secondSubnetworkAddress = findSubnetworkWhereIsContained(IPAddress.Parse(secondSNPPAddress));
                int firstVertexId = subnetworkToVertexId[firstSubnetworkAddress];
                int secondVertexId = subnetworkToVertexId[secondSubnetworkAddress];
                int capacity = link.FirstSNPP.Capacity;
                subnetwork.addEdge(firstVertexId, secondVertexId, capacity, link.ignore);
                counter++;
            }
        }
        public SNPP findSNPPbyAddress(IPAddress address)
        {
            SNPP first = null;
            SNPP second = null;
            SNPP found = null;
            String toFind = address.ToString();
            foreach (Link link in links)
            {
                first = link.FirstSNPP;
                second = link.SecondSNPP;
                if (first.Address.Equals(toFind))
                    found = first;
                else if (second.Address.Equals(toFind))
                    found = second;
            }
            return found;
        }

        private SubnetworkAddress findSubnetworkWhereIsContained(IPAddress isContained)
        {
            IPAddress subnetAddress = null;
            IPAddress subnetMask = null;
            SubnetworkAddress subnetworkAddressWhereIsContained = null;
            foreach (SubnetworkAddress address in subnetworks)
            {
                subnetAddress = address.subnetAddress;
                subnetMask = address.subnetMask;
                if (IPAddressExtensions.IsInSameSubnet(subnetAddress, isContained, subnetMask))
                    subnetworkAddressWhereIsContained = address;
            }
            return subnetworkAddressWhereIsContained;
        }
    }
}
