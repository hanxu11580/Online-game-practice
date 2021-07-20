using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
    private void OnEnter(string msg)
    {
        string[] splits = msg.Split(',');
        string desc = splits[0];
        if (myHuman.desc == desc) return; //自己
        float x = float.Parse(splits[1]);
        float y = float.Parse(splits[2]);
        float z = float.Parse(splits[3]);
        float eularY = float.Parse(splits[4]);
        CreateOtherPerson(desc, new Vector3(x, y, z), eularY);
    }

    private void OnMove(string msg)
    {
        Debug.Log("移动" + msg);
    }

    private void OnLeave(string msg)
    {
        Debug.Log("离开");
    }

    void Update()
    {
        NetManager.Update();
    }

    private void CreateOtherPerson(string desc,Vector3 pos, float eularY)
    {
        GameObject go = Instantiate(syncPrefab);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(new Vector3(0, eularY, 0)));
        BaseHuman human = go.AddComponent<SyncHuman>();
        human.desc = desc;
        otherHumans.Add(desc, human);
        Debug.Log("Create Succ");
    }
}
