using System;
using UnityEngine;

public class Slider2D : MonoBehaviour
{
    public GameObject handle;
    private bool dragging = false;

    private RectTransform rt;

    public Action<float> OnXChange, OnYChange;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (dragging && rt.rect.Contains(rt.InverseTransformPoint(Input.mousePosition)))
        {
            handle.transform.position = Input.mousePosition;
            Vector2 pos = rt.InverseTransformPoint(handle.transform.position);
            OnXChange((pos.x / rt.rect.width) + .5f);
            OnYChange((pos.y / rt.rect.height) + .5f);
        }
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
