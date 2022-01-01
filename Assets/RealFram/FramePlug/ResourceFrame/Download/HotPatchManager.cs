using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class HotPatchManager : Singleton<HotPatchManager>
{
	private MonoBehaviour m_Mono;
	private string m_UnPackPath = Application.persistentDataPath + "/Origin";
	private string m_DownloadPath = Application.persistentDataPath + "/Download";
	private string m_CurVersion;
	public string CurVersion { get => m_CurVersion; }
	private string m_CurPackName;
	private string m_ServerXmlPath = Application.persistentDataPath + "/ServerInfo.xml";
	private string m_LocalSerVerXmlPath = Application.persistentDataPath + "/LocalServerInfo.xml";
	private ServerInfo m_ServerInfo;
	private ServerInfo m_LocalServerInfo;
	private VersionInfo m_GameVersionInfo;
	// 当前热更 Patches
	private Patches m_CurPatches;
	public Patches CurPatches { get => m_CurPatches; }
	// 所有热更的东西
	private Dictionary<string, Patch> m_HotFixDict = new Dictionary<string, Patch>();
	// 所有需要下载的东西
	private Dictionary<string, Patch> m_DownloadDIct = new Dictionary<string, Patch>();
	// 所有需要的下载的东西 List
	private List<Patch> m_DownloadList = new List<Patch>();
	// 服务器上的资源名对应的MD5，用于下载后 MD5 校验
	private Dictionary<string, string> m_DownloadMD5Dict = new Dictionary<string, string>();
	// 计算需要解压的文件
	private List<string> m_UnPackedList = new List<string>();
	// 原包记录的 MD5码
	private Dictionary<string, ABMD5Base> m_PackedMD5Dict = new Dictionary<string, ABMD5Base>();
	// 服务器列表获取错误回调
	public Action ServerInfoError;
	// 文件下载出错的回调
	public Action<string> ItemError;
	// 文件下载完成回调
	public Action LoadOver;
	// 储存已经下载的资源
	public List<Patch> m_AlreadyDownList = new List<Patch>();
	// 是否开始下载
	public bool StartDownload = false;
	// 尝试重新下载次数
	private int m_TryDownCount = 0;
	private const int DOWNLOAD_COUNT = 4;
	// 当前正在下载的资源
	private DownloadAssetBundle m_CurDownload = null;
	/// 需要下载资源的总个数
	public int LoadFileCount { get; set; } = 0;
	/// 需要下载资源的总大小 KB
	public float LoadSumSize { get; set; } = 0;
	// 是否开始解压
	public bool IsStartUnPack = false;
	// 解压文件总大小
	public float UnPackSumSize { get; set; } = 0;
	// 已经解压大小
	public float AlreadyUnPackSize { get; set; } = 0;

	public void Init(MonoBehaviour mono) {
		m_Mono = mono;

		ReadLocalMD5();
	}

	/// <summary>
	/// 读取本地 MD5 
	/// </summary>
	void ReadLocalMD5() {
		m_PackedMD5Dict.Clear();
		TextAsset md5 = Resources.Load<TextAsset>("ABMD5");
        if (md5==null)
        {
			Debug.LogError("为获取到本地 MD5");
			return;
        }

        using (MemoryStream stream = new MemoryStream(md5.bytes))
        {
			BinaryFormatter bf = new BinaryFormatter();
			ABMD5 abmd5 = bf.Deserialize(stream) as ABMD5;
            foreach (ABMD5Base abmd5Base in abmd5.ABMD5List)
            {
				m_PackedMD5Dict.Add(abmd5Base.Name, abmd5Base);
            }
        }
	}

	/// <summary>
	/// 计算需要解压的文件
	/// </summary>
	/// <returns></returns>
	public bool ComputeUnPackFile() {

#if UNITY_ANDROID

		if (Directory.Exists(m_UnPackPath)==false)
        {
			Directory.CreateDirectory(m_UnPackPath);
        }

		m_UnPackedList.Clear();

        foreach (string fileName in m_PackedMD5Dict.Keys)
        {
			string filePath = m_UnPackPath + "/" + fileName;
            if (File.Exists(filePath)==true)
            {
				string md5 = MD5Manager.Instance.BuildFileMd5(filePath);
                if (m_PackedMD5Dict[fileName].Md5 != md5)
                {
					m_UnPackedList.Add(fileName);
                }
            }
            else{

				m_UnPackedList.Add(fileName);
			}
        }

        foreach (string fileName in m_UnPackedList)
        {
            if (m_PackedMD5Dict.ContainsKey(fileName)==true)
            {
				UnPackSumSize += m_PackedMD5Dict[fileName].Size;
            }
        }

		return m_UnPackedList.Count > 0;

#else
		return false;
#endif
	}

	/// <summary>
	/// 开始解压文件
	/// </summary>
	/// <param name="callback"></param>
	public void StartUnPackFile(Action callback) {
		IsStartUnPack = true;
		m_Mono.StartCoroutine(UnPackToPersistenDataPath(callback));
	}

	/// <summary>
	/// 将包里的原始资源你解压到本地
	/// </summary>
	/// <param name="callback"></param>
	/// <returns></returns>
	IEnumerator UnPackToPersistenDataPath(Action callback) {
        foreach (string fileName in m_UnPackedList)
        {
			UnityWebRequest unityWebRequest = UnityWebRequest.Get(Application.streamingAssetsPath+"/"+fileName);
			unityWebRequest.timeout = 30;
			yield return unityWebRequest.SendWebRequest();
			if (unityWebRequest.isHttpError == true)
			{
				Debug.LogError("Unpack Error : " + unityWebRequest.error);
			}
			else {
				byte[] bytes = unityWebRequest.downloadHandler.data;
				FileTool.CreateFile(m_UnPackPath+"/"+fileName,bytes);
			}

            if (m_PackedMD5Dict.ContainsKey(fileName)==true)
            {
				AlreadyUnPackSize += m_PackedMD5Dict[fileName].Size;
            }
			unityWebRequest.Dispose();
		}

        if (callback!=null)
        {
			callback.Invoke();
        }

		IsStartUnPack = false;
	}

	/// <summary>
	/// 获取解药进度
	/// </summary>
	/// <returns></returns>
	public float GetUnPackProgress() {
		return AlreadyUnPackSize / UnPackSumSize;
	}

	/// <summary>
	/// 计算AB包路径
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public string ComputeABPath(string name) {
		Patch patch = null;
		m_HotFixDict.TryGetValue(name,out patch);
        if (patch!=null)
        {
			return m_DownloadPath + "/" + name;
        }

		return "";
	}	

	/// <summary>
	/// 检测版本
	/// </summary>
	/// <param name="hotCallback"></param>
	public void CheckVersion(Action<bool> hotCallback=null) {
		m_TryDownCount = 0;
		m_HotFixDict.Clear();
		ReadVersion();
		m_Mono.StartCoroutine(ReadXml(() => {
            if (m_ServerInfo==null)
            {
                if (ServerInfoError!=null)
                {
					ServerInfoError.Invoke();
                }
				return;
            }

            foreach (VersionInfo version in m_ServerInfo.GameVersion)
            {
                if (version.Version == m_CurVersion)
                {
					m_GameVersionInfo = version;
					break;
                }
            }

			GetHotAB();
			if (CheckLocalAndServerPatch() == true) // 需要更新ServerInfo.xml文件
			{
				ComputeDownload();
                if (File.Exists(m_ServerXmlPath))
                {
                    if (File.Exists(m_LocalSerVerXmlPath))
                    {
						File.Delete(m_LocalSerVerXmlPath);
                    }

					File.Move(m_ServerXmlPath,m_LocalSerVerXmlPath);
                }
			}
			else {
				CheckLocalResource();
			}

			LoadFileCount = m_DownloadList.Count;
			LoadSumSize = m_DownloadList.Sum(x=>x.Size);
			
            if (hotCallback!=null)
            {
				hotCallback.Invoke(m_DownloadList.Count>0);
            }
		}));
	}

	/// <summary>
	/// 检查本地资源，是否与服务器下载列表信息一致
	/// 主要是避免下载一半，游戏退出，未完全更新完的情况
	/// </summary>
	void CheckLocalResource() {
		m_DownloadDIct.Clear();
		m_DownloadList.Clear();
		m_DownloadMD5Dict.Clear();
		if (m_GameVersionInfo != null && m_GameVersionInfo.Patches != null
			&& m_GameVersionInfo.Patches.Length > 0)
		{
			m_CurPatches = m_GameVersionInfo.Patches[m_GameVersionInfo.Patches.Length - 1];
			if (m_CurPatches.Files != null && m_CurPatches.Files.Count > 0)
			{
				foreach (Patch patch in m_CurPatches.Files)
				{
					if ((Application.platform == RuntimePlatform.WindowsEditor
						|| Application.platform == RuntimePlatform.WindowsPlayer)
						&& patch.Platform.Contains("StandaloneWindows64"))
					{
						AddDownloadList(patch);
					}
					else if ((Application.platform == RuntimePlatform.Android
						|| Application.platform == RuntimePlatform.WindowsEditor)
						&& patch.Platform.Contains("Android"))
					{
						AddDownloadList(patch);
					}
					else if ((Application.platform == RuntimePlatform.IPhonePlayer
						|| Application.platform == RuntimePlatform.WindowsEditor)
						&& patch.Platform.Contains("iOS"))
					{
						AddDownloadList(patch);
					}
				}
			}
		}
	}

	/// <summary>
	/// 检测本地和服务器的 ServerInfo 是否一致
	/// 不一致则更新
	/// </summary>
	/// <returns>true：需要更新</returns>
	bool CheckLocalAndServerPatch() {
        if (File.Exists(m_LocalSerVerXmlPath)==false)
        {
			return true;
        }

		m_LocalServerInfo = BinarySerializeOpt.XmlDeserialize(m_LocalSerVerXmlPath,typeof(ServerInfo)) as ServerInfo;
		VersionInfo localGameVersion = null;
        if (m_LocalServerInfo!=null)
        {
            foreach (VersionInfo version in m_LocalServerInfo.GameVersion)
            {
                if (version.Version==m_CurVersion)
                {
					localGameVersion = version;
					break;
                }
            }
        }

        if (localGameVersion!=null && m_GameVersionInfo.Patches!=null
			&& localGameVersion.Patches!=null && m_GameVersionInfo.Patches.Length>0
			&& m_GameVersionInfo.Patches[m_GameVersionInfo.Patches.Length-1].Version
			!=localGameVersion.Patches[localGameVersion.Patches.Length-1].Version)
        {
			return true;
        }

		return false;
	}

	/// <summary>
	/// 读取打包时的版本
	/// </summary>
	void ReadVersion() {
		TextAsset versionTex = Resources.Load<TextAsset>("Version");
        if (versionTex==null)
        {
			Debug.LogError("未读到本地版本");
			return;
        }

		string[] all = versionTex.text.Split('\n');
        if (all.Length>0)
        {
			string[] infoList = all[0].Split(';');
            if (infoList.Length>=2)
            {
				m_CurVersion = infoList[0].Split('|')[1];
				m_CurPackName = infoList[1].Split('|')[1];
            }
        }
	}

	IEnumerator ReadXml(Action callback) {
		string xmlUrl = "http://127.0.0.1/ServerInfo.xml";
		UnityWebRequest webRequest = UnityWebRequest.Get(xmlUrl);
		webRequest.timeout = 30;
		yield return webRequest.SendWebRequest();

		if (webRequest.isHttpError)
		{
			Debug.LogError($"Download Error :{webRequest.error}");
		}
		else {
			FileTool.CreateFile(m_ServerXmlPath,webRequest.downloadHandler.data);
			if (File.Exists(m_ServerXmlPath))
			{
				m_ServerInfo = BinarySerializeOpt.XmlDeserialize(m_ServerXmlPath,typeof(ServerInfo)) as ServerInfo;

			}
			else {
				Debug.LogError("热梗配置读取错误");
			}
		}

        if (callback!=null)
        {
			callback.Invoke();
        }
	}

	/// <summary>
	/// 获取所有热更包信息
	/// </summary>
	void GetHotAB() {
		m_HotFixDict.Clear();
        if (m_GameVersionInfo!=null && m_GameVersionInfo.Patches!=null
			&& m_GameVersionInfo.Patches.Length > 0)
        {
			Patches lastPatches = m_GameVersionInfo.Patches[m_GameVersionInfo.Patches.Length-1];
            foreach (Patch patch in lastPatches.Files)
            {
				m_HotFixDict.Add(patch.Name,patch);
            }
		}
	}

	/// <summary>
	/// 计算要下载的资源
	/// </summary>
	void ComputeDownload() {
		m_DownloadDIct.Clear();
		m_DownloadList.Clear();
		m_DownloadMD5Dict.Clear();
		if (m_GameVersionInfo != null && m_GameVersionInfo.Patches != null
			&& m_GameVersionInfo.Patches.Length > 0)
		{
			m_CurPatches = m_GameVersionInfo.Patches[m_GameVersionInfo.Patches.Length - 1];
            if (m_CurPatches.Files !=null && m_CurPatches.Files.Count>0)
            {
                foreach (Patch patch in m_CurPatches.Files)
                {
					if ((Application.platform == RuntimePlatform.WindowsEditor
						|| Application.platform == RuntimePlatform.WindowsPlayer)
						&& patch.Platform.Contains("StandaloneWindows64"))
					{
						AddDownloadList(patch);
					}
					else if ((Application.platform == RuntimePlatform.Android
						|| Application.platform == RuntimePlatform.WindowsEditor)
						&& patch.Platform.Contains("Android"))
					{
						AddDownloadList(patch);
					}
					else if ((Application.platform == RuntimePlatform.IPhonePlayer
						|| Application.platform == RuntimePlatform.WindowsEditor)
						&& patch.Platform.Contains("iOS"))
					{
						AddDownloadList(patch);
					}
				}
            }
		}
	}

	/// <summary>
	/// 把要在下载的热更资源添加到列表中
	/// </summary>
	/// <param name="patch"></param>
	private void AddDownloadList(Patch patch)
	{
		string filePath = m_DownloadPath + "/" + patch.Name;
		// 存在这个文件时，进行对比看是否与服务器MD5码一致，不一致放到下载队列中；
		// 如果不存在，则直接放入下载队列
		if (File.Exists(filePath))
		{
			string md5 = MD5Manager.Instance.BuildFileMd5(filePath);
			if (patch.Md5 != md5)
			{
				m_DownloadList.Add(patch);
				m_DownloadDIct.Add(patch.Name, patch);
				m_DownloadMD5Dict.Add(patch.Name, patch.Md5);
			}
		}
		else {
			m_DownloadList.Add(patch);
			m_DownloadDIct.Add(patch.Name, patch);
			m_DownloadMD5Dict.Add(patch.Name, patch.Md5);
		}
    }
	/// <summary>
	/// 获取下载进度
	/// </summary>
	/// <returns></returns>
	public float GetProgress() {

		return GetLoadSize() / LoadSumSize;
	}

	/// <summary>
	/// 获取已经下载的总大小
	/// </summary>
	/// <returns></returns>
	public float GetLoadSize() {
		float alreadySize = m_AlreadyDownList.Sum(x => x.Size);
		float curAlreadySize = 0;
		if (m_CurDownload != null)
		{
			Patch patch = FindPatchByGamePath(m_CurDownload.FileName);
			if (patch != null && m_AlreadyDownList.Contains(patch) == false)
			{
				curAlreadySize = m_CurDownload.GetProcess() * patch.Size;
			}
		}

		return (alreadySize + curAlreadySize) ;
	}

	/// <summary>
	/// 开始下载AB 包
	/// </summary>
	/// <param name="callback"></param>
	/// <param name="allPatch"></param>
	/// <returns></returns>
	public IEnumerator StartDownloadAB(Action callback, List<Patch> allPatch = null) {
		m_AlreadyDownList.Clear();
		StartDownload = true;

        if (allPatch==null)
        {
			allPatch = m_DownloadList;
        }
        if (Directory.Exists(m_DownloadPath)==false)
        {
			Directory.CreateDirectory(m_DownloadPath);
        }

		List<DownloadAssetBundle> downloadAssetBundles = new List<DownloadAssetBundle>();
        foreach (Patch patch in allPatch)
        {
			downloadAssetBundles.Add(new DownloadAssetBundle(patch.Url,m_DownloadPath));
        }

        foreach (DownloadAssetBundle download in downloadAssetBundles)
        {
			m_CurDownload = download;
			yield return m_Mono.StartCoroutine(download.Download());
			Patch patch = FindPatchByGamePath(download.FileName);
            if (patch!=null)
            {
				m_AlreadyDownList.Add(patch);
            }
			download.Destroy();
        }

		// MD5 码校验; 如果校验没有通过，自动重新下载没有通过的文件，重复下载计数，达到一定次数后，反馈某文件下载失败
		VerifyMD5(downloadAssetBundles,callback);
	}

	void VerifyMD5(List<DownloadAssetBundle> downloadAssetBundles,Action callback) {
		List<Patch> downloadList = new List<Patch>();
        foreach (DownloadAssetBundle download in downloadAssetBundles)
        {
			string md5 = "";
            if (m_DownloadMD5Dict.TryGetValue(download.SaveFilePath,out md5))
            {
                if (MD5Manager.Instance.BuildFileMd5(download.SaveFilePath)!=md5)
                {
					Debug.Log(string.Format("此文件{0}MD5校验失败，即将重新下载",download.FileName));
					Patch patch = FindPatchByGamePath(download.FileName);
                    if (patch!=null)
                    {
						downloadList.Add(patch);
                    }
				}
            }
        }

		if (downloadList.Count <= 0)
		{
			m_DownloadMD5Dict.Clear();
			if (callback != null)
			{
				StartDownload = false;
				callback.Invoke();
			}

            if (LoadOver!=null)
            {
				LoadOver.Invoke();
            }
		}
		else {
			if (m_TryDownCount >= DOWNLOAD_COUNT)
			{
				string allName = "";
				StartDownload = false;
                foreach (Patch patch in downloadList)
                {
					allName += patch.Name + "、";
                }
				Debug.LogError($"资源重复下载{DOWNLOAD_COUNT}次MD5校验都失败，请检查资源：{allName}");
                if (ItemError!=null)
                {
					ItemError.Invoke(allName);
                }
			}
			else {
				m_TryDownCount++;
				m_DownloadMD5Dict.Clear();
                foreach (Patch patch in downloadList)
                {
					m_DownloadMD5Dict.Add(patch.Name,patch.Md5);
                }

				// 自动重新下载校验失败的文件
				m_Mono.StartCoroutine(StartDownloadAB(callback,downloadList));
			}
		}
	}



	/// <summary>
	/// 根据名字查找对象的热更 Patch
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	Patch FindPatchByGamePath(string name) {
		Patch patch = null;
		m_DownloadDIct.TryGetValue(name,out patch);

		return patch;
	}
}


public class FileTool {

	/// <summary>
	/// 创建文件
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="bytes"></param>
	public static void CreateFile(string filePath,byte[] bytes) {
        if (File.Exists(filePath))
        {
			File.Delete(filePath);
        }

		FileInfo file = new FileInfo(filePath);
		Stream stream = file.Create();
		stream.Write(bytes,0,bytes.Length);
		stream.Close();
		stream.Dispose();
	}
}
