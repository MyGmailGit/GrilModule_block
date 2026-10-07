/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;

namespace FancyScrollView
{
    class Cell : FancyCell<ItemData, Context>
    {
        [SerializeField] Animator animator = default;
        [SerializeField] TextMeshProUGUI named = default;
        [SerializeField] TextMeshProUGUI titleTxt = default;
        [SerializeField] RawImage rawIcon;
        [SerializeField] Image laodingImg;
        [SerializeField] Image adImg;
        [SerializeField] Image vipImg;


        private int _currentRequestId = -1;
        private string _currentImageId;
        /// <summary>
        /// 
        /// </summary>
        private bool isFirst = false;

        static class AnimatorHash
        {
            public static readonly int Scroll = Animator.StringToHash("scroll");
        }

        public override void UpdateContent(ItemData itemData)
        {
            named.text = "";
            titleTxt.text = AB_Ctrl.Instance.GetNameWithMainId(itemData.MainId);

            isFirst = itemData.isFirst;

            if (_currentImageId != itemData.PicId)
            {
                // Context.SelectedIndex = Index;

                if (_currentRequestId != -1 && !string.IsNullOrEmpty(_currentImageId))
                {
                    VideoImgResourceManager.Instance.CancelLoad(_currentImageId, _currentRequestId);
                }

                _currentImageId = itemData.PicId;

                // 发起新的加载请求
                _currentRequestId = VideoImgResourceManager.Instance.GetImage(_currentImageId, OnIconLoad);

                laodingImg.gameObject.SetActive(true);
            }

            adImg.gameObject.SetActive(false);
            vipImg.gameObject.SetActive(false);
            if (!isFirst)
            {
                if (Index == 2)
                {
                    vipImg.gameObject.SetActive(true);
                }
                else if (Index == 3)
                {
                    adImg.gameObject.SetActive(true);
                }
            }
        }

        private void OnIconLoad(Texture2D tex)
        {
            laodingImg.gameObject.SetActive(false);
            rawIcon.texture = tex;
        }

        public override void UpdatePosition(float position)
        {
            currentPosition = position;

            if (animator.isActiveAndEnabled)
            {
                animator.Play(AnimatorHash.Scroll, -1, position);
            }

            animator.speed = 0;
        }

        float currentPosition = 0;

        void OnEnable() => UpdatePosition(currentPosition);
    }
}
