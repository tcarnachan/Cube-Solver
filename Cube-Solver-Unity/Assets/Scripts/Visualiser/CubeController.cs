using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    private const int DIM = 3;

    public GameObject cubiePrefab;
    // The cubies giong from bottom to top, left to right, front to back
    private Transform[] cubies;

    private void Start()
    {
        cubies = new Transform[DIM * DIM * DIM];
        for(int i = 0; i < cubies.Length; i++)
        {
            cubies[i] = Instantiate(cubiePrefab, transform).transform;
            cubies[i].localPosition = new Vector3(i % 3, (i / 9) % 3, (i / 3) % 3) - Vector3.one;
        }
    }
}
