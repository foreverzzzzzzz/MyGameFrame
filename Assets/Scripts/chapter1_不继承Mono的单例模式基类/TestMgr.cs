using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMgr : BaseManager<TestMgr>
{
    private TestMgr(){}
    
    public void Test()
    {
        Debug.Log("TestMgr");
    }
}
