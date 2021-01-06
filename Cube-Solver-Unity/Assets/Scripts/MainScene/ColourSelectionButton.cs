using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColourSelectionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public ColourPicker cp;
    private Image img;

    private void Start()
    {
        img = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Darken the button to makes it obvious when button is pressed
        Color colour = img.color;
        colour.a = .9f;      // Can't directly set the alpha channel of img.color
        img.color = colour;  // but I can set img.color to be a new colour
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Reset button colour
        Color colour = img.color;
        colour.a = 1;
        img.color = colour;

        switch (eventData.button)
        {
            // Left-click to select the colour
            case PointerEventData.InputButton.Left:
                cp.SelectColour(img);
                break;
            // Right-click to update the colour
            case PointerEventData.InputButton.Right:
                cp.colourPallette.oldColour.color = colour;
                cp.colourPallette.targetImage = img;
                cp.colourPallette.gameObject.SetActive(true);
                break;
        }
    }
}
