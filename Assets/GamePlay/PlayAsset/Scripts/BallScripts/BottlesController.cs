using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Watermelon;

public class BottlesController : MonoBehaviour
{
    private const float DELTA_BOTTLE_WIDTH = 0.6f;

    [SerializeField] private BottleController bottlePrefab;
    [SerializeField] private BallController ballPrefab;

    private static BottlesController instance;
    private readonly List<BottleController> bottles = new List<BottleController>();
    private readonly Queue<BottleController> bottlePool = new Queue<BottleController>();
    private readonly Queue<BallController> ballPool = new Queue<BallController>();

    private int bottlesAmount = 0;
    private int bottlesIndex = 0;

    private BottleController firstBottle;
    private BottleController secondBottle;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        gameObject.name = "[BottlesController]";
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void InstantiateBottle(int numberOfColors, int color1, int color2, int color3, int color4)
    {
        var bottle = GetBottleFromPool();
        bottle.SetBController(this);

        int[] colors = { color1, color2, color3, color4 };
        bottle.PrepareForSpawn(this, numberOfColors, colors, ChooseBottlePosition(bottlesIndex));
        bottle.OnBottleClicked += OnBottleClicked;

        bottles.Add(bottle);
        bottlesIndex++;
    }

    public BallController GetBallFromPool()
    {
        if (ballPool.Count > 0)
            return ballPool.Dequeue();

        var ball = Instantiate(ballPrefab, transform, false);
        ball.gameObject.SetActive(false);
        return ball;
    }

    public void ReturnBallToPool(BallController ball)
    {
        if (ball == null)
            return;

        ball.ResetForPool();
        ballPool.Enqueue(ball);
    }

    public (BottleController firstBottle, BottleController secondBottle) GetFirstAndSecondBottle()
    {
        if (bottles.Count < 2) return (null, null);
        return (bottles[0], bottles[1]);
    }

    private BottleController GetBottleFromPool()
    {
        if (bottlePool.Count > 0)
        {
            BottleController pooledBottle = bottlePool.Dequeue();
            pooledBottle.gameObject.SetActive(true);
            return pooledBottle;
        }

        BottleController createdBottle = Instantiate(bottlePrefab, transform, false);
        createdBottle.SetBController(this);
        createdBottle.gameObject.SetActive(false);
        return createdBottle;
    }

    private void ReturnBottleToPool(BottleController bottle)
    {
        if (bottle == null)
            return;

        bottle.OnBottleClicked -= OnBottleClicked;
        bottle.ResetBottle();
        bottlePool.Enqueue(bottle);
    }

    public void FinishScaleToHide(Action callback)
    {
        transform.localScale = Vector3.one;
        transform.DOScale(0, 0.45f).SetDelay(1f).OnComplete(() =>
        {
            callback?.Invoke();
            ResetBallSortModule();
            transform.localScale = Vector3.one;
        });
    }

    public void ResetBallSortModule()
    {
        firstBottle = null;
        secondBottle = null;

        for (int i = bottles.Count - 1; i >= 0; i--)
        {
            var bottle = bottles[i];
            if (bottle != null)
                ReturnBottleToPool(bottle);
        }

        bottles.Clear();
        bottlesAmount = 0;
        bottlesIndex = 0;
    }

    #region touch input
    private void OnBottleClicked(BottleController btocontroller)
    {
        if (btocontroller != null)
        {
            if (firstBottle == null)
            {
                firstBottle = btocontroller;

                if (firstBottle.AnyBallIsMoving() || firstBottle.GetBallsAmount() == 0 || firstBottle.GetIsDone())
                {
                    firstBottle = null;
                    return;
                }

                firstBottle.SelectBall();
            }
            else
            {
                if (firstBottle == btocontroller)
                {
                    firstBottle.UnselectBall();
                    firstBottle = null;
                }
                else
                {
                    secondBottle = btocontroller;

                    if (secondBottle.GetBallsAmount() == 4 ||
                        (firstBottle.GetTopColor() != secondBottle.GetTopColor() && secondBottle.GetBallsAmount() != 0))
                    {
                        firstBottle.UnselectBall();
                        firstBottle = btocontroller;
                        firstBottle.SelectBall();
                        secondBottle = null;
                    }
                    else
                    {
                        TryTransfer(firstBottle, secondBottle);
                        firstBottle = null;
                        secondBottle = null;
                    }
                }
            }
        }
    }

