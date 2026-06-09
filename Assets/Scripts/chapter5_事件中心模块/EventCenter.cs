using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 用于 里式替换原则 装载 子类的父类
/// </summary>
public abstract class EventInfoBase{}

/// <summary>
/// 用来包裹 对应观察者 函数委托的 类
/// </summary>
public class EventInfo<T> : EventInfoBase
{
    //真正观察者对应的函数信息记录在其中
    public UnityAction<T> actions;
    
    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}

//实现重载 记录无参无返回值委托
public class EventInfo : EventInfoBase
{
    //真正观察者对应的函数信息记录在其中
    public UnityAction actions;
    
    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}

public class EventCenter : BaseManager<EventCenter>
{
    private EventCenter(){}
    
    //用于记录对应事件 关联的 对应的逻辑
    //object包含 .NET 的所有类型（int, string, List, 自定义类等）
    //Object只包含 Unity 引擎内的资源类型（GameObject, Material, Texture等，不包含 int, string 等普通 C# 类型）
    private Dictionary<string, EventInfoBase> eventDic = new Dictionary<string, EventInfoBase>();

    /// <summary>
    /// 触发事件 
    /// </summary>
    /// <param name="eventName">事件名字</param>
    public void EventTrigger<T>(string eventName,T info)
    {
        //有事件相应逻辑 才会触发
        if (eventDic.ContainsKey(eventName))
        {
            //父类转换为子类
            (eventDic[eventName] as EventInfo<T>).actions?.Invoke(info);
        }
    }
    
    //触发无参无返回值委托
    public void EventTrigger(string eventName)
    {
        //有事件相应逻辑 才会触发
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions?.Invoke();
        }
    }

    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="func"></param>
    public void AddEventListener<T>(string eventName, UnityAction<T> func)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).actions += func;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo<T>(func));
        }
    }   
    
    public void AddEventListener(string eventName, UnityAction func)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions += func;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo(func));
        }
    } 
    
    /// <summary>
    /// 移除事件监听
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="func"></param>
    public void RemoveEventListener<T>(string eventName, UnityAction<T> func)
    {
        if (eventDic.ContainsKey(eventName))
            (eventDic[eventName] as EventInfo<T>).actions -= func;
    }
    
    public void RemoveEventListener(string eventName, UnityAction func)
    {
        if (eventDic.ContainsKey(eventName))
            (eventDic[eventName] as EventInfo).actions -= func;
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
