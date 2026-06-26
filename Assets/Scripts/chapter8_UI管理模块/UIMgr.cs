using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 层级枚举
/// </summary>
public enum E_UILayer
{
    /// <summary>
    /// 最底层
    /// </summary>
    Bottom,
    /// <summary>
    /// 中层
    /// </summary>
    Middle,
    /// <summary>
    /// 高层
    /// </summary>
    Top,
    /// <summary>
    /// 系统层 最高层
    /// </summary>
    System,
}

/// <summary>
/// 管理所有UI面板的管理器
/// 注意：面板预设体名要和面板类名一致！！！！！
/// </summary>
public class UIMgr : BaseManager<UIMgr>
{
    private Camera uiCamera;
    private Canvas uiCanvas;
    private EventSystem uiEventSystem;

    //层级父对象
    private Transform bottomLayer;
    private Transform middleLayer;
    private Transform topLayer;
    private Transform systemLayer;
    
    
    private UIMgr()
    {
        //动态创建唯一的Canvas和EventSystem（摄像机）
        uiCamera = GameObject.Instantiate(Resources.Load<GameObject>("UI/UICamera")).GetComponent<Camera>();
        //ui摄像机过场景不移除 专门用来渲染UI面板
        GameObject.DontDestroyOnLoad(uiCamera.gameObject);
    
        //动态创建Canvas
        uiCanvas = GameObject.Instantiate(Resources.Load<GameObject>("UI/Canvas")).GetComponent<Canvas>();
        //设置使用的UI摄像机
        uiCanvas.worldCamera = uiCamera;
        //过场景不移除
        GameObject.DontDestroyOnLoad(uiCanvas.gameObject);
    
        //找到层级父对象
        bottomLayer = uiCanvas.transform.Find("Bottom");
        middleLayer = uiCanvas.transform.Find("Middle");
        topLayer = uiCanvas.transform.Find("Top");
        systemLayer = uiCanvas.transform.Find("System");
    
        //动态创建EventSystem
        uiEventSystem = GameObject.Instantiate(Resources.Load<GameObject>("UI/EventSystem")).GetComponent<EventSystem>();
        GameObject.DontDestroyOnLoad(uiEventSystem.gameObject);
    }
    
    /// <summary>
    /// 获取对应层级的父对象
    /// </summary>
    /// <param name="layer">层级枚举值</param>
    /// <returns></returns>
    public Transform GetLayerFather(E_UILayer layer)
    {
        switch (layer)
        {
            case E_UILayer.Bottom:
                return bottomLayer;
            case E_UILayer.Middle:
                return middleLayer;
            case E_UILayer.Top:
                return topLayer;
            case E_UILayer.System:
                return systemLayer;
            default:
                return null;
        }
    }
}
