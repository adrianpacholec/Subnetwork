﻿using System;
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
         

        public Router(List<SubnetworkAddress> subnetworks, List<Link>links)
        {
            this.subnetworks = subnetworks;
            this.links = links;
            subnetwork = new Network();
            subnetworkToVertexId = new Dictionary<SubnetworkAddress, int>();
            linkAndEdgeIdCorrelation = new Dictionary<Link, int>();
            TranslateSubnetworkData();
        }

        private void TranslateSubnetworkData()
        {
            translateSubnetworksToNodes();
            translateLinksToEdges();
           
        }

        public List<SNPP> route(IPAddress sourceAddress, IPAddress destinationAddress)
        {
            List<SNPP> path = new List<SNPP>();
            SNPP sourceSNPP = findSNPPbyAddress(sourceAddress);
            SNPP destinationSNPP = findSNPPbyAddress(destinationAddress);
            subnetwork.algFloyda();
            SubnetworkAddress source = findSubnetworkWhereIsContained(sourceAddress);
            SubnetworkAddress destination = findSubnetworkWhereIsContained(destinationAddress);
            int sourceVertexId = subnetworkToVertexId[source];
            int destinationVertexId = subnetworkToVertexId[destination];
            List<Edge> edges=subnetwork.getPath(sourceVertexId, destinationVertexId);
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
                subnetwork.addEdge(firstVertexId, secondVertexId, capacity);
                counter++;
            }
        }
        public SNPP findSNPPbyAddress(IPAddress address)
        {
            SNPP first = null;
            SNPP second = null;
            SNPP found = null;
            String toFind = address.ToString();
            foreach(Link link in links)
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
