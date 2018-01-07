﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionController CC = new ConnectionController();
            LinkResourceManager LRM = new LinkResourceManager();
            RoutingController RC = new RoutingController(CC.ContainedSubnetworksAddresses, LRM.Links);
            SubnetworkServer.init(CC, RC, LRM);
            RC.testRouting();
            Console.ReadLine();
        }
    }
}
