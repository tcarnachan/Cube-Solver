using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Cube_Solver.Cubes;
using Cube_Solver.Solver;

/// <summary>
/// In charge of updating the menu, provides functions which are called by the buttons in MainScene
/// </summary>
public class Menu : MonoBehaviour
{
    // In charge of allowing the user to select/update colours
    public ColourPicker colourPicker;
    // Transform for displaying solutions
    public Transform solutionDisplay;
    // Prefab for displaying an algorithm
    public GameObject algText;

    // Variables used for running Search
    private List<Task> tasks;
    private CancellationTokenSource tokenSource;
    private SearchTables st;
    private Symmetries sym;

    // Used for giving the user feedback if an error occurs
    public GameObject errorDisplay;
    public Text errorText;
    private string error = null;

    // Solutions are passed to solutionsQueue from Search and then displayed
    private Queue<string> solutionsQueue = new Queue<string>();
    private Queue<GameObject> solutionText = new Queue<GameObject>();

    // Called when this gameobject is initialised
    private void Start()
    {
        // Load search tables from files
        st = new SearchTables("Assets/Resources/", s => solutionsQueue.Enqueue(s));
        // Initialise symmetries
        sym = new Symmetries();
        // If there were some previously loaded scenes
        string username = FindObjectOfType<ServerManager>().username;
        string solns = PlayerPrefs.GetString(username + "-solutions", null);
        if (solns != null)
        {
            // Display each solutions
            foreach(string solution in solns.Split('\n'))
                DisplaySolution(solution);
            PlayerPrefs.SetString(username + "-solutions", null);
        }
    }

    // Called each frame
    private void Update()
    {
        // If there is a new solution found
        if (solutionsQueue.Count > 0)
            DisplaySolution(solutionsQueue.Dequeue());
        // If there was an error
        if(error != null)
        {
            // Display the error
            errorText.text = error;
            errorDisplay.SetActive(true);
            // Reset error
            error = null;
        }
    }

    private void DisplaySolution(string solution)
    {
        // Instantiate a new gameobject as a child of solutionDisplay
        GameObject alg = Instantiate(algText, solutionDisplay);
        // Set its text field to the new solution
        alg.GetComponent<Text>().text = solution;
        // When the solution is clicked, visualise that solution
        alg.GetComponent<Button>().onClick.AddListener(() =>
        {
            string text = alg.GetComponent<Text>().text;
            // Load the solution to visualise
            PlayerPrefs.SetString("alg", text.Substring(0, text.IndexOf('(') - 1));
            // Store all of the solutions that were found for when this scene is reloaded
            string username = FindObjectOfType<ServerManager>().username;
            List<string> solutions = new List<string>();
            foreach (Transform child in solutionDisplay)
                solutions.Add(child.GetComponent<Text>().text);
            PlayerPrefs.SetString(username + "-solutions", string.Join("\n", solutions));
            // Load the visualiser scene
            SceneManager.LoadScene("Visualiser");
        });
        solutionText.Enqueue(alg);
    }

    // Called by 'Solved' button
    public void Solved()
    {
        colourPicker.SolvedColours();
    }

    // Called by 'Random' button
    public void Random()
    {
        // Generate a random cube
        FaceletCube fc = new FaceletCube(CubieCube.RandomCube());
        // Display the generated cube
        colourPicker.SetColours(fc.ToString().Replace(" ", ""));
    }

    // Called by 'Clear' button
    public void Clear()
    {
        // Stop searching
        EndSolver();
        colourPicker.ClearColours();
    }

    // Called by 'Solve' button
    public void Solve()
    {
        // End previous search
        EndSolver();

        // Get colours from map
        Color[] colours = colourPicker.GetColours();

        // Convert to string
        var unique = colours.Distinct().ToArray();
        Dictionary<Color, char> colour2char = new Dictionary<Color, char>();
        for (int i = 0; i < unique.Length; i++)
            colour2char[unique[i]] = "QWERTY"[i]; // Doesn't actually matter what characters are used
        string state = new string(colours.Select(c => colour2char[c]).ToArray());
        PlayerPrefs.SetString("state", state);

        // Reset variables for search
        tasks = new List<Task>();
        tokenSource = new CancellationTokenSource();
        Search.solutions = new ConcurrentDictionary<string, byte>();

        // Load the CubieCube
        CubieCube cc;
        try
        {
            cc = new CubieCube(new FaceletCube(state));
        }
        catch(System.Exception e)
        {
            error = e.Message;
            return;
        }

        // Chose what to run in parallel
        List<int> nums = Enumerable.Range(0, 6).ToList();
        HashSet<int> syms = sym.GetSymmetries(cc);
        // If the cube has rotational symmetry about the long diagonal
        if (syms.Union(new int[] { 16, 20, 24, 28 }).Count() > 0)
            nums = new List<int> { 0, 3 }; // Only search one rotation and its inverse
        // If the cube has anti-symmetry
        if (syms.Union(Enumerable.Range(Symmetries.N_SYMS, Symmetries.N_SYMS)).Count() > 0)
            nums = nums.Where(i => i < 3).ToList(); // Don't search the inverses

        // Start searching
        foreach(int i in nums)
        {
            bool inverse = i / 3 == 0;
            int rot = i % 3;
            tasks.Add(Task.Run(() =>
            {
                Search search = new Search(st, tokenSource.Token, sym, rot, inverse);
                search.Solve(cc);
            }, tokenSource.Token));
        }
    }

    // Called by 'Log out' button
    public void LogOut()
    {
        EndSolver();
        FindObjectOfType<ServerManager>().LogOut();
    }

    // Stop search
    private void EndSolver()
    {
        // If there is a search, request cancellation
        if (tokenSource != null)
            tokenSource.Cancel();
        // Remove the displayed solutions
        while (solutionText.Count > 0)
            Destroy(solutionText.Dequeue());
    }

    // Called by 'Webcam' button
    public void LoadWebcam()
    {
        SceneManager.LoadScene("Webcam");
    }
}
