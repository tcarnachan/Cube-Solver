using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows the user to create a custom colour
/// </summary>
public class ColourPallette : MonoBehaviour
{
    // Dimensions of the colour pallette menu
    public int bgWidth, bgHeight;
    // Dimensions of the colour pallette
    public int palletteWidth, palleteHeight;

    // Slide allowing the user to select the hue
    public Slider slider;
    public RawImage background;
    public Image handle;

    // 'Slider2D' allowing the user to select saturation and value
    public RawImage pallette;
    private Texture2D pltTex;
    public Slider2D slider2d;

    // Previous colour and currently selected colour
    public Image oldColour, newColour;
    public Image targetImage;

    public ColourPicker cp;

    private float h, s, v;

    private void OnEnable()
    {
        // Initialise slider background
        Texture2D tex = new Texture2D(bgWidth, bgHeight);
        background.texture = tex;

        for(int y = 0; y < tex.height; y++)
        {
            for(int x = 0; x < tex.width; x++)
                tex.SetPixel(x, y, Color.HSVToRGB((float)x / tex.width, 1.0f, 1.0f));
        }
        tex.Apply();

        // Initialise pallette
        pltTex = new Texture2D(palletteWidth, palleteHeight);
        pallette.texture = pltTex;
        slider2d.OnXChange = (f) => { s = f; };
        slider2d.OnYChange = (f) => { v = f; };

        // Set the slider positions to select the previous colour
        Color.RGBToHSV(oldColour.color, out h, out s, out v);
        SetH(h);
        slider2d.SetHandlePosition(s, v);
    }

    private void Update()
    {
        newColour.color = Color.HSVToRGB(h, s, v);
    }

    // Set hue
    public void SetH(float value)
    {
        slider.value = value;
        handle.color = Color.HSVToRGB(value, 1.0f, 1.0f);
        h = value;

        for(int y = 0; y < palleteHeight; y++)
        {
            for (int x = 0; x < palletteWidth; x++)
                pltTex.SetPixel(x, y, Color.HSVToRGB(h, (float)x / palletteWidth, (float)y / palleteHeight));
        }
        pltTex.Apply();
    }

    public void Close()
    {
        cp.UpdateColour(targetImage, Color.HSVToRGB(h, s, v));
        gameObject.SetActive(false);
    }
}
