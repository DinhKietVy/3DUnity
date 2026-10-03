using UnityEngine;
using UnityEngine.EventSystems;

public class CameraTouch : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [SerializeField] private ThirdPersonCamera cameraController;

    private Vector2 lastPosition;
    private bool touching;

    public void OnPointerDown(PointerEventData eventData)
    {
        touching = true;
        lastPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!touching)
            return;

        Vector2 currentPosition = eventData.position;

        Vector2 delta =
            currentPosition - lastPosition;

        lastPosition = currentPosition;

        if (cameraController != null)
        {
            cameraController.RotateByTouch(delta);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        touching = false;
    }
}