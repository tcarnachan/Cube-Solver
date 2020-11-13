using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System;

public class UpdateCubeState : MonoBehaviour
{
    public Transform map;
    public ColourFilter cf;

    private int currFace = 0;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Color[] colours = cf.GetColours();
            Transform face = map.GetChild(currFace++);
            for (int i = 0; i < colours.Length; i++)
                face.GetChild(i).GetComponent<Image>().color = colours[i];

            if (currFace == map.childCount)
            {
                // Convert colours to string
                string cube = "";
                foreach (Transform f in map)
                {
                    foreach (Transform facelet in f)
                    {
                        Color colour = facelet.GetComponent<Image>().color;
                        cube += $"{Array.IndexOf(ColourManager.instance.colours, colour)} ";
                    }
                }
                // Remove extra space
                cube = cube.Substring(0, cube.Length - 1);

                PlayerPrefs.SetString("webcam", cube);
                SceneManager.LoadScene("Main");
            }
        }
    }
}
