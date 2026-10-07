using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IMainPage : MonoBehaviour
{
    [SerializeField] protected Canvas canvasPage;
    public virtual void Init() { }
    public virtual void Show()
    {
        (transform as RectTransform).anchoredPosition = Vector2.zero;
        canvasPage.enabled = true;
    }
    public virtual void Hide()
    {
        (transform as RectTransform).anchoredPosition = new Vector2(2500, 0);
        canvasPage.enabled = false;
    }
}
