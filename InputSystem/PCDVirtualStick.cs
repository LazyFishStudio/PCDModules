using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Layouts;

public class PCDVirtualStick : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [InputControl(layout = "Vector2")]
    [SerializeField]
    private string m_ControlPath;
    public float movementRange = 50f;
    public GameObject background;
    public UnityEngine.UI.Image hitAreaImg;

    private void Start() {
        m_StartPos = ((RectTransform)transform).anchoredPosition;
        hitAreaImg.alphaHitTestMinimumThreshold = 0.5f;
    }

    public void OnPointerDown(PointerEventData eventData) {
        Vector2 pointerPosition = eventData.position;
        Camera uiCamera = eventData.pressEventCamera;

        var canvasRect = transform.parent?.GetComponentInParent<RectTransform>();
        if (canvasRect == null) {
            Debug.LogError("OnScreenStick needs to be attached as a child to a UI Canvas to function properly.");
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, uiCamera, out var pointerDown);
        m_PointerDownPos = pointerDown;

        ((RectTransform)transform).anchoredPosition = m_PointerDownPos;
        if (background != null)
            ((RectTransform)background.transform).anchoredPosition = m_PointerDownPos;
    }

    public void OnPointerUp(PointerEventData eventData) {
        ((RectTransform)transform).anchoredPosition = m_PointerDownPos = m_StartPos;
        if (background != null)
            ((RectTransform)background.transform).anchoredPosition = m_StartPos;
        SendValueToControl(Vector2.zero);
    }

    public void OnDrag(PointerEventData eventData) {
        Vector2 pointerPosition = eventData.position;
        Camera uiCamera = eventData.pressEventCamera;

        var canvasRect = transform.parent?.GetComponentInParent<RectTransform>();
        if (canvasRect == null) {
            Debug.LogError("OnScreenStick needs to be attached as a child to a UI Canvas to function properly.");
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, uiCamera, out var position);
        var delta = position - m_PointerDownPos;
        delta = Vector2.ClampMagnitude(delta, movementRange);
        ((RectTransform)transform).anchoredPosition = m_PointerDownPos + delta;

        var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);
        SendValueToControl(newPos);
    }

    private Vector2 m_StartPos;
    private Vector2 m_PointerDownPos;

    protected override string controlPathInternal {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }
}
