using System;
using UnityEngine;

/// <summary>
/// Allows the user to select a point in a square (where the x
/// position is the saturation and y position is the value)
/// </summary>
public class Slider2D : MonoBehaviour
{
    // The handle the user drags around
    public RectTransform handle;
    private bool dragging = false;

    // The rectange of the square
    private RectTransform rt;

    // Set by ColourPallette, called when the handle is dragged
    public Action<float> OnXChange, OnYChange;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // If dragging and mouse is within the square
        if (dragging && rt.rect.Contains(rt.InverseTransformPoint(Input.mousePosition)))
        {
            // Update handle position
            handle.position = Input.mousePosition;
            // Call methods set by ColourPallette
            Vector2 pos = rt.InverseTransformPoint(handle.position);
            OnXChange((pos.x / rt.rect.width) + .5f);
            OnYChange((pos.y / rt.rect.height) + .5f);
        }
    }

    // x, y in range [0, 1]
    public void SetHandlePosition(float x, float y)
    {
        handle.localPosition = new Vector2(
            (x - .5f) * (rt.rect.width - handle.rect.width),
            (y - .5f) * (rt.rect.height - handle.rect.height)
        );
    }

    // Set dragging when mouse is pressed and released
    public void MouseDown() => dragging = true;
    public void MouseUp() => dragging = false;
}
