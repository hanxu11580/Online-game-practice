using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
namespace C7_Server
{
    public class ClientState
    {
        public Socket socket;

        public ByteArray readBuff = new ByteArray();

        public long lastPingTime = 0;

        public Player player;

        public static ClientState Create(Socket socket)
        {
            ClientState clientState = new ClientState();
            clientState.socket = socket;
            return clientState;
        }

    }
}
