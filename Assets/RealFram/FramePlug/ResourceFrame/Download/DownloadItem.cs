using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class DownloadItem 
{
    /// <summary>
    /// 网络资源 Url 路径
    /// </summary>
    protected string m_Url;
    public string Url { get => m_Url; }

    /// <summary>
    /// 资源下载存放路径，不包含文件（跟路径）
    /// </summary>
    protected string m_SavePath;
    public string SavePath { get => m_SavePath; }

    /// <summary>
    /// 文件后缀
    /// </summary>
    protected string m_FileExt;
    public string FileExt { get => m_FileExt; }

    /// <summary>
    /// 文件名，不包含后缀
    /// </summary>
    protected string m_FileNameWithoutExt;
    public string FileNameWithoutExt { get => m_FileNameWithoutExt; }

    /// <summary>
    /// 文件名，包含后缀
    /// </summary>
    protected string m_FileName;
    public string FileName { get => m_FileName; }

    /// <summary>
    /// 下载文件的全路径，（路径+文件名+后缀）
    /// </summary>
    protected string m_SaveFilePath;
    public string SaveFilePath { get => m_SaveFilePath; }

    /// <summary>
    /// 源文件大小
    /// </summary>
    protected long m_FileLength;
    public long FileLength { get => m_FileLength; }

    /// <summary>
    /// 当前下载的大小
    /// </summary>
    protected string m_CurLength;
    public string CurLength { get => m_CurLength; }

    /// <summary>
    /// 是否开始下载
    /// </summary>
    protected bool m_StartDownload;
    public bool StartDownload { get => m_StartDownload; }

    public DownloadItem(string url,string path) {
        m_Url = url;
        m_SavePath = path;
        m_StartDownload = false;
        m_FileNameWithoutExt = Path.GetFileNameWithoutExtension(m_Url);
        m_FileExt = Path.GetExtension(m_Url);
        m_FileName = string.Format("{0}{1}",m_FileNameWithoutExt,m_FileExt);
        m_SaveFilePath = string.Format("{0}/{1}{2}",m_SavePath,m_FileNameWithoutExt,m_FileExt);
    }

    public virtual IEnumerator Download(Action callback=null) {
        yield return null;
    }

    /// <summary>
    /// 获取下载进度
    /// </summary>
    /// <returns></returns>
    public abstract float GetProcess();

    /// <summary>
    /// 获取当前下载的文件当前已下载的大小
    /// </summary>
    /// <returns></returns>
    public abstract long GetCurLength();

    /// <summary>
    /// 获取当前下载的文件大小
    /// </summary>
    /// <returns></returns>
    public abstract long GetLength();

    public abstract void Destroy();
}
