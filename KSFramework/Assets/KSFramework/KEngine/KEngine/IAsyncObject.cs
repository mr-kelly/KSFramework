using UnityEngine;
using System.Collections;

namespace KEngine
{
    /// <summary>
    /// 用于协程，线程，结果调度类
    /// </summary>
    public interface IAsyncObject
    {
        /// <summary>
        /// 最终加载结果的资源
        /// </summary>
        object AsyncResult { get; }

        /// <summary>
        /// 是否已经完成，它的存在令Loader可以用于协程StartCoroutine
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// 类似WWW, IsFinished再判断是否有错误对吧
        /// </summary>
        bool IsError { get; }

        /// <summary>
        /// 过程信息
        /// </summary>
        string AsyncMessage { get; }

        /// <summary>
        /// 是否成功
        /// </summary>
        bool IsSuccess { get; }
    }


}