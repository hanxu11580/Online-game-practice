using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Linq;

namespace C7_Server
{
    class NetManager
    {
        public static Socket listenfd;

        public static Dictionary<Socket, ClientState> clientDict = new Dictionary<Socket, ClientState>();

        static List<Socket> checkSockets = new List<Socket>();




    }
}
