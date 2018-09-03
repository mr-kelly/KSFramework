#region Copyright(c) Kingsoft Xishanju

// Company: Kingsoft Xishanju
// Filename: KSymbolLinkHelper.cs
// Date:     2015/11/07
// Author:   Kelly / chenpeilin1
// Email: chenpeilin1@kingsoft.com / 23110388@qq.com

#endregion

using System;
using System.IO;
using KUnityEditorTools;
using UnityEngine;

namespace KUnityEditorTools
{
    /// <summary>
    /// 硬链接目录工具。。。支持win+mac, 需要win 7以上才有mklink命令
    /// </summary>
    public class KSymbolLinkHelper
    {
        /// <summary>
        /// 删除硬链接目录
        /// </summary>
        /// <param name="linkPath"></param>
        public static void DeleteLink(string linkPath)
        {
            var os = Environment.OSVersion;
            if (os.ToString().Contains("Windows"))
            {
                KEditorUtils.ExecuteCommand(String.Format("rmdir \"{0}\"", linkPath));
            }
            else if (os.ToString().Contains("Unix"))
            {
                KEditorUtils.ExecuteCommand(String.Format("rm -Rf \"{0}\"", linkPath));
            }
            else
            {
                Debug.LogError(String.Format("[SymbolLinkFolder]Error on OS: {0}", os.ToString()));
            }
        }

        public static void SymbolLinkFolder(string srcFolderPath, string targetPath)
        {
            var os = Environment.OSVersion;
            if (os.ToString().Contains("Windows"))
            {
                KEditorUtils.ExecuteCommand(String.Format("mklink /J \"{0}\" \"{1}\"", targetPath, srcFolderPath));
            }
            else if (os.ToString().Contains("Unix"))
            {
                var fullPath = Path.GetFullPath(targetPath);
                if (fullPath.EndsWith("/"))
                {
                    fullPath = fullPath.Substring(0, fullPath.Length - 1);
                    fullPath = Path.GetDirectoryName(fullPath);
                }
                KEditorUtils.ExecuteCommand(String.Format("ln -s {0} {1}", Path.GetFullPath(srcFolderPath), fullPath));
            }
            else
            {
                Debug.LogError(String.Format("[SymbolLinkFolder]Error on OS: {0}", os.ToString()));
            }
        }

        /// <summary>
        /// 删除指定目录所有硬链接
        /// </summary>
        /// <param name="assetBundlesLinkPath"></param>
        public static void DeleteAllLinks(string assetBundlesLinkPath)
        {
            if (Directory.Exists(assetBundlesLinkPath))
            {
                foreach (var dirPath in Directory.GetDirectories(assetBundlesLinkPath))
                {
                    DeleteLink(dirPath);
                }
            }

        }
    }
}