using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotFixUi : Window
{
    private HotFixPanel m_Panel;
    private float m_SumTime=0;

    public override void Awake(params object[] paralist)
    {
        m_SumTime = 0;
        m_Panel = GameObject.GetComponent<HotFixPanel>();
        m_Panel.ImageSlider.fillAmount = 0;
        m_Panel.InfoText.text = $"下载中...{0}M/S";        
        HotPatchManager.Instance.ServerInfoError += ServerInfoError;
        HotPatchManager.Instance.ItemError += ItemError;

#if UNITY_EDITOR
        StartOnFinish();
#else
        // 判断是否需要解压
        if (HotPatchManager.Instance.ComputeUnPackFile() == true)
        {
            m_Panel.InfoText.text = "解压中... ...";
            HotPatchManager.Instance.StartUnPackFile(()=> {
                m_SumTime = 0;
                HotFix();
            });
        }
        else { 
            HotFix();

        }

#endif
    }

    private void HotFix()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            // 提示网络错误，检测网路连接是否正常
            GameStart.OpenCommonConfirm("网路连接失败","网络连接失败，请检查网络是否正常？",
                ()=> { Application.Quit(); }, () => { Application.Quit(); });
        }
        else {
            CheckVersion();
        }
    }
    void CheckVersion() {
        HotPatchManager.Instance.CheckVersion((isHot)=> {
            if (isHot == true)
            {
                // 提示玩家是否热更下载
                GameStart.OpenCommonConfirm("热更确定",string.Format("当前版本为{0}，有{1:F}M大小热更包，是否确认下载?",HotPatchManager.Instance.CurVersion,HotPatchManager.Instance.LoadSumSize/1024.0f),
                    OnClickStartDownload, OnClickCancelDownload);
            }
            else {
                StartOnFinish();
            }
        });
    }

    void OnClickStartDownload() {
        if (Application.platform == RuntimePlatform.IPhonePlayer
            || Application.platform == RuntimePlatform.Android)
        {
            // 数据流量网络
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                GameStart.OpenCommonConfirm("下载确认","当前使用的是手机流量，是否继续下载？",
                    StartDownload,OnClickCancelDownload);
            }
        }
        else {
            StartDownload();
        }
    }
    void OnClickCancelDownload() {
        Application.Quit();
    }

    /// <summary>
    /// 正式开始下载
    /// </summary>
    void StartDownload() {
        m_Panel.InfoText.text = "下载中。。。";
        m_Panel.InfoGo.SetActive(true);
        m_Panel.ContentText.text = HotPatchManager.Instance.CurPatches.Desc;
        GameStart.Instance.StartCoroutine(HotPatchManager.Instance.StartDownloadAB(StartOnFinish));
    }

    /// <summary>
    /// 下载完成回调，或者没有下载的东西直接进入游戏
    /// </summary>
    void StartOnFinish() {
        GameStart.Instance.StartCoroutine(OnFinish());
    }

    IEnumerator OnFinish() { 
        yield return GameStart.Instance.StartCoroutine(GameStart.Instance.StarGame(m_Panel.ImageSlider, m_Panel.InfoText));
        UIManager.Instance.CloseWnd(this);
    }

    public override void OnUpdate()
    {
        if (HotPatchManager.Instance.IsStartUnPack == true)
        {
            m_SumTime += Time.deltaTime;
            m_Panel.ImageSlider.fillAmount = HotPatchManager.Instance.GetUnPackProgress();
            float speed = (HotPatchManager.Instance.AlreadyUnPackSize / 1024.0f) / m_SumTime;
            m_Panel.SpeedText.text = $"{speed}M/S";
        }

        if (HotPatchManager.Instance.StartDownload==true)
        {
            m_SumTime += Time.deltaTime;
            m_Panel.ImageSlider.fillAmount = HotPatchManager.Instance.GetProgress();
            float speed = (HotPatchManager.Instance.GetLoadSize() / 1024.0f) / m_SumTime;
            m_Panel.SpeedText.text = $"{speed}M/S";
        }
    }
    public override void OnClose()
    {
        HotPatchManager.Instance.ServerInfoError -= ServerInfoError;
        HotPatchManager.Instance.ItemError -= ItemError;

        //加载场景
        GameMapManager.Instance.LoadScene(ConStr.MENUSCENE);
    }

    private void ItemError(string obj)
    {
        GameStart.OpenCommonConfirm("资源下载失败",string.Format("{0}等资源下载失败，请重新尝试已下载",obj),
            ReDownload,Application.Quit);
    }

    /// <summary>
    /// 重新下载
    /// </summary>
    private void ReDownload()
    {
        HotPatchManager.Instance.CheckVersion((isHot)=> {
            if (isHot)
            {
                StartDownload();
            }
            else {
                StartOnFinish();
            }
        });
    }

    private void ServerInfoError()
    {
        GameStart.OpenCommonConfirm("服务器列表获取失败", "服务器列表获取失败，请检查网络连接是否正常？尝试重新下载！",
            CheckVersion,Application.Quit);
    }
}
