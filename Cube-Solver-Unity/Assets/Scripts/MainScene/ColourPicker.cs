using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles allowing the user to select and update the colours of the cube
/// </summary>
public class ColourPicker : MonoBehaviour
{
    // Currently selected colour
    private Color selected;
    // Cube face map
    public Transform map;
    // Colour buttons
    public Image[] colours;

    // Clear and load state buttons
    public GameObject clearButton, loadStateButton;
    private bool isClear = true;
    public bool IsClear
    {
        get { return isClear; }
        set
        {
            isClear = value;
            clearButton.SetActive(!isClear);
            loadStateButton.SetActive(isClear);
        }
    }

    private Color defaultColour;

    // References to GameObjects in scene
    public ColourPallette colourPallette;
    public GameObject colourDisplay, colourDisplayObj;
    public Transform colourDisplayContent;

    private ServerManager serverManager;
    private ColourManager colourManager;

    // Called when this gameobject is initialised, but before start
    private void Awake()
    {
        serverManager = FindObjectOfType<ServerManager>();
        colourManager = FindObjectOfType<ColourManager>();
        // Update colours based on the colours stored in colourManager
        for (int i = 0; i < colours.Length; i++)
            UpdateColour(colours[i], colourManager.colours[i]);
    }

    // Called when this gameobject is initialised, but after awake
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

        IsClear = false;
        // Check for webcam
        string webcam = PlayerPrefs.GetString("webcam", string.Empty);
        if (webcam != string.Empty)
        {
            // Parse colours
            Color[] colours = webcam.Split().Select(i =>
            {
                if(int.Parse(i) >= 0)
                    return colourManager.colours[int.Parse(i)];
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
        else
        {
            // Load previously saved cubestate
            string state = PlayerPrefs.GetString(serverManager.username, null);
            if (state != null)
            {
                int ix = 0;
                foreach (Transform face in map)
                {
                    foreach (Transform facelet in face)
                    {
                        int t = state[ix++] - '0';
                        if (t < colourManager.colours.Length)
                            facelet.GetComponent<Image>().color = colourManager.colours[t];
                    }
                }
            }
            else
                IsClear = true;
        }
    }

    // Save cube state when leaving scene
    private void OnDestroy()
    {
        Color[] colours = GetColours();
        string s = "";
        foreach(Color colour in colours)
        {
            int ix = System.Array.IndexOf(colourManager.colours, colour);
            if (ix == -1) ix = 6;
            s += ix;
        }
        PlayerPrefs.SetString(serverManager.username, s);
    }

    // Update selected
    public void SelectColour(Image img) => selected = img.color;
    // Update the colour of an image
    private void PlaceColour(Transform img, Color colour)
    {
        img.GetComponent<Image>().color = colour;
        IsClear &= (colour == defaultColour);
    }

    // Reset the cube colours
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
        IsClear = true;
    }

    // Display a solved cube
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

    // Allow the user to customise their colour scheme
    public void UpdateColour(Image targetImage, Color newColour)
    {
        // Update all the objects in scene
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
        int ix = System.Array.IndexOf(colourManager.colours, targetImage.color);
        if (ix != -1)
            colourManager.colours[ix] = newColour;
        // Update target image
        targetImage.color = newColour;
    }

    // Save the colour scheme to the database
    public void SaveColours()
    {
        StartCoroutine(colourManager.SaveColours());
    }

    // Load colour schemes from database
    public void LoadColours()
    {
        StartCoroutine(colourManager.LoadColours(x =>
        {
            // Initialise colour display
            colourDisplay.SetActive(true);
            foreach(Color[] colours in x)
            {
                Transform dispTrans = Instantiate(colourDisplayObj, colourDisplayContent).transform;
                for (int i = 0; i < colours.Length; i++)
                    dispTrans.GetChild(i).GetComponent<Image>().color = colours[i];
                Color[] temp = colours;
                // When a colour scheme is selected
                dispTrans.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    // Update colours
                    for(int i = 0; i < temp.Length; i++)
                        UpdateColour(this.colours[i], temp[i]);
                    // Remove colour display
                    for(int i = colourDisplayContent.childCount - 1; i >= 0; i--) Destroy(colourDisplayContent.GetChild(i).gameObject);
                    colourDisplay.SetActive(false);
                });
            }
        }));
    }

    // Set the colours of the cube
    public void SetColours(string colours)
    {
        // Dictionary to convert from the string to Colors
        Dictionary<char, Color> colourLookup = new Dictionary<char, Color>();
        string faces = "ULFRBD";
        for (int i = 0; i < faces.Length; i++)
        {
            Transform f = map.GetChild(i);
            colourLookup[faces[i]] = f.GetChild(f.childCount / 2).GetComponent<Image>().color;
        }
        // Update the cube colours on screen
        int ix = 0;
        foreach(Transform face in map)
        {
            foreach (Transform facelet in face)
                PlaceColour(facelet, colourLookup[colours[ix++]]);
        }
    }

    // Get the colours of the cube in scene
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

    // Used in Menu
    public int IndexOf(Color colour)
    {
        return System.Array.IndexOf(colourManager.colours, colour);
    }
}
