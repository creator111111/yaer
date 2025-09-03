using System;

namespace GameFramework.CoreExtend.Systems.Logger
{
    public class FrameworkException: Exception
    {
        /// <summary>初始化游戏框架异常类的新实例。</summary>
        public FrameworkException()
        {
        }

        /// <summary>使用指定错误消息初始化游戏框架异常类的新实例。</summary>
        /// <param name="message">描述错误的消息。</param>
        public FrameworkException(string message)
            : base(message)
        {
        }

        /// <summary>使用指定错误消息和对作为此异常原因的内部异常的引用来初始化游戏框架异常类的新实例。</summary>
        /// <param name="message">解释异常原因的错误消息。</param>
        /// <param name="innerException">导致当前异常的异常。如果 innerException 参数不为空引用，则在处理内部异常的 catch 块中引发当前异常。</param>
        public FrameworkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        
    }
}