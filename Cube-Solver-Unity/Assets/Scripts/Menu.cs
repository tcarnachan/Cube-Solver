using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public ColourPicker colourPicker;

    public void Clear()
    {
        colourPicker.ClearColours();
    }

    public void Solve()
    {

    }

    public void Exit()
    {
        Application.Quit();
    }
}
