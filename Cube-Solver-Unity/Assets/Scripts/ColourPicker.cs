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

    private void Start()
    {
        // Add listeners to all facelets on the cube map
        foreach(Transform face in map)
        {
            foreach(Transform facelet in face)
                facelet.gameObject.AddComponent<Button>().onClick.AddListener(() => PlaceColour(facelet));
        }
        // Add listeners to all colour buttons
        foreach (Image img in colours)
            img.gameObject.AddComponent<Button>().onClick.AddListener(() => SelectColour(img));
        // Initialise selected colour
        selected = colours[0].color;
    }

    private void SelectColour(Image img)
    {
        selected = img.color;
    }

    private void PlaceColour(Transform img)
    {
        img.GetComponent<Image>().color = selected;
    }
}
