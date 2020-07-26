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

    private void Start()
    {
        // Add listeners to all facelets on the cube map
        foreach(Transform face in map)
        {
            foreach(Transform facelet in face)
                facelet.gameObject.AddComponent<Button>().onClick.AddListener(() => PlaceColour(facelet, selected));
        }
        // Add listeners to all colour buttons
        foreach (Image img in colours)
        {
            img.gameObject.AddComponent<Button>().onClick.AddListener(() => SelectColour(img));
            img.gameObject.AddComponent<OpenColourPallette>().cp = colourPallette;
        }
        // Initialise selected colour
        selected = colours[0].color;
        // Get default colour
        defaultColour = map.GetComponentInChildren<Image>().color;
    }

    private void SelectColour(Image img)
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
            foreach (Transform facelet in face)
                PlaceColour(facelet, defaultColour);
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
        targetImage.color = newColour;
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
