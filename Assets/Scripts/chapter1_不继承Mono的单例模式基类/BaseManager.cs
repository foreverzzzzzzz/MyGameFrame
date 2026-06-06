using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

//约束：T必须是类类型，且必须有公共无参构造函数
public class BaseManager<T> where T : class//,new()
{
    private static T instance;

    //判断单例模式对象 是否为null
    protected bool InstanceisNull => instance == null;

    //用于加锁的对象
    protected static readonly object lockObj = new object();

    //属性的方式
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        //instance = new T();
                        //利用反射得到无参私有的构造函数 来用于对象的实例化
                        Type type = typeof(T);
                        ConstructorInfo info = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                            null,
                            Type.EmptyTypes,
                            null);
                        if (info != null)
                            instance = info.Invoke(null) as T;
                        else
                            Debug.LogError("没有得到对应的无参构造函数");
    
                        //instance = Activator.CreateInstance(typeof(T), true) as T;
                    }
                }
            }
            return instance;
        }
    }


    //方法的方式
    // public static T GetInstance()
    // {
    //     if (instance == null)
    //         instance = new T();
    //     return instance;
    // }

}