    public void TryTransfer(BottleController from, BottleController to)
    {
        int consecutiveCount = from.GetConsecutiveSameColorCount();
        if (consecutiveCount == 0) return;

        int topColor = from.GetTopColor();
        int canReceive = to.CanReceiveBalls(topColor, consecutiveCount);
        if (canReceive == 0) return;

        BallController[] ballsToMove = from.TransferBalls(canReceive, false);
        to.ReceiveBalls(ballsToMove, false);

        LevelController.SRecordMoveStep(from, to, canReceive);
        from.ClearReference(canReceive);
    }

    #endregion

    public void SetBottlesAmount(int bottlesAmount)
    {
        ResetBallSortModule();
        this.bottlesAmount = bottlesAmount;
    }

    public int GetBottlesAmount()
    {
        return bottlesAmount;
    }

    public void VerifyIfLevelIsDone()
    {
        if (bottles.Count == 0)
            return;

        for (int x = 0; x < bottles.Count; x++)
        {
            if (bottles[x].GetBallsAmount() != 0 && !bottles[x].GetIsDone())
                return;
        }

        GameController.GameComplete();
        Debug.Log("level done");
    }

    public bool IsPlayingMove()
    {
        foreach (var it in bottles)
        {
            if (it.AnyBallIsMoving()) return true;
        }
        return false;
    }

    public void PullBackOneStep(BottleController fromArg, BottleController toArg, int ballNumArg)
    {
        if (fromArg == null || toArg == null || ballNumArg <= 0)
            return;

        BallController[] ballsToMove = toArg.TransferBalls(ballNumArg, true);
        if (ballsToMove == null || ballsToMove.Length == 0)
            return;

        fromArg.RestoreBalls(ballsToMove);
        toArg.ClearReference(ballsToMove.Length);
    }

    public bool AddABottle()
    {
        int botNum = bottlesAmount + 1;

        if (botNum > 16)
        {
            SystemMessage.ShowMessage("Too many bottles");
            return false;
        }

        bottlesAmount = botNum;
        SpawnBottle();

        for (int i = 0; i < bottles.Count; i++)
        {
            bottles[i].RetBotPosition(ChooseBottlePosition(i));
        }

        CameraController.Instance.SetCameraWidth(ChooseBottlesWidth());

        return true;
    }

