#if UNITY_5_3_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author：qingqing.zhao (569032731@qq.com)
/// Date：2020/12/1
/// Desc：用来接受UGUI的事件，代替使用空的Image或Text来接受事件
/// </summary>
public class EmptyImage : Image
{
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        toFill.Clear();
    }
}

#endif