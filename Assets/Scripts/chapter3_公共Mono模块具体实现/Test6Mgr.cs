using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//BaseManager没有继承MonoBehaviour，不能使用直接使用生命周期函数
public class Test6Mgr : BaseManager<Test6Mgr>
{
    private Coroutine coroutine;
    
    public void doUpdate()
    {
        MonoMgr.Instance.AddUpdateEvent(MyUpdate);
    }

    public void stopUpdate()
    {
        MonoMgr.Instance.RemoveUpdateEvent(MyUpdate);
    }

    public void MyUpdate()
    {
        Debug.Log("一直打印");
    }

    public void startCoroutine()
    {
        coroutine = MonoMgr.Instance.StartCoroutine(Test());
    }
    
    public void stopCoroutine()
    {
        MonoMgr.Instance.StopCoroutine(coroutine);
    }
    
    private IEnumerator Test()
    {
        yield return new WaitForSeconds(3);
        Debug.Log("Test  Test");
    }
}
