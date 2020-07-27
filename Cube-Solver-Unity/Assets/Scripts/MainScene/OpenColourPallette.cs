using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OpenColourPallette : MonoBehaviour, IPointerClickHandler
{
    public ColourPallette cp;
    private Image img;

    private void Start()
    {
        img = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            cp.oldColour.color = img.color;
            cp.targetImage = img;
            cp.gameObject.SetActive(true);
        }
    }
}
