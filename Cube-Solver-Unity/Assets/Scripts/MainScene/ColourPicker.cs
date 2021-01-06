﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColourPicker : MonoBehaviour
{
    // Currently selected colour
    private Color selected;
    // Cube face map
    public Transform map;
    // Colour buttons
    public Image[] colours;

    private Color defaultColour;

    public ColourPallette colourPallette;

    private void Awake()
    {
        for (int i = 0; i < colours.Length; i++)
            UpdateColour(colours[i], ColourManager.colours[i]);
    }

    private void Start()
    {
        // Add listeners to all facelets on the cube map
        foreach(Transform face in map)
        {
            foreach (Transform facelet in face)
            {
                if(facelet.name != "4") // Cannot change centre piece
                    facelet.gameObject.AddComponent<Button>().onClick.AddListener(() => PlaceColour(facelet, selected));
            }
        }
        // Add listeners to all colour buttons
        foreach (Image img in colours)
            img.gameObject.AddComponent<ColourSelectionButton>().cp = this;
        // Initialise selected colour
        selected = colours[0].color;
        // Get default colour
        defaultColour = map.GetComponentInChildren<Image>().color;

        // Check for webcam
        string webcam = PlayerPrefs.GetString("webcam", string.Empty);
        if (webcam != string.Empty)
        {
            // Parse colours
            Color[] colours = webcam.Split().Select(i =>
            {
                if(int.Parse(i) >= 0)
                    return ColourManager.colours[int.Parse(i)];
                return defaultColour;
            }).ToArray();
            // Update colours on map
            int ix = 0;
            foreach(Transform face in map)
            {
                foreach (Transform facelet in face)
                {
                    if (facelet.name != "4")
                        facelet.GetComponent<Image>().color = colours[ix++];
                    else
                        ix++;
                }
            }
            // Reset webcam
            PlayerPrefs.SetString("webcam", string.Empty);
        }
    }

    public void SelectColour(Image img)
    {
        selected = img.color;
    }

    private void PlaceColour(Transform img, Color colour)
    {
        img.GetComponent<Image>().color = colour;
    }

    public void ClearColours()
    {
        foreach(Transform face in map)
        {
            for(int i = 0; i < face.childCount; i++)
            {
                if(i != face.childCount / 2)
                    PlaceColour(face.GetChild(i), defaultColour);
            }
        }
    }

    public void SolvedColours()
    {
        foreach(Transform face in map)
        {
            Transform centre = face.GetChild(face.childCount / 2);
            Color colour = centre.GetComponent<Image>().color;
            foreach (Transform facelet in face)
                PlaceColour(facelet, colour);
        }
    }

    public void UpdateColour(Image targetImage, Color newColour)
    {
        foreach(Transform face in map)
        {
            foreach(Transform facelet in face)
            {
                Image img = facelet.GetComponent<Image>();
                if (img.color == targetImage.color)
                    img.color = newColour;
            }
        }
        // Update ColourManager
        int ix = System.Array.IndexOf(ColourManager.colours, targetImage.color);
        if (ix != -1)
            ColourManager.colours[ix] = newColour;
        // Update target image
        targetImage.color = newColour;
    }

    public void SetColours(string colours)
    {
        Dictionary<char, Color> colourLookup = new Dictionary<char, Color>();
        string faces = "ULFRBD";
        for (int i = 0; i < faces.Length; i++)
        {
            Transform f = map.GetChild(i);
            colourLookup[faces[i]] = f.GetChild(f.childCount / 2).GetComponent<Image>().color;
        }

        int ix = 0;
        foreach(Transform face in map)
        {
            foreach (Transform facelet in face)
                PlaceColour(facelet, colourLookup[colours[ix++]]);
        }
    }

    public Color[] GetColours()
    {
        List<Color> colours = new List<Color>();
        foreach(Transform face in map)
        {
            foreach (Transform facelet in face)
                colours.Add(facelet.GetComponent<Image>().color);
        }
        return colours.ToArray();
    }
}
