using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BeginPanel : BasePanel
{
    private void Start()
    {
        UIMgr.AddCustomEventListener(GetControl<Button>("btn"),EventTriggerType.PointerEnter, (data) =>
        {
            print("鼠标进入");
        });
    }

    public override void HideMe()
    {
        
    }

    public override void ShowMe()
    {
        
    }

    public void TestFunc()
    {
        print("想要执行的逻辑");
    }
}
