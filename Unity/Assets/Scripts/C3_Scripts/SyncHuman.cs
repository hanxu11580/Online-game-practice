using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncHuman : BaseHuman
{
    private Camera cameraMain;

    private new void Start()
    {
        base.Start();
        cameraMain = Camera.main;
    }

    new void Update()
    {
        base.Update();
    }
}