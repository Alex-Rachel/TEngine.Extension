using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace TEngine
{
    public class TweenManager
    {
        private static TweenManager _instance = null;

        public static TweenManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TweenManager();
                }
                return _instance;
            }
        }

        public TweenManager()
        {
            Utility.Unity.AddUpdateListener(Update);
        }
        
        private const float CheckInterval = 60f;

        private float m_CheckInterval = CheckInterval;

        private void Update()
        {
            m_CheckInterval -= Time.deltaTime;
            // 如果时间间隔小于等于0
            if (m_CheckInterval <= 0)
            {
                // 释放未使用的Tween
                PrimeTweenHelperImp.ReleaseUnusedTween();
                // 重置时间间隔
                m_CheckInterval = CheckInterval;
            }
        }
    }

    /// <summary>
    /// Tween实现类。
    /// <remarks>需要PrimeTween新增 public long Id => id; 
    /// </remarks>
    /// </summary>
    public class PrimeTweenHelperImp : Utility.Tween.ITweenHelper
    {
        // 缓存Tween的字典，键为Tween的ID，值为Tween对象
        private static readonly Dictionary<long, Tween> m_cacheTweenDic = new Dictionary<long, Tween>();

        // 临时列表，用于存储需要释放的Tween的ID
        private static readonly List<long> m_tempList = new List<long>();

        // 缓存Sequence的字典，键为Sequence的ID，值为Sequence对象
        private static readonly Dictionary<long, Sequence> m_cacheSequenceDic = new Dictionary<long, Sequence>();

        // 临时列表，用于存储需要释放的Sequence的ID
        private static readonly List<long> m_tempSequenceList = new List<long>();

        // Tween的最大容量
        private const int TweenCapacity = 128;

        /// <summary>
        /// 构造函数，初始化Tween配置。
        /// </summary>
        public PrimeTweenHelperImp()
        {
            PrimeTweenConfig.SetTweensCapacity(TweenCapacity);
            PrimeTweenConfig.warnEndValueEqualsCurrent = false;
            var t = TweenManager.Instance;
            Debug.Log($"Init PrimeTweenConfig.");
        }

        /// <summary>
        /// 将TEngine的Ease枚举转换为PrimeTween的Ease枚举。
        /// </summary>
        /// <param name="ease">TEngine的Ease枚举值。</param>
        /// <returns>对应的PrimeTween的Ease枚举值。</returns>
        private PrimeTween.Ease GetEase(TEngine.Ease ease)
        {
            return (PrimeTween.Ease)(int)ease;
        }

        /// <summary>
        /// 将TEngine的CycleMode枚举转换为PrimeTween的CycleMode枚举。
        /// </summary>
        /// <param name="cycleMode">TEngine的CycleMode枚举值。</param>
        /// <returns>对应的PrimeTween的CycleType枚举值。</returns>
        private PrimeTween.CycleMode GetCycleMode(TEngine.CycleMode cycleMode)
        {
            return (PrimeTween.CycleMode)(int)cycleMode;
        }

        /// <summary>
        /// 缓存Tween对象。
        /// </summary>
        /// <param name="tween">需要缓存的Tween对象。</param>
        private void CacheTween(Tween tween)
        {
            if (tween.Id <= 0)
            {
                return;
            }

            m_cacheTweenDic.TryAdd(tween.Id, tween);
        }

        /// <summary>
        /// 根据Tween的ID获取Tween对象。
        /// </summary>
        /// <param name="tweenId">Tween的ID。</param>
        /// <returns>对应的Tween对象，如果不存在则返回null。</returns>
        public static Tween GetTween(long tweenId)
        {
            return m_cacheTweenDic.GetValueOrDefault(tweenId);
        }

        /// <summary>
        /// 根据Sequence的ID获取Sequence对象。
        /// </summary>
        /// <param name="Id">Sequence的ID。</param>
        /// <returns>对应的Sequence对象，如果不存在则返回null。</returns>
        public static Sequence GetSequence(long Id)
        {
            return m_cacheSequenceDic.GetValueOrDefault(Id);
        }

        /// <summary>
        /// 判断指定对象是否正在执行Tween动画。
        /// </summary>
        /// <param name="onTarget">需要检查的对象。</param>
        /// <returns>如果正在执行Tween动画则返回true，否则返回false。</returns>
        public bool IsTweening(object onTarget)
        {
            return GetTweenCount(onTarget) > 0;
        }

        /// <summary>
        /// 获取指定对象正在执行的Tween动画数量。
        /// </summary>
        /// <param name="onTarget">需要检查的对象。</param>
        /// <returns>正在执行的Tween动画数量。</returns>
        public int GetTweenCount(object onTarget)
        {
            return Tween.GetTweensCount(onTarget);
        }

        /// <summary>
        /// 判断指定ID的Tween是否还存活。
        /// </summary>
        /// <param name="tweenId">Tween的ID。</param>
        /// <returns>如果Tween还存活则返回true，否则返回false。</returns>
        public bool IsAlive(long tweenId)
        {
            if (m_cacheTweenDic.TryGetValue(tweenId, out var tween))
            {
                return tween.isAlive;
            }

            return false;
        }

        /// <summary>
        /// 释放未使用的Tween对象。
        /// </summary>
        public static void ReleaseUnusedTween()
        {
            m_tempList.Clear();
            using var iter = m_cacheTweenDic.GetEnumerator();
            while (iter.MoveNext())
            {
                var tween = iter.Current.Value;
                var tempId = iter.Current.Key;
                // 如果Tween自己的Id为0，且缓存的Id不等于0；
                if (tween.Id == 0 && tempId != 0)
                {
                    m_tempList.Add(tempId);
                }
                else
                {
                    if (!tween.isAlive)
                    {
                        m_tempList.Add(tween.Id);
                    }
                }
            }

            var removeCnt = m_tempList.Count;
            for (int i = 0; i < removeCnt; i++)
            {
                m_cacheTweenDic.Remove(m_tempList[i]);
            }

            m_tempList.Clear();
        }

        public void StopTween(long tweenId)
        {
            if (m_cacheTweenDic.TryGetValue(tweenId, out var tween))
            {
                tween.Stop();
            }
        }

        public void CompleteTween(long tweenId)
        {
            if (m_cacheTweenDic.TryGetValue(tweenId, out var tween))
            {
                tween.Complete();
            }
        }

        public void Stop(long tweenId)
        {
            if (m_cacheTweenDic.TryGetValue(tweenId, out var tween))
            {
                tween.Stop();
            }
        }

        public void Complete(long tweenId)
        {
            if (m_cacheTweenDic.TryGetValue(tweenId, out var tween))
            {
                tween.Complete();
            }
        }

        public int StopAll(object onTarget = null)
        {
            return Tween.StopAll(onTarget);
        }

        public int CompleteAll(object onTarget = null)
        {
            return Tween.CompleteAll(onTarget);
        }

        public void OnComplete(long tweenId, Action onComplete)
        {
            if (m_cacheTweenDic.TryGetValue(tweenId, out var tween))
            {
                if (tween.isAlive)
                {
                    tween.OnComplete(onComplete: onComplete);
                }
            }
        }

        public long Delay(float duration, Action onComplete = null, bool useUnscaledTime = false, bool warnIfTargetDestroyed = true)
        {
            Tween tween = Tween.Delay(duration, onComplete, useUnscaledTime, warnIfTargetDestroyed);
            CacheTween(tween);
            return tween.Id;
        }

        public long Delay(object target, float duration, Action onComplete = null, bool useUnscaledTime = false, bool warnIfTargetDestroyed = true)
        {
            Tween tween = Tween.Delay(target, duration, onComplete, useUnscaledTime, warnIfTargetDestroyed);
            CacheTween(tween);
            return tween.Id;
        }

        public long LocalRotation(Transform target, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.LocalRotation(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long LocalRotation(Transform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.LocalRotation(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Scale(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.Scale(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Scale(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.Scale(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Rotation(Transform target, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.Rotation(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Rotation(Transform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.Rotation(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Position(Transform target, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.Position(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Position(Transform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.Position(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long PositionX(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.PositionX(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long PositionX(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.PositionX(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long PositionY(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.PositionY(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long PositionY(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.PositionY(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long PositionZ(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.PositionZ(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long PositionZ(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.PositionZ(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long LocalPosition(Transform target, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.LocalPosition(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long LocalPosition(Transform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.LocalPosition(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long LocalPositionX(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.LocalPositionX(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long LocalPositionX(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.LocalPositionX(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long LocalPositionY(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.LocalPositionY(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long LocalPositionY(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.LocalPositionY(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long LocalPositionZ(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.LocalPositionZ(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long LocalPositionZ(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.LocalPositionZ(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Rotation(Transform target, Quaternion endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.Rotation(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Rotation(Transform target, Quaternion startValue, Quaternion endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.Rotation(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long LocalRotation(Transform target, Quaternion endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.LocalRotation(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long LocalRotation(Transform target, Quaternion startValue, Quaternion endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.LocalRotation(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Scale(Transform target, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.Scale(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Scale(Transform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.Scale(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long ScaleX(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.ScaleX(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long ScaleX(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.ScaleX(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long ScaleY(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.ScaleY(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long ScaleY(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.ScaleY(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long ScaleZ(Transform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.ScaleZ(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long ScaleZ(Transform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.ScaleZ(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Color(SpriteRenderer target, Color endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.Color(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Color(SpriteRenderer target, Color startValue, Color endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.Color(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Alpha(SpriteRenderer target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.Alpha(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Alpha(SpriteRenderer target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.Alpha(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UISliderValue(Slider target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.UISliderValue(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UISliderValue(Slider target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UISliderValue(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UINormalizedPosition(ScrollRect target, Vector2 endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UINormalizedPosition(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UINormalizedPosition(ScrollRect target, Vector2 startValue, Vector2 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UINormalizedPosition(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIHorizontalNormalizedPosition(ScrollRect target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIHorizontalNormalizedPosition(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIHorizontalNormalizedPosition(ScrollRect target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIHorizontalNormalizedPosition(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay,
                useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIAnchoredPosition(RectTransform target, Vector2 endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIAnchoredPosition(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIAnchoredPosition(RectTransform target, Vector2 startValue, Vector2 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIAnchoredPosition(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIAnchoredPositionX(RectTransform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIAnchoredPositionX(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIAnchoredPositionX(RectTransform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIAnchoredPositionX(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIAnchoredPositionY(RectTransform target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIAnchoredPositionY(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIAnchoredPositionY(RectTransform target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIAnchoredPositionY(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIVerticalNormalizedPosition(ScrollRect target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIVerticalNormalizedPosition(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIVerticalNormalizedPosition(ScrollRect target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIVerticalNormalizedPosition(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay,
                useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIAnchoredPosition3D(RectTransform target, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIAnchoredPosition3D(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIAnchoredPosition3D(RectTransform target, Vector3 startValue, Vector3 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIAnchoredPosition3D(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UISizeDelta(RectTransform target, Vector2 endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UISizeDelta(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UISizeDelta(RectTransform target, Vector2 startValue, Vector2 endValue, float duration, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UISizeDelta(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Color(Graphic target, Color endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.Color(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Color(Graphic target, Color startValue, Color endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.Color(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Alpha(CanvasGroup target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.Alpha(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Alpha(CanvasGroup target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.Alpha(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Alpha(Graphic target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0,
            float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.Alpha(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Alpha(Graphic target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.Alpha(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIFillAmount(Image target, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIFillAmount(target, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long UIFillAmount(Image target, float startValue, float endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0,
            float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.UIFillAmount(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long MoveBezierPath(Transform target, Vector3[] path, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0,
            bool useUnscaledTime = false)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (path.Length < 2)
            {
                throw new ArgumentException("Path must have at least 2 points.");
            }

            Tween tween = Tween.Custom<Transform>(target, 0f, 1f, duration, (transform, t) =>
                {
                    // 计算贝塞尔曲线上的点  
                    Vector3 position = CalculateBezierPoint(t, path);
                    transform.position = position;

                    if (Mathf.Approximately(t, 1f))
                    {
                        transform.position = path[^1];
                    }
                }, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay,
                useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        #region BezierPathHelper

        /// <summary>
        /// N 阶贝塞尔曲线的计算。
        /// </summary>
        /// <param name="t"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        private static Vector3 CalculateBezierPoint(float t, Vector3[] points)
        {
            int n = points.Length - 1;
            Vector3 point = Vector3.zero;

            // 计算贝塞尔点  
            for (int i = 0; i <= n; i++)
            {
                float coefficient = BinomialCoefficient(n, i) * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i);
                point += coefficient * points[i];
            }

            return point;
        }

        /// <summary>
        /// 计算二项式系数。
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        private static int BinomialCoefficient(int n, int k)
        {
            if (k < 0 || k > n)
            {
                return 0;
            }

            if (k == 0 || k == n)
            {
                return 1;
            }

            int result = 1;
            for (int i = 0; i < k; i++)
            {
                result *= (n - i);
                result /= (i + 1);
            }

            return result;
        }

        #endregion

        public long Custom<T>(T target, Vector3 startValue, Vector3 endValue, float duration, Action<T, Vector3> onValueChange, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) where T : class
        {
            Tween tween = Tween.Custom<T>(target, startValue, endValue, duration, onValueChange, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay,
                useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Custom<T>(T target, uint startValue, uint endValue, float duration, Action<T, uint> onValueChange, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) where T : class
        {
            throw new NotImplementedException();
        }

        public long Custom<T>(T target, int startValue, int endValue, float duration, Action<T, int> onValueChange, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) where T : class
        {
            Tween tween = Tween.Custom<T>(target, startValue, endValue, duration, (arg1, f) => { onValueChange?.Invoke(arg1, (int)f); }, GetEase(ease), cycles,
                GetCycleMode(cycleMode), startDelay, endDelay,
                useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Custom<T>(T target, long startValue, long endValue, float duration, Action<T, long> onValueChange, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) where T : class
        {
            Tween tween = Tween.Custom<T>(target, startValue, endValue, duration, (arg1, f) => { onValueChange?.Invoke(arg1, (long)f); }, GetEase(ease), cycles,
                GetCycleMode(cycleMode), startDelay, endDelay,
                useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }

        public long Custom<T>(T target, float startValue, float endValue, float duration, Action<T, float> onValueChange, Ease ease = Ease.Default, int cycles = 1,
            CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false) where T : class
        {
            Tween tween = Tween.Custom<T>(target, startValue, endValue, duration, onValueChange, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay,
                useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }
        public long MaterialColor(Material target, Color startValue, Color endValue, float duration, Ease ease = Ease.Default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart,
            float startDelay = 0, float endDelay = 0, bool useUnscaledTime = false)
        {
            Tween tween = Tween.MaterialColor(target, startValue, endValue, duration, GetEase(ease), cycles, GetCycleMode(cycleMode), startDelay, endDelay, useUnscaledTime);
            CacheTween(tween);
            return tween.Id;
        }
    }
}