using CustomSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Subnetwork
{
    class SubnetworkServer
    {
        private static ConnectionController connectionController;
        private static RoutingController routingController;
        private static LinkResourceManager linkResourceManager;
        private static CSocket csocket;

        public static void init(ConnectionController cc, RoutingController rc, LinkResourceManager lrm)
        {
            cc = cc;
            rc = rc;
            lrm = lrm;
            initListeningCustomSocket();
        }

        public void initListeningCustomSocket()
        {
            String parentAddress = Config.getProperty("ParentSubnetworkAddress");
            IPAddress parentSubnetworkAddress = IPAddress.Parse(parentAddress);
            int port = Config.getIntegerProperty("port");
            csocket = new CSocket(parentSubnetworkAddress, port);
        }

        public void ListenForConnections()
        {

        }

        private Thread initListenThread(ConnectionController cc, RoutingController rc, LinkResourceManager lrm)
        {
            var t = new Thread(() => RealStart(cc,rc,lrm));
            t.IsBackground = true;
            t.Start();
            return t;
        }

        private void RealStart(ConnectionController cc, RoutingController rc, LinkResourceManager lrm)
        {
            Socket connected=csocket.Accept();
       
        }

        private void waitForInputFromSocketInAnotherThread(Socket connected)
        {
            var t = new Thread(() => waitForInput());
            t.Start();
        }

        private void waitForInput()
        {
            Object receivedObject = csocket.ReceiveObject();
            if(receivedObject.GetType() == typeof(List<SNPP>))
            {
                insertSNPPSToRC((List<SNPP>)receivedObject);
            }
               

        }

        private void insertSNPPSToRC(List<SNPP> receivedList)
        {
            for (int i = 0; i < receivedList.Count; i++)
                rc.addSNPP(receivedList.ElementAt(i));
        }
        
    }
}
