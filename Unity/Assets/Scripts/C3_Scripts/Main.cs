using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace C3
{
    public class Main : MonoBehaviour
    {
        public GameObject syncPrefab;
        public BaseHuman myHuman;
        public Dictionary<string, BaseHuman> otherHumans = new Dictionary<string, BaseHuman>();

        void Start()
        {
            NetManager.AddListener("Enter", OnEnter);
            NetManager.AddListener("Move", OnMove);
            NetManager.AddListener("Leave", OnLeave);
            NetManager.AddListener("List", OnList);
            NetManager.Connect("127.0.0.1", 8888);

            GameObject go = Instantiate(syncPrefab);
            go.transform.position = Vector3.up * 0.45f;
            myHuman = go.AddComponent<CtrlHuman>();
            myHuman.desc = NetManager.GetDesc();

            Vector3 pos = go.transform.position;
            float eularY = go.transform.eulerAngles.y;
            string sendStr = "Enter|";
            sendStr += myHuman.desc + ",";
            sendStr += pos.x + ",";
            sendStr += pos.y + ",";
            sendStr += pos.z + ",";
            sendStr += eularY;
            NetManager.Send(sendStr);
            Invoke("SendListMsg", 0.1f);
        }

        void SendListMsg()
        {
            NetManager.Send("List|");
        }

        private void OnList(string msg)
        {
            string[] splits = msg.Split(',');
            int personInfoCount = (splits.Length - 1) / 6;
            for (int i = 0, count = personInfoCount; i < count; i++)
            {
                string desc = splits[i * 6];
                if (myHuman.desc == desc) continue;
                float x = float.Parse(splits[i * 6 + 1]);
                float y = float.Parse(splits[i * 6 + 2]);
                float z = float.Parse(splits[i * 6 + 3]);
                float eulY = float.Parse(splits[i * 6 + 4]);
                int hp = int.Parse(splits[i * 6 + 5]);
                CreateOtherPerson(desc, new Vector3(x, y, z), eulY);
            }

        }

        private void OnEnter(string msg)
        {
            string[] splits = msg.Split(',');
            string desc = splits[0];
            if (myHuman.desc == desc) return;
            float x = float.Parse(splits[1]);
            float y = float.Parse(splits[2]);
            float z = float.Parse(splits[3]);
            float eularY = float.Parse(splits[4]);
            CreateOtherPerson(desc, new Vector3(x, y, z), eularY);
        }

        private void OnMove(string msg)
        {
            string[] split = msg.Split(',');
            string desc = split[0];
            float x = float.Parse(split[1]);
            float y = float.Parse(split[2]);
            float z = float.Parse(split[3]);
            if (otherHumans.TryGetValue(desc, out BaseHuman otherHuman))
            {
                otherHuman.MoveTo(new Vector3(x, y, z));
            }
        }

        private void OnLeave(string msg)
        {
            string[] split = msg.Split(',');
            string desc = split[0];
            if (otherHumans.TryGetValue(desc, out BaseHuman otherHuman))
            {
                Destroy(otherHuman.gameObject);
                otherHumans.Remove(desc);
            }

        }

        void Update()
        {
            NetManager.Update();
        }

        private void CreateOtherPerson(string desc, Vector3 pos, float eularY)
        {
            GameObject go = Instantiate(syncPrefab);
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(new Vector3(0, eularY, 0)));
            BaseHuman human = go.AddComponent<SyncHuman>();
            human.desc = desc;
            otherHumans.Add(desc, human);
        }
    }
}