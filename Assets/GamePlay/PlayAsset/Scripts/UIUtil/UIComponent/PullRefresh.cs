using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Watermelon;
using DG.Tweening;


namespace UIUtil.UI
{
    [Serializable]
    public struct PullRefreshSetting
    {
        public bool enable;
        public float height;    //触发高度
        public float elasticHeight; //回弹目标高度(边界的距离)
        public Func<bool> triggerHandler;
        public GameObject tip;
    }

    [Serializable]
    public struct LimitRefreshSetting
    {
        public bool enable;
        public float height;    //触发高度
        public float interval;  //间隔时间
        public Func<bool> triggerHandler;
    }

    public class PullRefresh : MonoBehaviour
    {
        public enum Status
        {
            None,
            TriggerPullDown,
            TriggerPullUp,
            WaitPullDownRefresh,
            WaitPullUpRefresh
            //Finish,
        }

        [SerializeField]
        private PullRefreshSetting pullDownSetting;

        [SerializeField]
        private PullRefreshSetting pullUpSetting;

        [SerializeField]
        private LimitRefreshSetting tailRefreshSetting;

        private Status m_status;
        private ScrollView scrollRect;
        private Tween m_tweener;
        private bool m_isCoolingDown_tailRefresh;
        private Coroutine m_coroutine_tailRefresh;
        private bool isInit = false;

        public bool IsWaitPullDownRefresh
        {
            get { return m_status == Status.WaitPullDownRefresh; }
        }

        public bool IsWaitPullUpRefresh
        {
            get { return m_status == Status.WaitPullUpRefresh; }
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            if (!isInit)
            {
                isInit = true;

                scrollRect = this.GetComponent<ScrollView>();
                __Reset();
                scrollRect.OnTouchScrollEnd += __OnEndDrag;
            }
        }

        public void SetPullDownAction(Func<bool> action)
        {
            pullDownSetting.triggerHandler = action;
        }

        public void SetPullUpAction(Func<bool> action)
        {
            pullUpSetting.triggerHandler = action;
        }

        public void SetTailRefreshAction(Func<bool> action)
        {
            tailRefreshSetting.triggerHandler = action;
        }

        public PullRefreshSetting GetPullDownSetting()
        {
            return pullDownSetting;
        }

        /// <summary>
        /// 正常完成刷新
        /// </summary>
        public void FinishRefresh()
        {
            if (scrollRect == null)
            {
                return;
            }
            scrollRect.enabled = true;
            __Reset();
        }

        /// <summary>
        /// 强制完成
        /// </summary>
        public void ForceFinish()
        {
            if (scrollRect == null)
            {
                return;
            }
            scrollRect.enabled = true;
            __Reset();

            Vector2 vec2 = scrollRect.content.anchoredPosition;
            if (vec2.y < 0)
            {
                vec2.y = 0;
                scrollRect.content.anchoredPosition = vec2;
            }
        }

        private void Update()
        {
            if (m_status == Status.TriggerPullDown)
            {
                __TriggerPullDownRefresh();
            }
            else if (m_status == Status.TriggerPullUp)
            {
                __TriggerPullUpRefresh();
            }
        }

        private void __SetGameObjectActive(GameObject go, bool active)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }

        private void __Reset()
        {
            m_status = Status.None;

            __SetGameObjectActive(pullDownSetting.tip, false);
            __SetGameObjectActive(pullUpSetting.tip, false);
            __StopTailRefreshCorotine();

            if (m_tweener != null)
            {
                m_tweener.Kill();
                m_tweener = null;
            }
        }

        private void __OnEndDrag(Vector2 preTouchPos, Vector2 curTouchPos, PointerEventData eventData)
        {
            if (eventData.position.y - eventData.pressPosition.y < 0)
            {
                if (__CheckPullDownRefresh())
                {
                    return;
                }
            }

            if (eventData.position.y - eventData.pressPosition.y > 0)
            {
                if (__CheckPullUpRefresh())
                {
                    return;
                }

                if (__CheckTailRefresh())
                {
                    return;
                }
            }
        }

