using UnityEngine;
using UnityEngine.UI;

public class ColourPallette : MonoBehaviour
{
    public int bgWidth, bgHeight;
    public int palletteWidth, palleteHeight;

    public Slider slider;
    public RawImage background;
    public Image handle;
    
    public RawImage pallette;
    private Texture2D pltTex;
    public Slider2D slider2d;

    public Image oldColour, newColour;
    public Image targetImage;

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

        Color.RGBToHSV(oldColour.color, out h, out s, out v);
        SetH(h);
        slider2d.SetHandlePosition(s, v);
    }

    private void Update()
    {
        newColour.color = Color.HSVToRGB(h, s, v);
    }

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
        targetImage.color = Color.HSVToRGB(h, s, v);
        gameObject.SetActive(false);
    }
}
