using Cube_Solver.Cubes;
using Cube_Solver.Solver;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public ColourPicker colourPicker;
    public Transform solutionDisplay;
    public GameObject algText;

    private List<Task> tasks;
    private CancellationTokenSource tokenSource;
    private SearchTables st;
    private Symmetries sym;

    public GameObject errorDisplay;
    public Text errorText;

    private Queue<string> cb = new Queue<string>();
    private Queue<GameObject> solutionText = new Queue<GameObject>();
    private string error = null;

    private void Start()
    {
        st = new SearchTables("Assets/Resources/", s => cb.Enqueue(s));
        sym = new Symmetries();
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
            solutionText.Enqueue(alg);
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
        FaceletCube fc = new FaceletCube(CubieCube.RandomCube());
        colourPicker.SetColours(fc.ToString().Replace(" ", ""));
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
        if (syms.Union(new int[] { 16, 20, 24, 28 }).Count() > 0) // The cube has rotational symmetry about the long diagonal
            nums = new List<int> { 0, 3 }; // Only search one rotation and its inverse
        if (syms.Union(Enumerable.Range(Symmetries.N_SYMS, Symmetries.N_SYMS)).Count() > 0) // The cube has anti-symmetry
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

    public void Exit()
    {
        EndSolver();
        Application.Quit();
    }

    private void EndSolver()
    {
        if (tokenSource != null)
            tokenSource.Cancel();
        while (solutionText.Count > 0)
            Destroy(solutionText.Dequeue());
    }

    public void LoadWebcam()
    {
        SceneManager.LoadScene("Webcam");
    }
}
