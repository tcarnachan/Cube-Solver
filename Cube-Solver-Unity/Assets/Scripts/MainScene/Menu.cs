using Cube_Solver.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public ColourPicker colourPicker;
    public Transform solutionDisplay;
    public GameObject algText;
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
            GameObject alg = Instantiate(algText, solutionDisplay);
            alg.GetComponent<Text>().text = cb.Dequeue();
            alg.GetComponent<Button>().onClick.AddListener(() =>
            {
                string text = alg.GetComponent<Text>().text;
                PlayerPrefs.SetString("alg", text.Substring(0, text.IndexOf('(') - 1));
                SceneManager.LoadScene("Visualiser");
            });
        }
        if(error != null)
        {
            errorText.text = error;
            errorDisplay.SetActive(true);
            error = null;
        }
    }

    public void Solved()
    {
        colourPicker.SolvedColours();
    }

    public void Random()
    {
        colourPicker.SetColours(search.RandomCube().Replace(" ", ""));
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
        while (solutionDisplay.childCount > 0)
            Destroy(solutionDisplay.GetChild(0).gameObject);
    }
}
