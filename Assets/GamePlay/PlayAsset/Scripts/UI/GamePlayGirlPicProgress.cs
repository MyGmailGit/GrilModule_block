using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;

/// <summary>
/// 获取的女图进度
/// </summary>
public class GamePlayGirlPicProgress : MonoBehaviour
{
    [SerializeField] private Image progress;
    [SerializeField] private Image[] heartImg;

    public void SetNowPicProgress()
    {
        var nowstageFinishPic = VideoSerilNumberManager.Instance.GetCurrentStageProgress();
        for (int i = 0; i < nowstageFinishPic.totalCount; ++i)
        {
            if (i < nowstageFinishPic.usedCount + 1)
            {
                heartImg[i].gameObject.SetActive(true);//.color = Color.white;
            }
            else
            {
                heartImg[i].gameObject.SetActive(false);//.color = Color.gray;
            }
        }
        progress.fillAmount = (float)(nowstageFinishPic.usedCount) / (nowstageFinishPic.totalCount - 1);
    }
}
