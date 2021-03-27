using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using KEngine;
using KEngine.UI;
using KSFramework;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 作者：赵青青 (569032731@qq.com)
/// 时间：2021-3-19
/// 说明：从资源器下载资源，流程：
///         1.对比version
///         2.下载lua.zip、setting.zip、filelist差异文件(跳过md5相同的文件)
///         3.解压zip
///         4.更新version.txt，进入游戏
/// </summary>
public class DownloadManager : KSingleton<DownloadManager>
{
    private bool downloadFinish = false;
    public bool DownloadFinish
    {
        get { return downloadFinish;}
        set
        {
            if (downloadFinish != value)
            {
                downloadFinish = value;
                if (downloadFinish) UIModule.Instance.GetExistUI<LoadingPanel>()?.DisPlay(false);
            }
        }
    }
    public UpdateErrorType ErrorType { get; set; } = UpdateErrorType.None;
    private List<FileList> downloadFiles = new List<FileList>();
    private long downloadTotalSize;
    Dictionary<string, FileList> remoteVersion = new Dictionary<string, FileList>();
    Dictionary<string, FileList> localVersion = new Dictionary<string, FileList>();

    /// <summary>
    /// 标志位，是否需要更新lua.zip和setting.zip
    /// </summary>
    private bool needUnpackLua, needUnpackSetting;

    /// <summary>
    /// 本地将要保存的filelist.txt
    /// </summary>
    StringBuilder filelistBuilder = new StringBuilder();

    /// <summary>
    /// 本地将要保存的version.txt
    /// </summary>
    StringBuilder versionBuilder = new StringBuilder();

    /// <summary>
    /// version.txt中的filelist
    /// </summary>
    private FileList filelistVersion;

    void ClearData()
    {
        downloadFiles.Clear();
        downloadTotalSize = 0;
        remoteVersion.Clear();
        localVersion.Clear();
        filelistBuilder.Clear();
        versionBuilder.Clear();
        needUnpackLua = false;
        needUnpackSetting = false;
    }

    private bool CompareVersion(string verName, bool needDownload = true)
    {
        remoteVersion.TryGetValue(verName, out FileList remote_ver);
        localVersion.TryGetValue(verName, out FileList local_ver);
        bool isSame = remote_ver == local_ver;
        Log.LogToFile($"{verName}版本是否相同:{isSame} ,远程版本号:{remote_ver?.md5} ,本地版本号:{local_ver?.md5}");
        if (isSame == false)
        {
            var realPath = KResourceModule.AppDataPath + verName;
            if (File.Exists(realPath) && remote_ver != null && KTool.MD5_File(realPath) == remote_ver.md5)
            {
                Log.LogToFile($"文件存在:{verName}，且md5一致，跳过下载");
                return isSame;
            }

            if (!needDownload) return isSame;
            downloadFiles.Add(remote_ver);
            if (verName.Contains("lua")) needUnpackLua = true;
            if (verName.Contains("setting")) needUnpackSetting = true;
            downloadTotalSize += remote_ver.size;
        }

        return isSame;
    }

