using System;
using UnityEngine;

public class Slider2D : MonoBehaviour
{
    public RectTransform handle;
    private bool dragging = false;

    private RectTransform rt;

    public Action<float> OnXChange, OnYChange;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (dragging && rt.rect.Contains(rt.InverseTransformPoint(Input.mousePosition)))
        {
            handle.position = Input.mousePosition;
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

    public void MouseDown()
    {
        dragging = true;
    }

    public void MouseUp()
    {
        dragging = false;
    }
}
