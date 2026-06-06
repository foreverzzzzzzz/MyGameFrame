using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//需要将此脚本挂载在场景中的一个 GameObject 上
public class TestMgr2 : SingletonMono<TestMgr2>
{
    private int i;

    protected override void Awake()
    {
        base.Awake();
        i = 10;
    }

    public void Test()
    {
        print("TestMgr2" + i);
    }
}
