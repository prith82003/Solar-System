using UnityEngine;

public class UIDraggable : MonoBehaviour
{
    protected virtual void OnDragStart(Vector3 mousePosition) { }
    protected virtual void OnDragEnd(Vector3 mousePosition) { }
    protected virtual void OnDrag(Vector3 mousePosDiff, Vector3 mousePosition) { }

    Vector3 startMousePosition;

    private void OnMouseDown()
    {
        startMousePosition = Input.mousePosition;
        OnDragStart(Input.mousePosition);
    }

    private void OnMouseDrag()
    {
        OnDrag(Input.mousePosition - startMousePosition, Input.mousePosition);
    }

    private void OnMouseUp()
    {
        OnDragEnd(Input.mousePosition);
    }
}
