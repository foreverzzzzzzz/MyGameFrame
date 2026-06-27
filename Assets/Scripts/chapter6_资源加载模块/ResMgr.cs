using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 资源信息基类 主要用于里式替换原则 父类容器装子类对象
/// </summary>
public abstract class ResInfoBase { }

/// <summary>
/// 资源信息对象 主要用于存储资源信息 异步加载委托信息 异步加载 协程信息
/// </summary>
/// <typeparam name="T">资源类型</typeparam>
public class ResInfo<T> : ResInfoBase
{
    //资源
    public T asset;
    //主要用于异步加载结束后 传递资源到外部的委托
    public UnityAction<T> callBack;
    //用于存储异步加载时 开启的协同程序
    public Coroutine coroutine;
    //是否需要移除
    public bool isDel;
}

public class ResMgr : BaseManager<ResMgr>
{
    //用于存储加载过的资源或者加载中的资源的容器
    private Dictionary<string, ResInfoBase> resDic = new Dictionary<string, ResInfoBase>();
    
    private ResMgr() { }
    
    /// <summary>
    /// 同步加载Resources下资源的方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    /// 指定为UnityEngine的Object而不是System的，后面同理
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        string resName = path + "_" + typeof(T).Name;
        ResInfo<T> info;
        //字典中不存在资源时
        if (!resDic.ContainsKey(resName))
        {
            //直接同步加载 并且记录资源信息 到字典中 方便下次直接取出来用
            T res = Resources.Load<T>(path);
            info = new ResInfo<T>();
            info.asset = res;
            resDic.Add(resName, info);
            return res;
        }
        else
        {
            //取出字典中的记录
            info = resDic[resName] as ResInfo<T>;
            //存在异步加载 还在加载中
            if(info.asset == null)
            {
                //停止异步加载 
                MonoMgr.Instance.StopCoroutine(info.coroutine);
                //直接采用同步的方式加载成功
                T res = Resources.Load<T>(path);
                //记录 
                info.asset = res;
                //还应该把那些等待着异步加载结束的委托去执行了
                info.callBack?.Invoke(res);
                //回调结束 异步加载也停了 所以清除无用的引用
                info.callBack = null;
                info.coroutine = null;
                // 并使用
                return res;
            }
            else
            {
                //如果已经加载结束 直接用
                return info.asset;
            }
        }

    }
    
    /// <summary>
    /// 异步加载资源的方法
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径（Resources下的）</param>
    /// <param name="callBack">加载结束后的回调函数 当异步加载资源结束后才会调用</param>
    public void LoadAsync<T>(string path, UnityAction<T> callBack) where T: UnityEngine.Object
    {
        //资源的唯一ID，是通过 路径名_资源类型 拼接而成的
        string resName = path + "_" + typeof(T).Name;
        ResInfo<T> info;
        if (!resDic.ContainsKey(resName))
        {
            //声明一个 资源信息对象
            info = new ResInfo<T>();
            //将资源记录添加到字典中（资源还没有加载成功）
            resDic.Add(resName, info);
            //记录传入的委托函数 一会儿加载完成了 再使用
            info.callBack += callBack;
            //开启协程去进行 异步加载 并且记录协同程序 （用于之后可能的 停止）
            info.coroutine = MonoMgr.Instance.StartCoroutine(ReallyLoadAsync<T>(path));
        }
        else
        {
            //从字典中取出资源信息
            info = resDic[resName] as ResInfo<T>;
            //如果资源还没有加载完 
            //意味着 还在进行异步加载
            if (info.asset == null)
                //将自己的callBack也记录进去，等加载完后，两个callBack一块调用
                info.callBack += callBack;
            else
                callBack?.Invoke(info.asset);
        }

    }

    private IEnumerator ReallyLoadAsync<T>(string path) where T : UnityEngine.Object
    {
        //异步加载资源
        ResourceRequest rq = Resources.LoadAsync<T>(path);
        //等待资源加载结束后 才会继续执行yield return后面的代码
        //此处不返回任何东西，只是将rq交给协程调度器，每一帧都会检测rq.isDone 是否为 true，如果是才继续向后执行
        yield return rq;
        
        string resName = path + "_" + typeof(T).Name;
        //资源加载结束 将资源传到外部的委托函数去进行使用
        if (resDic.ContainsKey(resName))
        {
            ResInfo<T> resInfo = resDic[resName] as ResInfo<T>;
            //取出资源信息 并且记录加载完成的资源
            resInfo.asset = rq.asset as T;
            //将加载完成的资源传递出去 将存入的callBack一起调用
            //资源加载结束 将资源传到外部的委托函数去进行使用 真正的资源存储在rq.asset中
            //如果发现需要删除 再去移除资源
            if (resInfo.isDel)
                UnloadAsset<T>(path);
            else
            {
                //将加载完成的资源传递出去
                resInfo.callBack?.Invoke(resInfo.asset);
                //加载完毕后 这些引用就可以清空 避免引用的占用 可能带来的潜在的内存泄漏问题
                resInfo.callBack = null;
                resInfo.coroutine = null;
            }

        }

    }
    
    /// <summary>
    /// 异步加载资源的方法
    /// </summary>
    /// <param name="path">资源路径（Resources下的）</param>
    /// <param name="callBack">加载结束后的回调函数 当异步加载资源结束后才会调用</param>
    [Obsolete("注意：建议使用泛型加载方式，如果实在要用Type加载，一定不能和泛型加载混用去加载同类型同名资源")]
    public void LoadAsync(string path, Type type, UnityAction<UnityEngine.Object> callBack) 
    {
        //资源的唯一ID，是通过 路径名_资源类型 拼接而成的
        string resName = path + "_" + type.Name;
        ResInfo<UnityEngine.Object> info;
        if (!resDic.ContainsKey(resName))
        {
            //声明一个 资源信息对象
            info = new ResInfo<UnityEngine.Object>();
            //将资源记录添加到字典中（资源还没有加载成功）
            resDic.Add(resName, info);
            //记录传入的委托函数 一会儿加载完成了 再使用
            info.callBack += callBack;
            //开启协程去进行 异步加载 并且记录协同程序 （用于之后可能的 停止）
            info.coroutine = MonoMgr.Instance.StartCoroutine(ReallyLoadAsync(path, type));
        }
        else
        {
            //从字典中取出资源信息
            info = resDic[resName] as ResInfo<UnityEngine.Object>;
            //如果资源还没有加载完 
            //意味着 还在进行异步加载
            if (info.asset == null)
                info.callBack += callBack;
            else
                callBack?.Invoke(info.asset);
        }
    }

    private IEnumerator ReallyLoadAsync(string path, Type type)
    {
        //异步加载资源
        ResourceRequest rq = Resources.LoadAsync(path, type);
        //等待资源加载结束后 才会继续执行yield return后面的代码
        yield return rq;

        string resName = path + "_" + type.Name;
        //资源加载结束 将资源传到外部的委托函数去进行使用
        if (resDic.ContainsKey(resName))
        {
            ResInfo<UnityEngine.Object> resInfo = resDic[resName] as ResInfo<UnityEngine.Object>;
            //取出资源信息 并且记录加载完成的资源
            resInfo.asset = rq.asset;
            //如果发现需要删除 再去移除资源
            if (resInfo.isDel)
                UnloadAsset(path, type);
            else
            {
                //将加载完成的资源传递出去
                resInfo.callBack?.Invoke(resInfo.asset);
                //加载完毕后 这些引用就可以清空 避免引用的占用 可能带来的潜在的内存泄漏问题
                resInfo.callBack = null;
                resInfo.coroutine = null;
            }

        }
    }

    
    /// <summary>
    /// 指定卸载一个资源
    /// </summary>
    /// <param name="assetToUnload"></param>
    public void UnloadAsset<T>(string path)
    {
        string resName = path + "_" + typeof(T).Name;
        //判断是否存在对应资源
        if(resDic.ContainsKey(resName))
        {
            ResInfo<T> resInfo = resDic[resName] as ResInfo<T>;
            //资源已经加载结束 
            if(resInfo.asset != null)
            {
                //从字典移除
                resDic.Remove(resName);
                //通过api 卸载资源
                Resources.UnloadAsset(resInfo.asset as UnityEngine.Object);
            }
            else//资源正在异步加载中
            {
                //MonoMgr.Instance.StopCoroutine(resInfo.coroutine);
                //resDic.Remove(resName);
                //为了保险起见 一定要让资源移除了
                //改变表示 待删除
                resInfo.isDel = true;
            }
        }
    }

    //实现非泛型加载时调用的卸载方法
    public void UnloadAsset(string path, Type type)
    {
        string resName = path + "_" + type.Name;
        //判断是否存在对应资源
        if (resDic.ContainsKey(resName))
        {
            ResInfo<UnityEngine.Object> resInfo = resDic[resName] as ResInfo<UnityEngine.Object>;
            //资源已经加载结束 
            if (resInfo.asset != null)
            {
                //从字典移除
                resDic.Remove(resName);
                //通过api 卸载资源
                Resources.UnloadAsset(resInfo.asset);
            }
            else//资源正在异步加载中
            {
                //MonoMgr.Instance.StopCoroutine(resInfo.coroutine);
                //resDic.Remove(resName);
                //为了保险起见 一定要让资源移除了
                //改变表示 待删除
                resInfo.isDel = true;
            }
        }
    }

    /// <summary>
    /// 异步卸载对应没有使用的Resources相关的资源
    /// </summary>
    /// <param name="callBack">回调函数</param>
    public void UnloadUnusedAssets(UnityAction callBack)
    {
        MonoMgr.Instance.StartCoroutine(ReallyUnloadUnusedAssets(callBack));
    }

    private IEnumerator ReallyUnloadUnusedAssets(UnityAction callBack)
    {
        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        //卸载完毕后 通知外部
        callBack();
    }

}
