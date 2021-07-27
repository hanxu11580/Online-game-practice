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

        public void OnClickConnect()
        {
            NetManager.Connect("127.0.0.1", 8888);
        }

        public void OnClickClose()
        {
            NetManager.Close();
        }



    }
}