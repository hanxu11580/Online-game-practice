using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
namespace C7_Server
{
    class ClientState
    {
        public Socket socket;
        public ByteArray readBuff = new ByteArray();


    }
}
