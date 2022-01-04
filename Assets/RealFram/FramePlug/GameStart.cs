using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameStart : MonoSingleton<GameStart>
{
    private GameObject m_obj;
    protected override void Awake()
    {
        base.Awake();
        GameObject.DontDestroyOnLoad(gameObject);
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
        HotPatchManager.Instance.Init(this);
        UIManager.Instance.Init(transform.Find("UIRoot") as RectTransform, transform.Find("UIRoot/WndRoot") as RectTransform, transform.Find("UIRoot/UICamera").GetComponent<Camera>(), transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>());
        RegisterUI();
    }
    // Use this for initialization
    void Start ()
    {
        UIManager.Instance.PopUpWnd(ConStr.HOTFIXPANEL,isResourceLoad:true);
       
    }

    public IEnumerator StarGame(Image image, Text text) {
        image.fillAmount = 0;
        yield return null;
        text.text = "加载本地数据......";
        AssetBundleManager.Instance.LoadAssetBundleConfig();

        image.fillAmount = 0.2f;
        yield return null;
        text.text = "加载热更 DLL ......";
        ILRuntimeManager.Instance.Init();

        image.fillAmount = 0.3f;
        yield return null;
        text.text = "加载数据表......";
        LoadConfiger();

        image.fillAmount = 0.6f;
        yield return null;
        text.text = "准备UI相关数据......";

        image.fillAmount = 0.9f;
        yield return null;
        text.text = "初始化地图数据......";
        GameMapManager.Instance.Init(this);

        image.fillAmount = 1.0f;
    }

    //注册UI窗口
    void RegisterUI()
    {
        UIManager.Instance.Register<Window>(ConStr.MENUPANEL);
        UIManager.Instance.Register<Window>(ConStr.LOADINGPANEL);
        UIManager.Instance.Register<HotFixUi>(ConStr.HOTFIXPANEL);
    }

    //加载配置表
    void LoadConfiger()
    {
        //ConfigerManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
        //ConfigerManager.Instance.LoadData<BuffData>(CFG.TABLE_BUFF);
    }
	
	// Update is called once per frame
	void Update ()
    {
        UIManager.Instance.OnUpdate();
	}

    public static void OpenCommonConfirm(string title, string content,
        UnityAction confirmAction, UnityAction cancelAction) {
        GameObject comConfrmObj = GameObject.Instantiate(Resources.Load<GameObject>("CommonConfirm"));
        comConfrmObj.transform.SetParent(UIManager.Instance.m_WndRoot,false);
        CommonConfirm commonItem = comConfrmObj.GetComponent<CommonConfirm>();
        commonItem.Show(title,content,confirmAction,cancelAction);
    }

    private void OnDestroy()
    {
        ILRuntimeManager.Instance.OnDestroy();
    }
    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
        Debug.Log("清空编辑器缓存");
#endif
    }
}
