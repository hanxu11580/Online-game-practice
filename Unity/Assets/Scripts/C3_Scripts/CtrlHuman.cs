using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlHuman : BaseHuman
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

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cameraMain.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit);
            if (hit.collider.CompareTag("TerrainTag"))
            {
                MoveTo(hit.point);
            }
        }
    }
}
