using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestArrayByte : MonoBehaviour
{
    private void Start()
    {
        ByteArray ba = new ByteArray(8);
        ba.Debug();
        byte[] wb = new byte[] { 1, 2, 3, 4, 5 };
        ba.Write(wb, 0, wb.Length);
        ba.Debug();

        byte[] rb = new byte[4];
        ba.Read(rb, 0, 2);
        Debug.Log(BitConverter.ToString(rb, 0, rb.Length));
        ba.Debug();
        Debug.Log(ba.ToString());

        //扩容测试
        wb = new byte[] { 6, 7, 8, 9, 10, 11 };
        ba.Write(wb, 0, wb.Length);
        ba.Debug();
        Debug.Log(ba.ToString());
    }
}
