using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    
    public static T Instance
    {
        get
        {
            return instance;
        }
    }

    //Awake可以被子类重写
    protected virtual void Awake()
    {
        //把当前这个脚本对象，强制转换成 “泛型类型 T”，然后赋值给静态实例 instance
        instance = this as T;
    }
}
