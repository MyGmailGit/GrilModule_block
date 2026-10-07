using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Watermelon;

public class BottleController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private BallController ballPrefab;
    [SerializeField] private Sprite[] ballSprites;
    [SerializeField] private Transform coverTrans;
    [SerializeField] private ParticleSystem doneEffect;

    public event Action<BottleController> OnBottleClicked;
    /// <summary>
    /// 赋值，不能直接 += 或者 -=
    /// </summary>
    public Action<BottleController> OnBottleClickedOnly;

    private BallController[] balls;
    private int ballsAmount = 0;
    [SerializeField] private Transform[] positions;
    private bool isDone;
    private BottlesController bottlesController;

    private bool isTouchEnableFor = true;

    private void Awake()
    {
        balls = new BallController[4];
    }
    /// <summary>
    /// 临时暂停点击，主要为了新手引导
    /// </summary>
    /// <param name="isDisable"></param>
    public void SetIsTouchEnableFor(bool istouch)
    {
        isTouchEnableFor = istouch;
    }

    public void ResetBottle()
    {
        ResetInternalState();
        isDone = false;
        if (coverTrans != null)
            coverTrans.localScale = Vector3.zero;
        if (doneEffect != null)
            doneEffect.Stop();
        transform.DOKill();
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;
        gameObject.SetActive(false);

        isTouchEnableFor = true;
        OnBottleClickedOnly = null;
    }

    private void ResetInternalState()
    {
        StopAllCoroutines();
        for (int i = 0; i < ballsAmount; i++)
        {
            if (balls[i] != null)
            {
                balls[i].ResetForPool();
                bottlesController?.ReturnBallToPool(balls[i]);
            }
            balls[i] = null;
        }
        ballsAmount = 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isTouchEnableFor)
        {
            OnBottleClicked?.Invoke(this);
            OnBottleClickedOnly?.Invoke(this);
        }
    }

    public void PrepareForSpawn(BottlesController controller, int quantify, int[] selectedColors, Vector3 position)
    {
        bottlesController = controller;
        transform.SetParent(controller.transform, false);
        transform.position = position;
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);
        ResetInternalState();

        for (int x = 0; x < quantify; x++)
        {
            var ball = controller.GetBallFromPool();
            ball.PrepareForSpawn(ballSprites[selectedColors[x] - 1], selectedColors[x], positions[x].position, transform);
            balls[ballsAmount] = ball;
            ballsAmount++;
        }
        UpdateBallCoverStates();
        isDone = false;
        if (coverTrans != null)
            coverTrans.localScale = Vector3.zero;
        if (doneEffect != null)
            doneEffect.Stop();
    }

    public void SpawnBalls(int quantify, int[] selectedColors)
    {
        PrepareForSpawn(bottlesController, quantify, selectedColors, transform.position);
    }

    public void SelectBall()
    {
        if (ballsAmount > 0)
            balls[ballsAmount - 1].Select(positions[4].position);
    }

    public void UnselectBall()
    {
        if (ballsAmount > 0)
            balls[ballsAmount - 1].Unselect(positions[ballsAmount - 1].position);
    }

    /// <summary>
    /// 获取从顶部开始连续相同颜色的球的数量
    /// </summary>
    public int GetConsecutiveSameColorCount()
    {
        if (ballsAmount == 0) return 0;

        int topColor = balls[ballsAmount - 1].GetColorIndex();
        int count = 0;

        for (int i = ballsAmount - 1; i >= 0; i--)
        {
            if (balls[i].GetColorIndex() == topColor)
                count++;
            else
                break;
        }
        return count;
    }

    /// <summary>
    /// 获取顶部球的颜色索引
    /// </summary>
    public int GetTopColor()
    {
        return ballsAmount > 0 ? balls[ballsAmount - 1].GetColorIndex() : -1;
    }

    /// <summary>
    /// 检查目标瓶子能接收多少个指定颜色的球
    /// </summary>
    /// <param name="colorIndex">要移动的球的颜色</param>
    /// <param name="requestCount">请求移动的数量</param>
    /// <returns>实际能接收的数量</returns>
    public int CanReceiveBalls(int colorIndex, int requestCount)
    {
        int availableSpace = 4 - ballsAmount;

        if (availableSpace == 0) return 0;

        if (ballsAmount == 0)
            return Mathf.Min(requestCount, availableSpace);

        if (isDone)
            return 0;

        int topColor = balls[ballsAmount - 1].GetColorIndex();
        if (topColor == colorIndex)
            return Mathf.Min(requestCount, availableSpace);

        return 0;
    }

    /// <summary>
    /// 从当前瓶子取出顶部连续相同颜色的球（最多指定数量）
    /// </summary>
    /// <param name="count">要取出的数量</param>
    /// <param name="showAtFirst">是否是从第一个球就要展示一次从杯中到杯口</param>
    /// <returns>取出的球数组</returns>
    public BallController[] TransferBalls(int count, bool showAtFirst)
    {
        if (count <= 0 || count > ballsAmount)
            return new BallController[0];

        BallController[] transferred = new BallController[count];

        for (int i = 0; i < count; i++)
        {
            transferred[i] = balls[ballsAmount - i - 1];
            if (showAtFirst)
            {
                transferred[i].Select(positions[4].position);//, i * 0.1f);
            }
            else
            {
                if (i != 0)
                    transferred[i].Select(positions[4].position);//, (i - 1) * 0.1f);
            }
        }

        ballsAmount -= count;
        UpdateBallCoverStates();
        // VerifyIfIsDone();
        return transferred;
    }

    /// <summary>
    /// 接收一组球到当前瓶子
    /// </summary>
    public void ReceiveBalls(BallController[] ballsToReceive, bool showWaitAtFirst)
    {
        if (ballsToReceive == null || ballsToReceive.Length == 0)
            return;

        int colorIndex = ballsToReceive[0].GetColorIndex();
        int canReceive = CanReceiveBalls(colorIndex, ballsToReceive.Length);

        if (canReceive == 0) return;

        for (int i = 0; i < canReceive; i++)
        {
            BallController ball = ballsToReceive[i];
            ball.MoveBall(positions[4].position, positions[ballsAmount].position);//, showWaitAtFirst ? i + 1 : i);

            balls[ballsAmount] = ball;
            ballsAmount++;
        }

        UpdateBallCoverStates();
        VerifyIfIsDone();
    }

    /// <summary>
    /// 直接恢复一组球到当前瓶子，不进行颜色规则检查（回退一步）
    /// </summary>
    public void RestoreBalls(BallController[] ballsToRestore)
    {
        if (ballsToRestore == null || ballsToRestore.Length == 0)
            return;

        int availableSpace = 4 - ballsAmount;
        int restoreCount = Mathf.Min(availableSpace, ballsToRestore.Length);

        for (int i = 0; i < restoreCount; i++)
        {
            BallController ball = ballsToRestore[i];
            ball.MoveBall(positions[4].position, positions[ballsAmount].position);//, i + 1);

            balls[ballsAmount] = ball;
            ballsAmount++;
        }

        UpdateBallCoverStates();
    }

    /// <summary>
    /// 清理球引用
    /// </summary>
    public void ClearReference(int count)
    {
        for (int i = 0; i < count && ballsAmount + i < balls.Length; i++)
        {
            balls[ballsAmount + i] = null;
        }
    }

    private void UpdateBallCoverStates()
    {
        if (ballsAmount == 0)
            return;

        bool coverEnabled = LevelController.BallCoverEnabled;

        for (int i = 0; i < ballsAmount; i++)
        {
            if (balls[i] != null)
                balls[i].SetCoverVisible(coverEnabled);
        }

        if (!coverEnabled)
            return;

        // Top ball is always uncovered
        balls[ballsAmount - 1].SetCoverVisible(false);

        for (int i = ballsAmount - 2; i >= 0; i--)
        {
            if (balls[i + 1] != null && !balls[i + 1].GetCoverVisible() && balls[i] != null && balls[i].GetColorIndex() == balls[i + 1].GetColorIndex())
            {
                balls[i].SetCoverVisible(false);
            }
            else
            {
                break;
            }
        }
    }

    public void RetBotPosition(Vector3 pos)
    {
        transform.position = pos;

        for (int i = 0; i < ballsAmount; i++)
        {
            balls[i].SetPosition(positions[i].position);
        }
    }

    public bool AnyBallIsMoving()
    {
        for (int x = 0; x < ballsAmount; x++)
        {
            if (balls[x].GetAnimated())
                return true;
        }
        return false;
    }

    private void VerifyIfIsDone()
    {
        if (ballsAmount == 4)
        {
            int firstColor = balls[0].GetColorIndex();
            for (int x = 1; x < 4; x++)
            {
                if (balls[x].GetColorIndex() != firstColor)
                {
                    isDone = false;
                    return;
                }
            }
            isDone = true;
            bottlesController?.VerifyIfLevelIsDone();
            StartCoroutine(ShowDoneCoverEffect());
        }
        else
            isDone = false;
    }

    private IEnumerator ShowDoneCoverEffect()
    {
        while (AnyBallIsMoving())
        {
            yield return null;
        }
        if (doneEffect != null)
            doneEffect.Play();
        if (coverTrans != null)
        {
            AudioController.PlaySound(AudioController.AudioClips.particle1);

            Haptic.Play(Haptic.HAPTIC_HARD);

            coverTrans.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.OutBack);
            coverTrans.DOLocalMoveY(1, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
            {
                AudioController.PlaySound(AudioController.AudioClips.bottleClose);
                coverTrans.DOLocalMoveY(0, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    if (doneEffect != null)
                        doneEffect.Stop();
                });
            });
        }
        yield return null;
    }

    // ========== Getter / Setter ==========
    public void SetBController(BottlesController bController) => bottlesController = bController;
    public bool GetIsDone() => isDone;
    public int GetBallsAmount() => ballsAmount;
}
