using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonoMgr : SingletonAutoMono<MonoMgr>
{
    private event UnityAction updateEvent;
    private event UnityAction fixedUpdateEvent;
    private event UnityAction LateUpdateEvent;

    //添加帧更新函数
    public void AddUpdateEvent(UnityAction updateFun)
    {
        updateEvent += updateFun;
    }
    public void AddFixedUpdateEvent(UnityAction fixedUpdateFun)
    {
        fixedUpdateEvent += fixedUpdateFun;
    }
    public void AddLateUpdateEvent(UnityAction LateUpdateFun)
    {
        LateUpdateEvent += LateUpdateFun;
    }
    //移除帧更新函数
    public void RemoveUpdateEvent(UnityAction updateFun)
    {
        updateEvent -= updateFun;
    }
    public void RemoveFixedUpdateEvent(UnityAction fixedUpdateFun)
    {
        fixedUpdateEvent -= fixedUpdateFun;
    }
    public void RemoveLateUpdateEvent(UnityAction LateUpdateFun)
    {
        LateUpdateEvent -= LateUpdateFun;
    }
    
    private void Update()
    {
        updateEvent?.Invoke();
    }
    private void FixedUpdate()
    {
        fixedUpdateEvent?.Invoke();
    }

    private void LateUpdate()
    {
        LateUpdateEvent?.Invoke();
    }
}
