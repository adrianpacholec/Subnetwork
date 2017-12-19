using System;
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
            RoutingController RC = new RoutingController();
            LinkResourceManager LRM = new LinkResourceManager();
            SubnetworkServer.init(CC, RC, LRM);
            Console.ReadLine();
        }
    }
}