    public IEnumerator CheckDownload()
    {
        ClearData();
        var loadingPanel = UIModule.Instance.GetOrCreateUI<LoadingPanel>();
        loadingPanel.SetProgress(I18N.Get("download_check"));
        loadingPanel.DisPlay(true);
        string url = AppConfig.resUrl + AppConfig.VersionTxtName;
        Log.LogToFile($"读取远程version.txt:{url}");
        var loader = KWWWLoader.Load(url);
        while (!loader.IsCompleted)
        {
            yield return null;
        }

        if (!loader.IsError)
        {
            ParseText(loader.Www.text, remoteVersion);
            remoteVersion.TryGetValue("filelist.txt", out filelistVersion);
        }
        else
        {
            ErrorType = UpdateErrorType.RemoteVersionError;
            yield break;
        }

        url = KResourceModule.GetResourceFullPath(AppConfig.VersionTxtName, false);
        Log.LogToFile($"读取本地version.txt:{url}");
        loader = KWWWLoader.Load(url);
        while (!loader.IsCompleted)
        {
            yield return null;
        }

        if (!loader.IsError)
        {
            ParseText(loader.Www.text, localVersion);
        }
        else
        {
            ErrorType = UpdateErrorType.LocalVersionError;
            yield break;
        }

        loader.Dispose();
        loader = null;
        CompareVersion("lua.zip");
        CompareVersion("setting.zip");
        bool filelistSame = CompareVersion("filelist.txt", false);
        if (filelistSame == false)
        {
            //对比ab列表
            string remote_filelist = null;
            url = AppConfig.resUrl + AppConfig.FilelistName;
            loader = KWWWLoader.Load(url);
            while (!loader.IsCompleted)
            {
                yield return null;
            }

            if (!loader.IsError) remote_filelist = loader.Www.text;
            else ErrorType = UpdateErrorType.FilelistnError;
            url = KResourceModule.GetResourceFullPath(AppConfig.FilelistName, false);
            loader = KWWWLoader.Load(url);
            while (!loader.IsCompleted)
            {
                yield return null;
            }

            //开始对比两个filelist
            if (!loader.IsError) GetDownloadFromFilelist(loader.Www.text, remote_filelist);
            else ErrorType = UpdateErrorType.LocalFilelistnError;
        }

        if (downloadFiles.Count > 0)
        {
            var panel = UIModule.Instance.GetOrCreateUI<KUIMsgBox>();
            UIMsgBoxInfo info = new UIMsgBoxInfo().GetDefalut(I18N.Get("download_msg", KTool.FormatFileSize(downloadTotalSize)), strCancel: I18N.Get("common_skip"));
            info.OkCallback = () => { Game.Instance.StartCoroutine(StartUpdate()); };
            info.CancelCallback = IngoreDownload;
            panel.info = info;
            panel.DisPlay(true);
        }
        else
        {
            Log.LogToFile($"本次启动无资源更新，跳过下载");
            ClearData();
            DownloadFinish = true;
        }
    }

    void IngoreDownload()
    {
        DownloadFinish = true;
        ClearData();
    }
    
    IEnumerator DownloadItem(FileList file, string appDataPath)
    {
        using (var download = KHttpDownloader.Load(AppConfig.resUrl + file.path, appDataPath + "/" + file.path))
        {
            while (!download.IsFinished)
            {
                downloadTemp = (download.Progress * file.size);
                OnUpdateProgress();
                yield return null;
            }
            //TODO 下载出错，后台重试几次
            if (download.IsError) yield break;
            if (file.path.Contains("lua.zip") || file.path.Contains("setting.zip"))
            {
                versionBuilder.AppendLine(file.ToFilelistFormat());
            }
            else
            {
                filelistBuilder.AppendLine(file.ToFilelistFormat());
            }
            downloadNow = downloadNow + file.size;
        }
    }

    IEnumerator StartUpdate()
    {
        //更新下载进度
        var loadingPanel = UIModule.Instance.GetOrCreateUI<LoadingPanel>();
        loadingPanel.DisPlay(true);

        var appDataPath = KResourceModule.AppDataPath;
        //TODO 是否所有资源都下载成功
        var total = downloadFiles.Count;
        for (int i = 0; i < total; i++)
        {
            yield return Game.Instance.StartCoroutine(DownloadItem(downloadFiles[i], appDataPath));
        }

        Log.LogToFile(total > 0 ? "下载更新资源完成" : "本次无需下载新资源");
        try
        {
            //更新filelist
            if (filelistBuilder.Length > 0)
            {
                var dirName = Path.GetDirectoryName(appDataPath + AppConfig.FilelistName);
                if (Directory.Exists(dirName) == false) Directory.CreateDirectory(dirName);
                File.WriteAllText(appDataPath + AppConfig.FilelistName, filelistBuilder.ToString());
                Log.LogToFile("filelist更新完成");
            }
            else
            {
                Log.LogToFile("本次filelist无需更新");
            }

            //更新version
            if (versionBuilder.Length > 0)
            {
                if (filelistVersion == null) Log.LogError("更新version.txt中的filelist.txt version 失败，data is null");
                if (filelistVersion != null) versionBuilder.AppendLine(filelistVersion.ToFilelistFormat());
                var dirName = Path.GetDirectoryName(appDataPath + AppConfig.VersionTxtName);
                if (Directory.Exists(dirName) == false) Directory.CreateDirectory(dirName);
                File.WriteAllText(appDataPath + AppConfig.VersionTxtName, versionBuilder.ToString());
                Log.LogToFile("version更新完成");
            }
            else
            {
                Log.LogToFile("本次version无需更新");
            }
        }
        catch (Exception e)
        {
            Log.LogError($"保存filelist出错,{e.Message}");
            ErrorType = UpdateErrorType.DownloadError;
        }
        
        //解压可以放在多线程中
        loadingPanel.SetProgress(I18N.Get("download_unpackzip"));
        bool waitLua = true, waitSetting = true;

        if (needUnpackLua)
        {
            KAsync.Start().Thread(() => UnpackZip(appDataPath + "/lua.zip", appDataPath + "/Lua/", () => waitLua = false));
        }
        else
        {
            waitLua = false;
        }

        if (needUnpackSetting)
        {
            KAsync.Start().Thread(() => UnpackZip(appDataPath + "/setting.zip", appDataPath + "/Setting/", () => waitSetting = false));
        }
        else
        {
            waitSetting = false;
        }

        loadingPanel = UIModule.Instance.GetExistUI<LoadingPanel>();
        loadingPanel?.SetFixProgress(1 - zipPercent, 1.0f);
        WaitForSeconds wait = new WaitForSeconds(0.05f);
        while (waitLua || waitSetting)
        {
            loadingPanel?.UpdateFixedProgress();
            yield return wait;
        }
        
        DownloadFinish = true;
        if (needUnpackLua || needUnpackSetting) Log.LogToFile("解压更新资源完成");
        ClearData();
    }

