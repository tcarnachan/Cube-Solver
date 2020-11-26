using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintFacelet : MonoBehaviour
{
    public Vector3 direction;
    public Vector3 offset;

    private MeshRenderer renderer;

    private void Start()
    {
        renderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position + offset, direction, out hit))
        {
            renderer.material.color = hit.collider.GetComponent<MeshRenderer>().material.color;
        }
    }
}
