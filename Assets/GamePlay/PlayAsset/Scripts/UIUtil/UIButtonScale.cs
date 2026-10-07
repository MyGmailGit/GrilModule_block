using UnityEngine;
using UnityEngine.EventSystems;
using Watermelon;
using DG.Tweening;

[DisallowMultipleComponent]
public class UIButtonScale : MonoBehaviour,
    IPointerUpHandler,
    IPointerDownHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public GameObject tweenTarget;
    public Vector3 pressedScale = new Vector3(0.95f, 0.95f, 0.95f);
    public float duration = 0.1f;
    public bool needRemoveAllTween = true;
    private Vector3 _cacheScale;
    private Tween _tweener;
    private bool _started = false;
    private bool _pressed = false;

    void Start()
    {
        Init();
    }

    void Init()
    {
        if (!_started)
        {
            _started = true;
            if (tweenTarget == null) tweenTarget = gameObject;
            _cacheScale = tweenTarget.transform.localScale;
        }
    }

    public void OnDestroy()
    {
        _tweener?.Kill();
        _tweener = null;

        if (tweenTarget != null)
        {
            tweenTarget.transform.localScale = _cacheScale;
        }
    }

    void OnEnable()
    {
        if (_started)
        {
            _pressed = false;
            tweenTarget.transform.localScale = _cacheScale;
        }
    }

    void OnDisable()
    {
        if (_started)
        {
            if (tweenTarget != null)
            {
                if (needRemoveAllTween == false)
                {
                    _tweener?.Kill();
                    _tweener = null;
                }
                tweenTarget.transform.localScale = _cacheScale;
            }
        }
    }

    void OnPress(bool isPressed)
    {
        if (!_started)
        {
            Init();
        }

        if (enabled)
        {
            if (needRemoveAllTween == false)
            {
                _tweener?.Kill();
                _tweener = null;
            }
            if (isPressed)
            {
                Vector3 destScale = new Vector3(pressedScale.x * _cacheScale.x, pressedScale.y * _cacheScale.y, pressedScale.z * _cacheScale.z);

                _tweener = tweenTarget.transform.DOScale(destScale, duration);
            }
            else
            {
                _tweener = tweenTarget.transform.DOScale(_cacheScale, duration);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        OnPress(_pressed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        OnPress(_pressed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPress(_pressed);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPress(false);
    }

    public void SetDefaultScale(Vector3 scale)
    {
        if (!_started)
        {
            Init();
        }
        _cacheScale = scale;
        if (needRemoveAllTween == false)
        {
            _tweener?.Kill();
            _tweener = null;
        }
    }
}