    FileList ParseToFileList(string line)
    {
        if (string.IsNullOrEmpty(line)) return null;
        FileList file = null;
        var arr = line.Split(',');
        if (arr != null && arr.Length >= 3)
        {
            file = new FileList() {path = arr[0], md5 = arr[1], size = arr[2].ToInt32()};
        }

        return file;
    }

    /// <summary>
    /// 对比两个filelist，把不相同的添加到下载列表中
    /// </summary>
    /// <param name="local"></param>
    /// <param name="remote"></param>
    void GetDownloadFromFilelist(string local, string remote)
    {
        if (string.IsNullOrEmpty(local))
        {
            //TODO filelist异常
            Log.LogError("本地filelist为空");
            return;
        }

        if (string.IsNullOrEmpty(remote))
        {
            //TODO filelist异常
            Log.LogError("远程filelist为空");
            return;
        }

        Dictionary<string, FileList> localDict = new Dictionary<string, FileList>();
        using (StringReader reader = new StringReader(local))
        {
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;
                var file = ParseToFileList(line);
                if (file != null)
                {
                    localDict[file.path] = file;
                }
            }
        }

        string abDirName = AppConfig.StreamingBundlesFolderName + "/" + KResourceModule.GetBuildPlatformName() + "/";
        var savePath = KResourceModule.AppDataPath + abDirName;
        using (StringReader reader = new StringReader(remote))
        {
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;

                var file = ParseToFileList(line);
                if (file != null)
                {
                    var realPath = savePath + file.path;
                    if (File.Exists(realPath) && KTool.MD5_File(realPath) == file.md5)
                    {
                        filelistBuilder.AppendLine(file.ToFilelistFormat());
                        Log.LogToFile($"文件存在:{file.path}，且md5一致，跳过下载");
                        continue;
                    }

                    if (localDict.TryGetValue(file.path, out FileList exist))
                    {
                        if (exist.md5 != file.md5 || exist.size != file.size)
                        {
                            var newFile = new FileList() {path = abDirName + file.path, md5 = file.md5, size = file.size};
                            downloadFiles.Add(newFile);
                            downloadTotalSize += file.size;
                        }
                        else
                        {
                            filelistBuilder.AppendLine(file.ToFilelistFormat());
                        }
                    }
                    else
                    {
                        var newFile = new FileList() {path = abDirName + file.path, md5 = file.md5, size = file.size};
                        downloadFiles.Add(newFile);
                        downloadTotalSize += file.size;
                    }
                }
            }
        }
    }

    public void ParseText(string text, Dictionary<string, FileList> dict)
    {
        if (dict == null) dict = new Dictionary<string, FileList>();
        if (!string.IsNullOrEmpty(text))
        {
            using (StringReader reader = new StringReader(text))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;
                    if (TextParser.IsCommentLine(line)) continue;
                    var file = ParseToFileList(line);
                    if (file != null)
                    {
                        dict[file.path] = file;
                    }
                    else
                    {
                        Log.LogError($"这一行的格式错误:{line}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 解压zip包
    /// 注意：使用python压缩是不带最上层目录，保存路径手动补充最上层目录
    /// </summary>
    /// <param name="zipFile">zip文件</param>
    /// <param name="unzipSavePath">解压后的目录</param>
    /// <param name="callback">解压完成的回调</param>
    void UnpackZip(string zipFile, string unzipSavePath, Action callback)
    {
        if (File.Exists(zipFile) == false)
        {
            Log.LogError($"解压失败，文件不存在：{zipFile}");
            callback?.Invoke();
            return;
        }

        using (var s = new ZipInputStream(File.OpenRead(zipFile)))
        {
            ZipEntry theEntry;
            try
            {
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if (theEntry.IsDirectory) continue;

                    var cleanName = theEntry.Name.Replace("\\", "/");
                    string directorName = Path.Combine(unzipSavePath, Path.GetDirectoryName(cleanName));
                    var fileName = Path.GetFileName(cleanName);
                    // 其它解压
                    string fullFileName = Path.Combine(directorName, fileName);

                    if (!Directory.Exists(directorName))
                    {
                        Directory.CreateDirectory(directorName);
                    }

                    if (!String.IsNullOrEmpty(fullFileName))
                    {
                        using (FileStream streamWriter = File.Create(fullFileName))
                        {
                            byte[] data = new byte[s.Length];
                            s.Read(data, 0, data.Length);
                            streamWriter.Write(data, 0, data.Length);
#if UNITY_EDITOR
                            //Log.LogConsole_MultiThread("解压文件: {0}, 解压的大小: {1}KB", cleanName, data.Length / 1024f);
#endif
                        }
                    }
                }
                File.Delete(zipFile);
            }
            catch (Exception e)
            {
                Log.LogConsole_MultiThread("解压文件出错，Error:{0}", e.Message);
            }
        }

        callback?.Invoke();
    }

    #region 下载进度和网速
    //已下载内容， 前1秒下载量
    private long downloadNow,downloadLastSecond;
    private float downloadTemp,remainTime,last_time,totalProgress,lastTotalProgress,time_per;
    /// <summary>
    /// NOTE 解压也放在同一进度条，固定在一个百分比内
    /// </summary>
    private float zipPercent = 0.04f;
    private long speed = 0; 
    
    /// <summary>
    /// 每秒计算下载速度，每帧更新进度条
    /// </summary>
    void OnUpdateProgress()
    {
        long downloadFinish = (long) (downloadTemp + downloadNow);
        totalProgress = KTool.GetPercent(downloadFinish, downloadTotalSize) - zipPercent;
        totalProgress = Mathf.Max(totalProgress, 0.0f);

        if (Time.time > last_time)
        {
            last_time = Time.time + 1.0f;
            time_per = 0;
            // 下载速度 = 前1秒下载量 / 1s ,总下载=下载中 + 已下载
            speed = downloadFinish - downloadLastSecond;
            speed = (long) Mathf.Max(speed, 0);
            remainTime = KTool.GetPercent(downloadTotalSize - downloadFinish, speed);
            downloadLastSecond = downloadFinish;
            lastTotalProgress = totalProgress;

        }
        var panel = UIModule.Instance.GetExistUI<LoadingPanel>();
        if (panel != null)
        {
            string strSize = $"{KTool.FormatFileSize(downloadFinish)}/{KTool.FormatFileSize(downloadTotalSize)}";
            string strSpeed = (speed / 1024f).ToString("0.##");
            time_per += Time.deltaTime;
            var progress = Mathf.Lerp(lastTotalProgress, totalProgress, time_per);
            panel.SetProgress(I18N.Get("download_speed", strSize, strSpeed, KTool.HumanizeTimeString((int) remainTime)), progress);
        }
    }

    #endregion
}

public class FileList
{
    public string path;
    public string md5;
    public int size;

    /// <summary>
    /// 格式化为filelist的格式
    /// </summary>
    public string ToFilelistFormat()
    {
        return $"{path},{md5},{size}";
    }

    public static bool operator ==(FileList lhs, FileList rhs)
    {
        if (!object.ReferenceEquals(lhs, null) && !object.ReferenceEquals(rhs, null))
        {
            return lhs.md5 == rhs.md5;
        }

        if (object.ReferenceEquals(lhs, null) && object.ReferenceEquals(rhs, null))
        {
            return true;
        }

        return false;
    }

    public static bool operator !=(FileList lhs, FileList rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public enum UpdateErrorType
{
    None,
    RemoteVersionError,
    LocalVersionError,
    FilelistnError,
    LocalFilelistnError,
    DownloadError,
    ZipError
}