using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TEngine
{
    /// <summary>
    /// 跨线程同步上下文 - 用于将其他线程的回调同步到主线程执行。
    /// 适用场景：Unity中需要主线程执行的网络回调、异步任务结果处理等。
    /// 实现原理：通过线程安全队列存储待执行回调，由主线程定期驱动队列消费。
    /// 注意：需在Unity中设置 Scripting Runtime Version 为 .NET 4.x Equivalent。
    /// </summary>
    internal sealed class ThreadSyncContext : SynchronizationContext
    {
        // 线程安全的先进先出队列（并发队列）。
        // 用于安全地存储从其他线程投递过来的回调方法。
        private readonly ConcurrentQueue<Action> _safeQueue = new ConcurrentQueue<Action>();

        /// <summary>
        /// 主线程更新方法（每帧调用）。
        /// 功能：执行所有已入队的回调方法。
        /// </summary>
        public void Update()
        {
            // 循环直到清空当前队列。
            while (true)
            {
                // 尝试从队列头部取出回调
                if (_safeQueue.TryDequeue(out Action action) == false)
                    return; // 队列为空时退出循环。
                    
                // 执行回调（在调用线程的上下文中执行）。
                action.Invoke();
            }
        }

        /// <summary>
        /// 跨线程投递回调方法（线程安全）。
        /// 重写自 SynchronizationContext 的核心方法。
        /// </summary>
        /// <param name="callback">要执行的回调方法。</param>
        /// <param name="state">回调参数对象。</param>
        public override void Post(SendOrPostCallback callback, object state)
        {
            // 将回调封装为无参数的Action。
            Action action = new Action(() => { callback(state); });
            
            // 将回调任务加入队列（线程安全操作）。
            _safeQueue.Enqueue(action);
        }
    }
}