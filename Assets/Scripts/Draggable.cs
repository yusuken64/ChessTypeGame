using System;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;

    public LayerMask GridLayer;
    public Action OnHold;
    public Action<Cell> OnReleased;

    public bool IsDraggable;

    void OnMouseDown()
    {
        if (!IsDraggable)
        {
            return;
        }
        offset = transform.position - GetMouseWorldPosition();
        var hit = GetCellUnderneath();

        var cell = hit.collider?.GetComponent<Cell>();
        if (cell != null)
        {
            isDragging = true;
            OnHold?.Invoke();
        }
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
            CheckCellUnderneath();
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        RaycastHit2D hit = GetCellUnderneath();
        Cell cell = null;
        if (hit.collider != null)
        {
            cell = hit.collider.GetComponent<Cell>();
        }
        OnReleased?.Invoke(cell);
    }

    private void CheckCellUnderneath()
    {
        RaycastHit2D hit = GetCellUnderneath();

        //if (hit.collider != null)
        //{
        //    Debug.Log("Over Cell: " + hit.collider.gameObject.name);
        //}
        //else
        //{
        //    Debug.Log("Not over any cell.");
        //}
    }

    private RaycastHit2D GetCellUnderneath()
    {
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.1f; // Move ray slightly above to avoid self-hit
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero, Mathf.Infinity, GridLayer);
        return hit;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z; // Maintain depth
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
