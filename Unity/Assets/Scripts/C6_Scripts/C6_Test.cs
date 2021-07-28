using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace C6
{
    public class C6_Test : MonoBehaviour
    {
        private void Start()
        {
            NetManager.AddEventListener(NetEvent.ConnectSucc, OnConnectSucc);
            NetManager.AddEventListener(NetEvent.ConnectFail, OnConnectFail);
            NetManager.AddEventListener(NetEvent.Close, OnConnectClose);
            NetManager.AddMsgListener("MsgMove", OnMsgMove);
        }

        private void OnMsgMove(MsgBase msg)
        {
            MsgMove msgMove = msg as MsgMove;
            Debug.Log(msgMove.x);
            Debug.Log(msgMove.y);
            Debug.Log(msgMove.z);
        }

        private void OnConnectSucc(string err)
        {
            Debug.Log("OnConnectSucc");
        }

        private void OnConnectFail(string err)
        {
            Debug.Log("OnConnectFail"+ err);
        }

        private void OnConnectClose(string err)
        {
            Debug.Log("OnConnectClose" + err);
        }

        public void OnClickSend()
        {
            MsgMove msgMove = new MsgMove()
            {
                x = 1, y = 2, z = 3
            };
            NetManager.Send(msgMove);
        }


        public void OnClickConnect()
        {
            NetManager.Connect("127.0.0.1", 8888);
        }

        public void OnClickClose()
        {
            NetManager.Close();
        }

        private void Update()
        {
            NetManager.Update();
        }


    }
}