    public bool SuggestOneStep()
    {
        if (IsPlayingMove())
            return false;

        for (int fromIndex = 0; fromIndex < bottles.Count; fromIndex++)
        {
            BottleController from = bottles[fromIndex];
            if (from.GetBallsAmount() == 0 || from.GetIsDone())
                continue;

            int consecutiveCount = from.GetConsecutiveSameColorCount();
            if (consecutiveCount == 0)
                continue;

            int topColor = from.GetTopColor();

            for (int toIndex = 0; toIndex < bottles.Count; toIndex++)
            {
                if (fromIndex == toIndex)
                    continue;

                BottleController to = bottles[toIndex];
                int canReceive = to.CanReceiveBalls(topColor, consecutiveCount);
                if (canReceive == 0)
                    continue;

                BallController[] ballsToMove = from.TransferBalls(canReceive, true);
                if (ballsToMove == null || ballsToMove.Length == 0)
                    continue;

                to.ReceiveBalls(ballsToMove, true);
                LevelController.SRecordMoveStep(from, to, canReceive);
                from.ClearReference(canReceive);

                return true;
            }
        }

        return false;
    }
    private float ChooseBottlesWidth()
    {
        switch (bottlesAmount)
        {
            case 2:
                return 0.4f * 2 + DELTA_BOTTLE_WIDTH;
            case 3:
                return 0.8f * 2 + DELTA_BOTTLE_WIDTH;
            case 4:
                return 0.9f * 2 + DELTA_BOTTLE_WIDTH;
            case 5:
                return 1f * 2 + DELTA_BOTTLE_WIDTH;
            case 6:
                return 1.25f * 2 + DELTA_BOTTLE_WIDTH;
            case 7:
                return 0.9f * 2 + DELTA_BOTTLE_WIDTH;
            case 8:
                return 0.9f * 2 + DELTA_BOTTLE_WIDTH;
            case 9:
                return 1f * 2 + DELTA_BOTTLE_WIDTH;
            case 10:
                return 1f * 2 + DELTA_BOTTLE_WIDTH;
            case 11:
                return 1.25f * 2 + DELTA_BOTTLE_WIDTH;
            case 12:
                return 1.25f * 2 + DELTA_BOTTLE_WIDTH;
            case 13:
                return 1.38f * 2 + DELTA_BOTTLE_WIDTH;
            case 14:
                return 1.38f * 2 + DELTA_BOTTLE_WIDTH;
            case 15:
                return 1.54f * 2 + DELTA_BOTTLE_WIDTH;
            case 16:
                return 1.54f * 2 + DELTA_BOTTLE_WIDTH;
        }

        return 1;
    }
    private Vector3 ChooseBottlePosition(int bottle)
    {
        switch (bottlesAmount)
        {
            case 2:
                if (bottle == 0) return new Vector3(-0.4f, 0, 0);
                if (bottle == 1) return new Vector3(0.4f, 0, 0);
                break;

            case 3:
                if (bottle == 0) return new Vector3(-0.8f, 0, 0);
                if (bottle == 1) return new Vector3(0, 0, 0);
                if (bottle == 2) return new Vector3(0.8f, 0, 0);
                break;

            case 4:
                if (bottle == 0) return new Vector3(-0.9f, 0, 0);
                if (bottle == 1) return new Vector3(-0.3f, 0, 0);
                if (bottle == 2) return new Vector3(0.3f, 0, 0);
                if (bottle == 3) return new Vector3(0.9f, 0, 0);
                break;

            case 5:
                if (bottle == 0) return new Vector3(-1f, 0, 0);
                if (bottle == 1) return new Vector3(-0.5f, 0, 0);
                if (bottle == 2) return new Vector3(0, 0, 0);
                if (bottle == 3) return new Vector3(0.5f, 0, 0);
                if (bottle == 4) return new Vector3(1f, 0, 0);
                break;

            case 6:
                if (bottle == 0) return new Vector3(-1.25f, 0, 0);
                if (bottle == 1) return new Vector3(-0.75f, 0, 0);
                if (bottle == 2) return new Vector3(-0.25f, 0, 0);
                if (bottle == 3) return new Vector3(0.25f, 0, 0);
                if (bottle == 4) return new Vector3(0.75f, 0, 0);
                if (bottle == 5) return new Vector3(1.25f, 0, 0);
                break;

            case 7:
                if (bottle == 0) return new Vector3(-0.9f, 0.7f, 0);
                if (bottle == 1) return new Vector3(-0.3f, 0.7f, 0);
                if (bottle == 2) return new Vector3(0.3f, 0.7f, 0);
                if (bottle == 3) return new Vector3(0.9f, 0.7f, 0);
                if (bottle == 4) return new Vector3(-0.8f, -0.85f, 0);
                if (bottle == 5) return new Vector3(0f, -0.85f, 0);
                if (bottle == 6) return new Vector3(0.8f, -0.85f, 0);
                break;

            case 8:
                if (bottle == 0) return new Vector3(-0.9f, 0.7f, 0);
                if (bottle == 1) return new Vector3(-0.3f, 0.7f, 0);
                if (bottle == 2) return new Vector3(0.3f, 0.7f, 0);
                if (bottle == 3) return new Vector3(0.9f, 0.7f, 0);
                if (bottle == 4) return new Vector3(-0.9f, -0.85f, 0);
                if (bottle == 5) return new Vector3(-0.3f, -0.85f, 0);
                if (bottle == 6) return new Vector3(0.3f, -0.85f, 0);
                if (bottle == 7) return new Vector3(0.9f, -0.85f, 0);
                break;

            case 9:
                if (bottle == 0) return new Vector3(-1f, 0.7f, 0);
                if (bottle == 1) return new Vector3(-0.5f, 0.7f, 0);
                if (bottle == 2) return new Vector3(0, 0.7f, 0);
                if (bottle == 3) return new Vector3(0.5f, 0.7f, 0);
                if (bottle == 4) return new Vector3(1f, 0.7f, 0);
                if (bottle == 5) return new Vector3(-0.9f, -0.85f, 0);
                if (bottle == 6) return new Vector3(-0.3f, -0.85f, 0);
                if (bottle == 7) return new Vector3(0.3f, -0.85f, 0);
                if (bottle == 8) return new Vector3(0.9f, -0.85f, 0);
                break;

            case 10:
                if (bottle == 0) return new Vector3(-1f, 0.7f, 0);
                if (bottle == 1) return new Vector3(-0.5f, 0.7f, 0);
                if (bottle == 2) return new Vector3(0f, 0.7f, 0);
                if (bottle == 3) return new Vector3(0.5f, 0.7f, 0);
                if (bottle == 4) return new Vector3(1f, 0.7f, 0);
                if (bottle == 5) return new Vector3(-1f, -0.85f, 0);
                if (bottle == 6) return new Vector3(-0.5f, -0.85f, 0);
                if (bottle == 7) return new Vector3(0f, -0.85f, 0);
                if (bottle == 8) return new Vector3(0.5f, -0.85f, 0);
                if (bottle == 9) return new Vector3(1f, -0.85f, 0);
                break;

            case 11:
                if (bottle == 0) return new Vector3(-1f, 0.7f, 0);
                if (bottle == 1) return new Vector3(-0.5f, 0.7f, 0);
                if (bottle == 2) return new Vector3(0f, 0.7f, 0);
                if (bottle == 3) return new Vector3(0.5f, 0.7f, 0);
                if (bottle == 4) return new Vector3(1f, 0.7f, 0);

                if (bottle == 5) return new Vector3(-1.25f, -0.85f, 0);
                if (bottle == 6) return new Vector3(-0.75f, -0.85f, 0);
                if (bottle == 7) return new Vector3(-0.25f, -0.85f, 0);
                if (bottle == 8) return new Vector3(0.25f, -0.85f, 0);
                if (bottle == 9) return new Vector3(0.75f, -0.85f, 0);
                if (bottle == 10) return new Vector3(1.25f, -0.85f, 0);
                break;

            case 12:
                if (bottle == 0) return new Vector3(-1.25f, 0.7f, 0);
                if (bottle == 1) return new Vector3(-0.75f, 0.7f, 0);
                if (bottle == 2) return new Vector3(-0.25f, 0.7f, 0);
                if (bottle == 3) return new Vector3(0.25f, 0.7f, 0);
                if (bottle == 4) return new Vector3(0.75f, 0.7f, 0);
                if (bottle == 5) return new Vector3(1.25f, 0.7f, 0);

                if (bottle == 6) return new Vector3(-1.25f, -0.85f, 0);
                if (bottle == 7) return new Vector3(-0.75f, -0.85f, 0);
                if (bottle == 8) return new Vector3(-0.25f, -0.85f, 0);
                if (bottle == 9) return new Vector3(0.25f, -0.85f, 0);
                if (bottle == 10) return new Vector3(0.75f, -0.85f, 0);
                if (bottle == 11) return new Vector3(1.25f, -0.85f, 0);
                break;

            case 13:
                if (bottle == 0) return new Vector3(-1.38f, 0.7f, 0);
                if (bottle == 1) return new Vector3(-0.92f, 0.7f, 0);
                if (bottle == 2) return new Vector3(-0.46f, 0.7f, 0);
                if (bottle == 3) return new Vector3(0f, 0.7f, 0);
                if (bottle == 4) return new Vector3(0.46f, 0.7f, 0);
                if (bottle == 5) return new Vector3(0.92f, 0.7f, 0);
                if (bottle == 6) return new Vector3(1.38f, 0.7f, 0);

                if (bottle == 7) return new Vector3(-1.25f, -0.85f, 0);
                if (bottle == 8) return new Vector3(-0.75f, -0.85f, 0);
                if (bottle == 9) return new Vector3(-0.25f, -0.85f, 0);
                if (bottle == 10) return new Vector3(0.25f, -0.85f, 0);
                if (bottle == 11) return new Vector3(0.75f, -0.85f, 0);
                if (bottle == 12) return new Vector3(1.25f, -0.85f, 0);
                break;

            case 14:
                if (bottle == 0) return new Vector3(-1.38f, 0.7f, 0);
                if (bottle == 1) return new Vector3(-0.92f, 0.7f, 0);
                if (bottle == 2) return new Vector3(-0.46f, 0.7f, 0);
                if (bottle == 3) return new Vector3(0f, 0.7f, 0);
                if (bottle == 4) return new Vector3(0.46f, 0.7f, 0);
                if (bottle == 5) return new Vector3(0.92f, 0.7f, 0);
                if (bottle == 6) return new Vector3(1.38f, 0.7f, 0);

                if (bottle == 7) return new Vector3(-1.38f, -0.85f, 0);
                if (bottle == 8) return new Vector3(-0.92f, -0.85f, 0);
                if (bottle == 9) return new Vector3(-0.46f, -0.85f, 0);
                if (bottle == 10) return new Vector3(0f, -0.85f, 0);
                if (bottle == 11) return new Vector3(0.46f, -0.85f, 0);
                if (bottle == 12) return new Vector3(0.92f, -0.85f, 0);
                if (bottle == 13) return new Vector3(1.38f, -0.85f, 0);

                // if (bottle == 0) return new Vector3(-1.2f, 0.7f, 0);
                // if (bottle == 1) return new Vector3(-0.8f, 0.7f, 0);
                // if (bottle == 2) return new Vector3(-0.4f, 0.7f, 0);
                // if (bottle == 3) return new Vector3(0f, 0.7f, 0);
                // if (bottle == 4) return new Vector3(0.4f, 0.7f, 0);
                // if (bottle == 5) return new Vector3(0.8f, 0.7f, 0);
                // if (bottle == 6) return new Vector3(1.2f, 0.7f, 0);

                // if (bottle == 7) return new Vector3(-1.2f, -0.85f, 0);
                // if (bottle == 8) return new Vector3(-0.8f, -0.85f, 0);
                // if (bottle == 9) return new Vector3(-0.4f, -0.85f, 0);
                // if (bottle == 10) return new Vector3(0f, -0.85f, 0);
                // if (bottle == 11) return new Vector3(0.4f, -0.85f, 0);
                // if (bottle == 12) return new Vector3(0.8f, -0.85f, 0);
                // if (bottle == 13) return new Vector3(1.2f, -0.85f, 0);
                break;

            case 15:
                if (bottle == 0) return new Vector3(-1.54f, 0.7f, 0);
                if (bottle == 1) return new Vector3(-1.1f, 0.7f, 0);
                if (bottle == 2) return new Vector3(-0.66f, 0.7f, 0);
                if (bottle == 3) return new Vector3(-0.22f, 0.7f, 0);
                if (bottle == 4) return new Vector3(0.22f, 0.7f, 0);
                if (bottle == 5) return new Vector3(0.66f, 0.7f, 0);
                if (bottle == 6) return new Vector3(1.1f, 0.7f, 0);
                if (bottle == 7) return new Vector3(1.54f, 0.7f, 0);

                if (bottle == 8) return new Vector3(-1.38f, -0.85f, 0);
                if (bottle == 9) return new Vector3(-0.92f, -0.85f, 0);
                if (bottle == 10) return new Vector3(-0.46f, -0.85f, 0);
                if (bottle == 11) return new Vector3(0f, -0.85f, 0);
                if (bottle == 12) return new Vector3(0.46f, -0.85f, 0);
                if (bottle == 13) return new Vector3(0.92f, -0.85f, 0);
                if (bottle == 14) return new Vector3(1.38f, -0.85f, 0);
                // if (bottle == 8) return new Vector3(-1.2f, -0.85f, 0);
                // if (bottle == 9) return new Vector3(-0.8f, -0.85f, 0);
                // if (bottle == 10) return new Vector3(-0.4f, -0.85f, 0);
                // if (bottle == 11) return new Vector3(0f, -0.85f, 0);
                // if (bottle == 12) return new Vector3(0.4f, -0.85f, 0);
                // if (bottle == 13) return new Vector3(0.8f, -0.85f, 0);
                // if (bottle == 14) return new Vector3(1.2f, -0.85f, 0);
                break;

            case 16:
                if (bottle == 0) return new Vector3(-1.54f, 0.7f, 0);
                if (bottle == 1) return new Vector3(-1.1f, 0.7f, 0);
                if (bottle == 2) return new Vector3(-0.66f, 0.7f, 0);
                if (bottle == 3) return new Vector3(-0.22f, 0.7f, 0);
                if (bottle == 4) return new Vector3(0.22f, 0.7f, 0);
                if (bottle == 5) return new Vector3(0.66f, 0.7f, 0);
                if (bottle == 6) return new Vector3(1.1f, 0.7f, 0);
                if (bottle == 7) return new Vector3(1.54f, 0.7f, 0);

                if (bottle == 8) return new Vector3(-1.54f, -0.85f, 0);
                if (bottle == 9) return new Vector3(-1.1f, -0.85f, 0);
                if (bottle == 10) return new Vector3(-0.66f, -0.85f, 0);
                if (bottle == 11) return new Vector3(-0.22f, -0.85f, 0);
                if (bottle == 12) return new Vector3(0.22f, -0.85f, 0);
                if (bottle == 13) return new Vector3(0.66f, -0.85f, 0);
                if (bottle == 14) return new Vector3(1.1f, -0.85f, 0);
                if (bottle == 15) return new Vector3(1.54f, -0.85f, 0);
                // if (bottle == 0) return new Vector3(-1.4f, 0.7f, 0);
                // if (bottle == 1) return new Vector3(-1f, 0.7f, 0);
                // if (bottle == 2) return new Vector3(-0.6f, 0.7f, 0);
                // if (bottle == 3) return new Vector3(-0.2f, 0.7f, 0);
                // if (bottle == 4) return new Vector3(0.2f, 0.7f, 0);
                // if (bottle == 5) return new Vector3(0.6f, 0.7f, 0);
                // if (bottle == 6) return new Vector3(1f, 0.7f, 0);
                // if (bottle == 7) return new Vector3(1.4f, 0.7f, 0);

                // if (bottle == 8) return new Vector3(-1.4f, -0.85f, 0);
                // if (bottle == 9) return new Vector3(-1f, -0.85f, 0);
                // if (bottle == 10) return new Vector3(-0.6f, -0.85f, 0);
                // if (bottle == 11) return new Vector3(-0.2f, -0.85f, 0);
                // if (bottle == 12) return new Vector3(0.2f, -0.85f, 0);
                // if (bottle == 13) return new Vector3(0.6f, -0.85f, 0);
                // if (bottle == 14) return new Vector3(1f, -0.85f, 0);
                // if (bottle == 15) return new Vector3(1.4f, -0.85f, 0);
                break;
        }

        return new Vector3(0, 0, 0);
    }

    public void SpawnBottle(int color1, int color2, int color3, int color4)
    {
        InstantiateBottle(4, color1, color2, color3, color4);
    }

    public void SpawnBottle(int color1, int color2, int color3)
    {
        InstantiateBottle(3, color1, color2, color3, 0);
    }

    public void SpawnBottle(int color1, int color2)
    {
        InstantiateBottle(2, color1, color2, 0, 0);
    }

    public void SpawnBottle(int color1)
    {
        InstantiateBottle(1, color1, 0, 0, 0);
    }

    public void SpawnBottle()
    {
        InstantiateBottle(0, 0, 0, 0, 0);
    }
}
