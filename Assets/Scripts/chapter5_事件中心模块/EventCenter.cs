using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventCenter : BaseManager<EventCenter>
{
    private EventCenter(){}
    
    //用于记录对应事件 关联的 对应的逻辑
    //object包含 .NET 的所有类型（int, string, List, 自定义类等）
    //Object只包含 Unity 引擎内的资源类型（GameObject, Material, Texture等，不包含 int, string 等普通 C# 类型）
    private Dictionary<string, UnityAction<object>> eventDic = new Dictionary<string, UnityAction<object>>();

    /// <summary>
    /// 触发事件 
    /// </summary>
    /// <param name="eventName">事件名字</param>
    public void EventTrigger(string eventName,object info = null)
    {
        //有事件相应逻辑 才会触发
        if (eventDic.ContainsKey(eventName))
        {
            eventDic[eventName]?.Invoke(info);
        }
    }

    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="func"></param>
    public void AddEventListener(string eventName, UnityAction<object> func)
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
    public void RemoveEventListener(string eventName, UnityAction<object> func)
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
