using Cube_Solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public ColourPicker colourPicker;
    public Text solutionText;
    private Search search;

    public GameObject errorDisplay;
    public Text errorText;

    private Queue<string> cb = new Queue<string>();
    private string error = null;

    private void Start()
    {
        search = new Search("000000000111111111222222222333333333444444444555555555", "Assets/Resources/", s => cb.Enqueue(s));
    }

    private void Update()
    {
        if (cb.Count > 0)
        {
            if (solutionText.text != "")
                solutionText.text += "\n\n";
            solutionText.text += cb.Dequeue();
        }
        if(error != null)
        {
            errorText.text = error;
            errorDisplay.SetActive(true);
            error = null;
        }
    }

    public void Clear()
    {
        EndSolver();
        colourPicker.ClearColours();
    }

    public void Solve()
    {
        EndSolver();

        // Get colours from map
        Color[] colours = colourPicker.GetColours();

        // Convert to string
        var unique = colours.Distinct().ToArray();
        Dictionary<Color, char> colour2char = new Dictionary<Color, char>();
        for (int i = 0; i < unique.Length; i++)
            colour2char[unique[i]] = (char)i;
        string state = new string(colours.Select(c => colour2char[c]).ToArray());

        // Start solver
        new Thread(() =>
        {
            try
            {
                search.Solve(state);
                // search.Solve("wbrbwgbgyoywoogorbrroogbywybywwrwgbwgobgbygwyroryyrgro");
            }
            catch(Exception e)
            {
                error = e.Message;
            }
        }).Start();
    }

    public void Exit()
    {
        EndSolver();
        Application.Quit();
    }

    private void EndSolver()
    {
        search.exit = true;
        solutionText.text = "";
    }
}
