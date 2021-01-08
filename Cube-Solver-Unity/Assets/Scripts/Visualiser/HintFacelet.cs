using UnityEngine;

/// <summary>
/// Used to display the faces which would not normally be visible
/// </summary>
public class HintFacelet : MonoBehaviour
{
    public Vector3 direction, offset;
    private MeshRenderer renderer;

    private void Update()
    {
        if (!renderer) renderer = GetComponent<MeshRenderer>();
        RaycastHit hit;
        if(Physics.Raycast(transform.position + offset, direction, out hit))
            renderer.material.color = hit.collider.GetComponent<MeshRenderer>().material.color;
    }
}
