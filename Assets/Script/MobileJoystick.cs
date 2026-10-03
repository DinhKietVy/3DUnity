using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileJoystick : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [SerializeField] private RectTransform handle;

    private RectTransform joystickArea;

    private Vector2 input;

    public Vector2 Input
    {
        get { return input; }
    }

    private void Awake()
    {
        joystickArea = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickArea,
            eventData.position,
            eventData.pressEventCamera,
            out position
        );

        float radius = joystickArea.sizeDelta.x / 2f;

        input = position / radius;

        input = Vector2.ClampMagnitude(input, 1f);

        if (handle != null)
        {
            handle.anchoredPosition =
                input * radius;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;

        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }
}