using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Subnetwork
{
    class Program
    {
        public static int SUBNET_ADDRESS_POSITION = 0;
        public static int SUBNET_MASK_POSITION = 1;
        public static int MY_SNPP_ADDRESS = 2;
        public static int MY_SNPP_CAPACITY = 3;
        public static int EXT_SNPP_ADDRESS = 4;
        public static int EXT_SNPP_CAPACITY = 5;

        static void Main(string[] args)
        {
            ConnectionController CC = new ConnectionController();
            LinkResourceManager LRM = new LinkResourceManager();
            RoutingController RC = new RoutingController(CC.ContainedSubnetworksAddresses, LRM.Links);
            SubnetworkServer.init(CC, RC, LRM);
            loadEdgeSNPPs(CC, LRM);
            Console.ReadLine();
        }

        private static void loadEdgeSNPPs(ConnectionController cc, LinkResourceManager lrm)
        {
            string[] loaded = LoadFile(Config.getProperty("portsToDomainsFile"));
            string[] splitedParameters;
            foreach(string str in loaded)
            {
                if (str[0] != '#')
                {
                    splitedParameters = str.Split(' ');
                    lrm.addEdgeSNPP(new Subnetwork.SNPP(splitedParameters[MY_SNPP_ADDRESS], Int32.Parse(splitedParameters[MY_SNPP_CAPACITY])));
                    cc.addKeyToDictionary(new SubnetworkAddress(splitedParameters[SUBNET_ADDRESS_POSITION], splitedParameters[SUBNET_MASK_POSITION]));
                }
            }
            foreach(string str in loaded)
            {
                if (str[0] != '#')
                {
                    splitedParameters = str.Split(' ');
                    cc.addValueToDictionary(new SubnetworkAddress(splitedParameters[SUBNET_ADDRESS_POSITION], splitedParameters[SUBNET_MASK_POSITION]), new Tuple<IPAddress, IPAddress>(IPAddress.Parse(splitedParameters[MY_SNPP_ADDRESS]), IPAddress.Parse(splitedParameters[EXT_SNPP_ADDRESS])));
                }
            }
        }

        private static string[] LoadFile(String fileName)
        {
            string[] fileLines = System.IO.File.ReadAllLines(fileName);
            return fileLines;
        }
    }
}
