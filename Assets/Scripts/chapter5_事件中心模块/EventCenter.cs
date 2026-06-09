using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventCenter : BaseManager<EventCenter>
{
    private EventCenter(){}
    
    //用于记录对应事件 关联的 对应的逻辑
    private Dictionary<string, UnityAction> eventDic = new Dictionary<string, UnityAction>();

    /// <summary>
    /// 触发事件 
    /// </summary>
    /// <param name="eventName">事件名字</param>
    public void EventTrigger(string eventName)
    {
        //有事件相应逻辑 才会触发
        if (eventDic.ContainsKey(eventName))
        {
            eventDic[eventName]?.Invoke();
        }
    }

    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="func"></param>
    public void AddEventListener(string eventName, UnityAction func)
    {
        if (eventDic.ContainsKey(eventName))
            eventDic[eventName] += func;
        else
        {
            eventDic.Add(eventName, null);
            eventDic[eventName] += func;
        }
    }   
    /// <summary>
    /// 移除事件监听
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="func"></param>
    public void RemoveEventListener(string eventName, UnityAction func)
    {
        if (eventDic.ContainsKey(eventName))
            eventDic[eventName] -= func;
    }

    //清除所有事件监听
    public void Clear()
    {
        eventDic.Clear();
    }

    //清除某一个事件监听
    public void Clear(string eventName)
    {
        if (eventDic.ContainsKey(eventName))
        {
            eventDic.Remove(eventName);
        }
    }
}
