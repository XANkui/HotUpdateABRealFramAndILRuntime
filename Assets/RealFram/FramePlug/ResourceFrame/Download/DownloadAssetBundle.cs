using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadAssetBundle : DownloadItem
{
    UnityWebRequest m_UnityWebRequest;
    public DownloadAssetBundle(string url, string path) : base(url, path)
    {
    }

    public override IEnumerator Download(Action callback = null)
    {
        m_UnityWebRequest = UnityWebRequest.Get(m_Url);
        m_StartDownload = true;
        m_UnityWebRequest.timeout = 30;
        yield return m_UnityWebRequest.SendWebRequest();
        if (m_UnityWebRequest.isNetworkError == true)
        {
            Debug.LogError($"Download Error : {m_UnityWebRequest.error}");
        }
        else {
            byte[] bytes = m_UnityWebRequest.downloadHandler.data;
            FileTool.CreateFile(m_SaveFilePath,bytes);
            if (callback!=null)
            {
                callback();
            }
        }
    }

    public override float GetProcess()
    {
        if (m_UnityWebRequest!=null)
        {
            return m_UnityWebRequest.downloadProgress;
        }
        return 0;
    }

    public override long GetCurLength()
    {
        return 0;
    }

    public override long GetLength()
    {
        if (m_UnityWebRequest != null)
        {
           return (long) m_UnityWebRequest.downloadedBytes;
        }

        return 0;
    }

    public override void Destroy()
    {
        if (m_UnityWebRequest!=null)
        {
            m_UnityWebRequest.Dispose();
            m_UnityWebRequest = null;
        }
    }
}
