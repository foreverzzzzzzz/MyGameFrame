using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 抽屉（池子中的数据）对象  代替字典中存储各类对象的栈
/// </summary>
public class PoolData
{
    //用来存储抽屉中的对象
    private Stack<GameObject> dataStack = new Stack<GameObject>();
    
    //用来记录使用中的对象的   物体不会同时存在于这两个容器中
    private List<GameObject> usedList = new List<GameObject>();

    //抽屉上限 场景上同时存在的对象的上限个数
    private int maxNum;
    
    //抽屉根对象 用来进行布局管理的对象
    private GameObject rootObj;
    
    public int UsedCount => usedList.Count;

    /// <summary>
    /// 进行使用中对象数量和最大容量进行比较 小于返回true 需要实例化
    /// </summary>
    public bool NeedCreate => usedList.Count < maxNum;

    
    /// <summary>
    /// 初始化构造函数
    /// </summary>
    /// <param name="root">柜子（缓存池）父对象</param>
    /// <param name="name">抽屉父对象的名字</param>
    public PoolData(GameObject root, string name, GameObject usedObj)
    {
        //开启功能时 才会动态创建 建立父子关系
        if(PoolMgr.isOpenLayout)
        {
            //创建抽屉父对象
            rootObj = new GameObject(name);
            //和柜子父对象建立父子关系
            rootObj.transform.SetParent(root.transform);
        }
        
        //创建抽屉时 外部肯定是会动态创建一个对象的
        //我们应该将其记录到 使用中的对象容器中
        PushUsedList(usedObj);

        PoolObj poolObj = usedObj.GetComponent<PoolObj>();
        if (poolObj == null)
        {
            Debug.LogError("请为使用缓存池功能的预设体对象挂载PoolObj脚本 用于设置数量上限");
            return;
        }
        //记录上限数量值
        maxNum = poolObj.maxNum;

    }

    
    //获取容器中是否有对象
    public int Count => dataStack.Count;
    /// <summary>
    /// 从抽屉中弹出数据对象
    /// </summary>
    /// <returns>想要的对象数据</returns>
    public GameObject Pop()
    {
        //取出对象
        GameObject obj;
        
        if (Count > 0)
        {
            //从没有的容器当中取出使用
            obj = dataStack.Pop();
            //现在要使用了 应该要用使用中的容器记录它
            usedList.Add(obj);
        }
        else
        {
            //取0索引的对象 代表的就是使用时间最长的对象
            obj = usedList[0];
            //并且把它从使用着的对象中移除
            usedList.RemoveAt(0);
            //由于它还要拿出去用，所以我们应该把它又记录到 使用中的容器中去 
            //并且添加到尾部 表示 比较新的开始
            usedList.Add(obj);
        }
        
        //激活对象
        obj.SetActive(true);
        //断开父子关系
        if (PoolMgr.isOpenLayout)
            obj.transform.SetParent(null);

        return obj;
    }


    /// <summary>
    /// 将物体放入到抽屉对象中
    /// </summary>
    /// <param name="obj"></param>
    public void Push(GameObject obj)
    {
        //失活放入抽屉的对象
        obj.SetActive(false);
        //放入对应抽屉的根物体中 建立父子关系
        if (PoolMgr.isOpenLayout)
            obj.transform.SetParent(rootObj.transform);
        //通过栈记录对应的对象数据
        dataStack.Push(obj);
        //这个对象已经不再使用了 应该把它从记录容器中移除
        usedList.Remove(obj);

    }
    
    /// <summary>
    /// 将对象压入到使用中的容器中记录
    /// </summary>
    /// <param name="obj"></param>
    public void PushUsedList(GameObject obj)
    {
        usedList.Add(obj);
    }

}

public class PoolMgr : BaseManager<PoolMgr>
{
    //柜子容器当中有抽屉的体现
    private Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();

    //池子根对象
    private GameObject poolObj;

    //是否开启布局功能
    public static bool isOpenLayout = true;
    
    private PoolMgr() { }

    /// <summary>
    /// 拿东西的方法
    /// </summary>
    /// <param name="name">抽屉容器的名字</param>
    /// <returns>从缓存池中取出的对象</returns>
    public GameObject GetObj(string name)
    {
        //如果根物体为空 就创建
        if (poolObj == null && isOpenLayout)
            poolObj = new GameObject("Pool");
        
        GameObject obj;

        #region 没有上限时的逻辑
        // //有抽屉 并且 抽屉里 有对象 才去直接拿
        // if(poolDic.ContainsKey(name) && poolDic[name].Count > 0)
        // {
        //     //弹出栈中的对象 直接返回给外部使用
        //     obj = poolDic[name].Pop();
        // }
        // //否则，就应该去创造
        // else
        // {
        //     //没有的时候 通过资源加载 去实例化出一个GameObject
        //     obj = GameObject.Instantiate(Resources.Load<GameObject>(name));
        //     //避免实例化出来的对象 默认会在名字后面加一个(Clone)
        //     //我们重命名过后 方便往里面放
        //     obj.name = name;
        // }
        #endregion

        #region 加入上限的逻辑

        if(!poolDic.ContainsKey(name) ||
           (poolDic[name].Count == 0 && poolDic[name].NeedCreate))
        {
            //动态创建对象
            //没有的时候 通过资源加载 去实例化出一个GameObject
            obj = GameObject.Instantiate(Resources.Load<GameObject>(name));
            //避免实例化出来的对象 默认会在名字后面加一个(Clone)
            //我们重命名过后 方便往里面放
            obj.name = name;

            //创建抽屉
            if(!poolDic.ContainsKey(name))
                poolDic.Add(name, new PoolData(poolObj, name, obj));
            else//实例化出来的对象 需要记录到使用中的对象容器中
                poolDic[name].PushUsedList(obj);
        }
        //当抽屉中有对象 或者 使用中的对象超上限了 直接去取出来用
        else
        {
            obj = poolDic[name].Pop();
        }


        #endregion
        

        return obj;
    }


    /// <summary>
    /// 往缓存池中放入对象
    /// </summary>
    /// <param name="name">抽屉（对象）的名字</param>
    /// <param name="obj">希望放入的对象</param>
    public void PushObj(GameObject obj)
    {
        // //总之，目的就是要把对象隐藏起来
        // //并不是直接移除对象 而是将对象失活 一会儿再用 用的时候再激活它
        // //除了这种方式，还可以把对象放倒屏幕外看不见的地方
        // obj.SetActive(false);
        //
        // //把失活的对象（要放入抽屉中的对象） 父对象先设置为 柜子（缓存池）根对象
        // obj.transform.SetParent(poolObj.transform);

        
        //没有抽屉 创建抽屉
        // if(!poolDic.ContainsKey(obj.name))
        //     poolDic.Add(obj.name, new PoolData(poolObj, obj.name));

        //往抽屉当中放对象
        poolDic[obj.name].Push(obj);

        ////如果存在对应的抽屉容器 直接放
        //if(poolDic.ContainsKey(name))
        //{
        //    //往栈（抽屉）中放入对象
        //    poolDic[name].Push(obj);
        //}
        ////否则 需要先创建抽屉 再放
        //else
        //{
        //    //先创建抽屉
        //    poolDic.Add(name, new Stack<GameObject>());
        //    //再往抽屉里面放
        //    poolDic[name].Push(obj);
        //}
    }

    /// <summary>
    /// 用于清除整个柜子当中的数据 
    /// 使用场景 主要是 切场景时
    /// </summary>
    public void ClearPool()
    {
        poolDic.Clear();
        poolObj = null;
    }

}
