using System;
using System.Collections;
using System.Collections.Generic;
using ball;
using DG.Tweening;
using UnityEngine;
using Watermelon;

public class BallController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer ballColor;
    [SerializeField] private Animator animator;
    [SerializeField] private BallMove ballMove;
    [SerializeField] private SpriteRenderer ballCover;

    private enum BallState
    {
        Idle,
        SetectMoving,
        Moving
    }

    private BallState currentState = BallState.Idle;
    private int colorIndex;

    private void Start()
    {
        ballMove.startMovingAction = onStartMove;
        ballMove.endMovingAction = onEndMove;
        ballMove.SetBallController(this);
    }
    private void onEndMove()
    {
        currentState = BallState.Idle;
        animator.Play("Unselected");
    }

    private void onStartMove()
    {
    }
    public void SetRendererOrderLayer(int orderLayer)
    {
        ballColor.sortingOrder = orderLayer;
    }
    public void SetColor(Sprite color, int cIndex)
    {
        ballColor.sprite = color;
        colorIndex = cIndex;
    }

    public void PrepareForSpawn(Sprite color, int cIndex, Vector3 position, Transform parent)
    {
        // transform.SetParent(parent, false);
        transform.position = position;
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);
        ballColor.sprite = color;
        colorIndex = cIndex;
        currentState = BallState.Idle;
        SetCoverVisible(true);
        animator.Play("Idle");
    }

    public void ResetForPool()
    {
        StopAllCoroutines();
        transform.DOKill();
        currentState = BallState.Idle;
        colorIndex = 0;
        ballColor.sprite = null;
        SetCoverVisible(false);
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;
        // transform.SetParent(null, false);
        gameObject.SetActive(false);
    }

    public int GetColorIndex()
    {
        return colorIndex;
    }

    public bool GetCoverVisible()
    {
        return ballCover != null && ballCover.gameObject.activeSelf;
    }

    public void SetCoverVisible(bool visible)
    {
        if (ballCover != null)
            ballCover.gameObject.SetActive(visible);
    }

    public bool GetAnimated()
    {
        return currentState != BallState.Idle;
    }

    public void setIdleAnimation()
    {
        animator.Play("Idle");
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public void Select(Vector3 finalPosition)//, float delay = 0)
    {
        currentState = BallState.SetectMoving;

        // StartCoroutine(MoveAnimation(finalPosition, delay));

        ballMove.MoveTo(finalPosition);

        animator.Play("Selected");
    }

    public void Unselect(Vector3 finalPosition)
    {
        currentState = BallState.SetectMoving;

        // StartCoroutine(MoveAnimation(finalPosition));
        ballMove.MoveTo(finalPosition);

        animator.Play("Unselected");
    }

    private IEnumerator MoveAnimation(Vector3 finalPosition, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        Vector3 inicialPosition = transform.position;

        float t = 0;

        while (t <= 0.2f)
        {
            transform.position = Vector3.Lerp(inicialPosition, finalPosition, (t / 0.2f));

            t += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        transform.position = finalPosition;
        if (currentState == BallState.SetectMoving)
            currentState = BallState.Idle;
    }

    public void MoveBall(Vector3 bottlePosition, Vector3 endPosition)//, int delayMultiplier)
    {
        currentState = BallState.Moving;

        // StartCoroutine(ChangeBottleAnimation(bottlePosition, endPosition, delayMultiplier));
        ballMove.AddPos(bottlePosition, 2, true);
        ballMove.AddPos(endPosition);

        // animator.Play("Unselected");

        // currentState = BallState.Idle;
    }

    private IEnumerator ChangeBottleAnimation(Vector3 bottlePosition, Vector3 endPosition, int delayMultiplier)
    {
        yield return new WaitForSeconds(0.15f * delayMultiplier);
        Vector3 inicialPosition = transform.position;
        float t = 0;

        Vector3 pos1 = inicialPosition + new Vector3(0, 0.3f, 0);
        // Vector3 pos2 = Vector3.Lerp(pos1, bottlePosition, 0.1f) + new Vector3(0, 0.5f, 0);
        Vector3 pos3 = Vector3.Lerp(pos1, bottlePosition, 0.6f) + new Vector3(0, 0.4f, 0);

        Vector3[] vector3s = new Vector3[3] { pos1, pos3, bottlePosition };

        yield return transform.DOPath(vector3s, 0.35f, PathType.CatmullRom).WaitForCompletion();
        // yield return new WaitForSeconds(1.25f);

        transform.position = bottlePosition;

        animator.Play("Unselected");

        inicialPosition = transform.position;
        t = 0;
        while (t <= 0.15f)
        {
            transform.position = Vector3.Lerp(inicialPosition, endPosition, (t / 0.15f));

            t += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        transform.position = endPosition;

        currentState = BallState.Idle;
    }
}

