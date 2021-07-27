using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace C3
{

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
}