using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Code for reading a cube state from the webcam
/// </summary>
public class ColourFilter : MonoBehaviour
{
    public RawImage ri, overlay;

    private WebCamTexture webCam;

    public int captureWidth;
    public int padding;
    private int dim = 3;

    private List<Color> captureColours = new List<Color>();

    public Transform map;
    private int currFace = 0;

    private ColourManager colourManager;

    private void Awake()
    {
        webCam = new WebCamTexture();
        webCam.requestedFPS = 120;
        ri.texture = webCam;
        webCam.Play();

        colourManager = FindObjectOfType<ColourManager>();
    }

    private void Update()
    {
        GetCaptureRegions();

        // Update cube state when space bar is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Color[] colours = captureColours.ToArray();
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
                        cube += $"{Array.IndexOf(colourManager.colours, colour)} ";
                    }
                }
                // Remove extra space
                cube = cube.Substring(0, cube.Length - 1);

                PlayerPrefs.SetString("webcam", cube);
                SceneManager.LoadScene("Main");
            }
        }
    }

    private void GetCaptureRegions()
    {
        WebCamTexture tex = (WebCamTexture)ri.texture;

        // Check bounds
        if (tex.width - captureWidth * dim - padding * (dim - 1) < 0 ||
            tex.height - captureWidth * dim - padding * (dim - 1) < 0)
            return;

        // Initialise texture
        Color[] colours = tex.GetPixels();
        Texture2D newTex = new Texture2D(tex.width, tex.height);
        newTex.SetPixels(colours);

        // Get capture regions
        List<(int r, int c)> captureRegions = new List<(int r, int c)>();
        int regionWidth = captureWidth * dim + padding * (dim - 1);

        int top = (tex.height + regionWidth / 2) / 2;
        for (int i = 0; i < dim; i++)
        {
            int left = (tex.width - regionWidth) / 2;
            for (int j = 0; j < dim; j++)
            {
                captureRegions.Add((left, top));
                left += captureWidth + padding;
            }
            top -= captureWidth + padding;

        }

        // Clear previous captured colours
        captureColours.Clear();
        // tl represents the top left corner of each capture region
        foreach (var tl in captureRegions)
        {
            // Get the colours in that region
            Color[] regionColours = newTex.GetPixels(tl.r, tl.c, captureWidth, captureWidth);
            Color[] nColours = regionColours.Select(c => GetColour(c)).ToArray();

            // Count the occurrences of each colour in that region
            Dictionary<Color, int> counter = new Dictionary<Color, int>();
            foreach (Color c in nColours)
            {
                if (counter.ContainsKey(c)) counter[c]++;
                else counter[c] = 1;
            }

            // Get the colour which occurs the most often
            int max = 0;
            Color col = Color.black;
            foreach (var kvp in counter)
            {
                if (kvp.Value > max && kvp.Key != Color.black)
                {
                    max = kvp.Value;
                    col = kvp.Key;
                }
            }
            // If the most common colour takes up less than 50% of the capture region
            // assume that there is not a section of the cube in that capture region
            if ((float)max / nColours.Length < .5f)
                col = new Color(0, 0, 0, .2f);

            for (int i = 0; i < nColours.Length; i++)
                nColours[i] = col;
            captureColours.Add(col);

            // Update the texture
            newTex.SetPixels(tl.r, tl.c, captureWidth, captureWidth, nColours);
        }

        // Apply changes to the texture
        newTex.Apply();
        overlay.texture = newTex;
    }

    // See if the colour of a pixel could be a colour on the cube
    private Color GetColour(Color colour)
    {
        float h, s, v;
        Color.RGBToHSV(colour, out h, out s, out v);

        // White
        if (s <= .2f && v >= .3f)
            return colourManager.colours[3];
        // Yellow
        else if (.15f <= h && h <= .22f)
            return colourManager.colours[0];
        // Blue
        else if (.55f <= h && h <= .7f)
            return colourManager.colours[4];
        // Green
        else if (.33f <= h && h <= .42f)
            return colourManager.colours[1];
        // Red and orange are fun
        else if (h <= .03f || h >= .97f)
        {
            // Red
            if (s < .55f || v < .55f)
                return colourManager.colours[2];
            // Orange
            return colourManager.colours[5];
        }

        // Not a colour on the cube
        return Color.black;
    }
}
