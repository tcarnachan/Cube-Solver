using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public static class ColourManager
{
    public static Color[] colours = new Color[]
    {
        new Color(1, 1, 0),  // Yellow
        new Color(0, 1, 0),  // Green
        new Color(1, 0, 0),  // Red
        new Color(1, 1, 1),  // White
        new Color(0, 0, 1),  // Blue
        new Color(1, .5f, 0) // Orange
    };

    public static IEnumerator SaveColours()
    {
        if (!DBManager.serverStarted)
            DBManager.StartServer();

        WWWForm form = new WWWForm();
        for (int i = 0; i < colours.Length; i++)
            form.AddField($"colour{i + 1}", Col2Str(colours[i]));
        form.AddField("name", DBManager.username);

        WWW www = new WWW("http://localhost:8888/sqlconnect/savecolour.php", form);
        yield return www;

        DBManager.StopServer();

        if (www.text != "0")
            Debug.LogError($"Error in updating colours: {www.text}");
        else
            Debug.Log("Success!");
    }

    public static IEnumerator LoadColours(Action<List<Color[]>> DisplayColours)
    {
        if (!DBManager.serverStarted)
            DBManager.StartServer();

        WWWForm form = new WWWForm();
        form.AddField("name", DBManager.username);

        WWW www = new WWW("http://localhost:8888/sqlconnect/loadcolour.php", form);
        yield return www;

        string[] results = www.text.TrimEnd('\n').Split('\n');
        List<Color[]> colourSchemes = new List<Color[]>();
        foreach (string s in results)
            colourSchemes.Add(s.TrimEnd('\t').Split('\t').Select(Str2Col).ToArray());
        DisplayColours(colourSchemes);

        DBManager.StopServer();
    }

    private static string Col2Str(Color colour)
    {
        return Convert.ToBase64String(new byte[]
        {
            (byte)(colour.r * 255),
            (byte)(colour.g * 255),
            (byte)(colour.b * 255),
        });
    }

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
