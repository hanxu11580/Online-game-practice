using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace C6
{

    public class C7_Test : MonoBehaviour
    {
        public InputField userInput;
        public InputField pwInput;
        public InputField textInput;


        private void Start()
        {
            NetManager.AddEventListener(NetEvent.ConnectSucc, OnConnectSucc);
            NetManager.AddEventListener(NetEvent.ConnectFail, OnConnectFail);
            NetManager.AddEventListener(NetEvent.Close, OnConnectClose);

            NetManager.AddMsgListener("MsgRegister", OnMsgRegister);
            NetManager.AddMsgListener("MsgLogin", OnMsgLogin);
            NetManager.AddMsgListener("MsgKick", OnMsgKick);
            NetManager.AddMsgListener("MsgGetText", OnMsgGetText);
            NetManager.AddMsgListener("MsgSaveText", OnMsgSaveText);
        }

        private void OnDestroy()
        {
			NetManager.Close();
        }

        #region Click

        public void OnClickConnect()
		{
			NetManager.Connect("127.0.0.1", 8888);
		}


		//发送保存协议
		public void OnSaveClick()
		{
			MsgSaveText msg = new MsgSaveText();
			msg.text = textInput.text;
			NetManager.Send(msg);
		}

		//发送注册协议
		public void OnRegisterClick()
		{
			MsgRegister msg = new MsgRegister();
			msg.acc = userInput.text;
			msg.pw = pwInput.text;
			NetManager.Send(msg);
		}

		//发送登陆协议
		public void OnLoginClick()
		{
			MsgLogin msg = new MsgLogin();
			msg.acc = userInput.text;
			msg.pw = pwInput.text;
			NetManager.Send(msg);
		}

		public void OnCloseClick()
        {
			NetManager.Close();
        }

		#endregion End_Click

		private void OnConnectSucc(string err)
        {
            Debug.Log("OnConnectSucc");
        }

        private void OnConnectFail(string err)
        {
            Debug.Log("OnConnectFail" + err);
        }

        private void OnConnectClose(string err)
        {
            Debug.Log("OnConnectClose" + err);
        }

        private void Update()
        {
            NetManager.Update();
        }

		//被踢下线
		void OnMsgKick(MsgBase msgBase)
		{
			Debug.Log("被踢下线");
		}

		//收到注册协议
		public void OnMsgRegister(MsgBase msgBase)
		{
			MsgRegister msg = (MsgRegister)msgBase;
			if (msg.result == 0)
			{
				Debug.Log("注册成功");
			}
			else
			{
				Debug.Log("注册失败");
			}
		}

		//收到登陆协议
		public void OnMsgLogin(MsgBase msgBase)
		{
			MsgLogin msg = (MsgLogin)msgBase;
			if (msg.result == 0)
			{
				Debug.Log("登陆成功");
				//请求记事本文本
				MsgGetText msgGetText = new MsgGetText();
				NetManager.Send(msgGetText);
			}
			else
			{
				Debug.Log("登陆失败");
			}
		}

		//收到记事本文本协议
		public void OnMsgGetText(MsgBase msgBase)
		{
			MsgGetText msg = (MsgGetText)msgBase;
			textInput.text = msg.text;
		}

		//收到保存协议
		void OnMsgSaveText(MsgBase msgBase)
		{
			MsgSaveText msg = (MsgSaveText)msgBase;
			if (msg.result == 0)
			{
				Debug.Log("保存成功");
			}
			else
			{
				Debug.Log("保存失败");
			}
		}
	}
}
