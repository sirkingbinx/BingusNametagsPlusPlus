using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BingusNametagsPlusPlus.Utilities;

public sealed class WindowDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private RectTransform window = null!;
    private Action<Vector2> moved = delegate { };
    private Vector2 pointerStart;
    private Vector2 windowStart;
    private float rightMargin;

    internal void Initialize(RectTransform target, Action<Vector2> onMoved, float previewMargin)
    {
        window = target;
        moved = onMoved;
        rightMargin = previewMargin;
    }

    public void OnBeginDrag(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)window.parent, data.position, data.pressEventCamera, out pointerStart);
        windowStart = window.anchoredPosition;
    }

    public void OnDrag(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)window.parent, data.position, data.pressEventCamera, out var pointer))
            return;

        window.anchoredPosition = windowStart + pointer - pointerStart;
        Clamp(window, rightMargin);
        moved(window.anchoredPosition);
    }

    internal static void Clamp(RectTransform target, float rightMargin)
    {
        var available = ((RectTransform)target.parent).rect.size;
        target.anchoredPosition = new Vector2(Mathf.Clamp(target.anchoredPosition.x, 0, Mathf.Max(0, available.x - target.sizeDelta.x - rightMargin)), Mathf.Clamp(target.anchoredPosition.y, -Mathf.Max(0, available.y - target.sizeDelta.y), 0));
    }
}
