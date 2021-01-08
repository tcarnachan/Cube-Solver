using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Stores the current colour scheme, in charge of saving and loading
/// colour schemes to and from the database server
/// </summary>
class ColourManager : MonoBehaviour
{
    // Colour scheme
    public Color[] colours = new Color[]
    {
        new Color(1, 1, 0),  // Yellow
        new Color(0, 1, 0),  // Green
        new Color(1, 0, 0),  // Red
        new Color(1, 1, 1),  // White
        new Color(0, 0, 1),  // Blue
        new Color(1, .5f, 0) // Orange
    };

    private ServerManager serverManager;

    private void Start()
    {
        serverManager = FindObjectOfType<ServerManager>();
    }

    // Save colour scheme to server
    public IEnumerator SaveColours()
    {
        WWWForm form = new WWWForm();
        for (int i = 0; i < colours.Length; i++)
            form.AddField($"colour{i + 1}", Col2Str(colours[i]));
        form.AddField("name", serverManager.username);

        WWW www = new WWW("http://localhost:8888/sqlconnect/savecolour.php", form);
        yield return www;

        if (www.text != "0")
            Debug.LogError($"Error in updating colours: {www.text}");
        else
            Debug.Log("Success!");
    }

    // Load colour scheme from server
    public IEnumerator LoadColours(Action<List<Color[]>> DisplayColours)
    {
        WWWForm form = new WWWForm();
        form.AddField("name", serverManager.username);

        WWW www = new WWW("http://localhost:8888/sqlconnect/loadcolour.php", form);
        yield return www;

        string[] results = www.text.TrimEnd('\n').Split('\n');
        List<Color[]> colourSchemes = new List<Color[]>();
        foreach (string s in results)
            colourSchemes.Add(s.TrimEnd('\t').Split('\t').Select(Str2Col).ToArray());
        DisplayColours(colourSchemes);
    }

    // Encode the colour as a string
    private static string Col2Str(Color colour)
    {
        return Convert.ToBase64String(new byte[]
        {
            (byte)(colour.r * 255),
            (byte)(colour.g * 255),
            (byte)(colour.b * 255),
        });
    }

    // Decode the colour from a string
    private static Color Str2Col(string s)
    {
        byte[] col = Convert.FromBase64String(s);
        return new Color(
            col[0] / 255.0f,
            col[1] / 255.0f,
            col[2] / 255.0f
        );
    }
}