        private void __TriggerPullDownRefresh()
        {
            if (!__OnTriggerPullDownRefresh())
            {
                m_status = Status.None;
                return;
            }
            m_status = Status.WaitPullDownRefresh;
            scrollRect.enabled = false;
            m_tweener = scrollRect.content.DOAnchorPosY(0 - pullDownSetting.elasticHeight, 0.2f);
            // var anPos = scrollRect.content.anchoredPosition;
            // anPos.y = 0 - pullDownSetting.elasticHeight;
            // m_tweener = scrollRect.content.DOAnchorPos(anPos, 0.2f);
            __SetGameObjectActive(pullDownSetting.tip.gameObject, true);
        }

        private void __TriggerPullUpRefresh()
        {
            if (!__OnTriggerPullUpRefresh())
            {
                m_status = Status.None;
                return;
            }
            m_status = Status.WaitPullUpRefresh;
            scrollRect.enabled = false;
            float y = 0 - scrollRect.viewport.rect.height + pullUpSetting.elasticHeight;
            y += scrollRect.content.rect.height;

            var anPos = scrollRect.content.anchoredPosition;
            anPos.y = y;

            m_tweener = scrollRect.content.DOAnchorPosY(y, 0.2f);
            // m_tweener = scrollRect.content.DOAnchorPos(anPos, 0.2f);
            __SetGameObjectActive(pullUpSetting.tip.gameObject, true);
        }

        private bool __OnTriggerPullDownRefresh()
        {
            if (pullDownSetting.triggerHandler != null)
            {
                return pullDownSetting.triggerHandler();
            }
            return false;
        }

        private bool __OnTriggerPullUpRefresh()
        {
            if (pullUpSetting.triggerHandler != null)
            {
                return pullUpSetting.triggerHandler();
            }
            return false;
        }

        //检测下拉刷新
        private bool __CheckPullDownRefresh()
        {
            if (!pullDownSetting.enable)
            {
                return false;
            }

            Vector2 pos = scrollRect.content.anchoredPosition;
            if (pos.y <= 0 - pullDownSetting.height)
            {
                m_status = Status.TriggerPullDown;
                return true;
            }
            return false;
        }

        //检测上拉刷新
        private bool __CheckPullUpRefresh()
        {
            if (!pullUpSetting.enable)
            {
                return false;
            }

            Vector2 pos = scrollRect.content.anchoredPosition;
            pos.y -= scrollRect.content.rect.height;
            float bottom = 0 - scrollRect.viewport.rect.height;
            if (pos.y >= bottom + pullUpSetting.height)
            {
                m_status = Status.TriggerPullUp;
                return true;
            }
            return false;
        }

        //检测尾部刷新
        private bool __CheckTailRefresh()
        {
            if (!tailRefreshSetting.enable)
            {
                return false;
            }

            if (m_isCoolingDown_tailRefresh)
            {
                return false;
            }

            Vector2 pos = scrollRect.content.anchoredPosition;
            pos.y -= scrollRect.content.rect.height;
            float bottom = 0 - scrollRect.viewport.rect.height;
            if (pos.y + tailRefreshSetting.height >= bottom)
            {
                bool ret = false;
                if (tailRefreshSetting.triggerHandler != null)
                {
                    ret = tailRefreshSetting.triggerHandler();
                }
                if (ret)
                {
                    __StartTailRefreshCorotine();
                }
                return ret;
            }

            return false;
        }

        private void __StartTailRefreshCorotine()
        {
            if (tailRefreshSetting.interval > 0)
            {
                m_isCoolingDown_tailRefresh = true;
                m_coroutine_tailRefresh = StartCoroutine(__DoCoolDownTailRefresh());
            }
        }

        IEnumerator __DoCoolDownTailRefresh()
        {
            yield return new WaitForSeconds(tailRefreshSetting.interval);
            m_coroutine_tailRefresh = null;
            m_isCoolingDown_tailRefresh = false;
        }

        private void __StopTailRefreshCorotine()
        {
            if (m_coroutine_tailRefresh != null)
            {
                StopCoroutine(m_coroutine_tailRefresh);
                m_coroutine_tailRefresh = null;
                m_isCoolingDown_tailRefresh = false;
            }
        }
    }
}