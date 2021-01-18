using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Displays a 3d animation of the solution
/// </summary>
public class CubeController : MonoBehaviour
{
    private const int DIM = 3;

    public GameObject cubiePrefab;
    // The cubies going from bottom to top, left to right, front to back
    private Transform[] cubies;
    // Pivot for making moves
    private Transform pivot;

    // Moves to execute
    private enum Layer { R, L, U, D, B, F };
    private Queue<(Layer layer, bool cw)> moves = new Queue<(Layer, bool)>();
    // Whether or not the cube is currently executing a move
    private bool rotating = false;
    // Speed to animate rotation
    public float rotSpeed = 5.0f;

    // The algorithm to be executed
    private string algorithmText;
    // Index of nextMove in algorithm
    private int nextMove = 0;
    private string[] algorithm;
    // Variables for displaying the algorithm on screen
    public Transform algorithmDisplay;
    public GameObject moveTextPrefab;

    // Colours used for displaying the algorithm
    private Color defaultColour;
    private Color highlightColour = Color.red;

    // Variables for controlling the visualisation
    private bool playing = false;
    public Sprite play, pause;
    public Image buttonImg;
    public Button step, undo;

    // Materials for the cube colours
    public Material[] cubeMaterials;
    private ColourManager colourManager;

    private ServerManager serverManager;

    private void Start()
    {
        colourManager = FindObjectOfType<ColourManager>();
        serverManager = FindObjectOfType<ServerManager>();

        // Update material colours
        for (int i = 0; i < cubeMaterials.Length; i++)
            cubeMaterials[i].color = colourManager.colours[i];

        // Initialise cubies
        cubies = new Transform[DIM * DIM * DIM];
        for(int i = 0; i < cubies.Length; i++)
        {
            cubies[i] = Instantiate(cubiePrefab, transform).transform;
            cubies[i].localPosition = new Vector3(i % 3, (i / 9) % 3, (i / 3) % 3) - Vector3.one;
        }

        pivot = new GameObject("Pivot").transform;
        pivot.SetParent(transform);

        // Initialise algorithm display
        algorithmText = PlayerPrefs.GetString("alg");
        InitAlgorithm();
        defaultColour = moveTextPrefab.GetComponent<Text>().color;
        Setup(algorithm);
    }

    // Called by 'Save' button
    public void Save()
    {
        StartCoroutine(SaveSolution());
    }

    // Saves the solution to database
    private IEnumerator SaveSolution()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", serverManager.username);
        string solution = algorithmText.Replace("'", "\\'"); // Escape ' characters for sql
        solution += $"({algorithm.Length})"; // Store algorithm length
        form.AddField("solution", solution); 
        form.AddField("state", PlayerPrefs.GetString("state"));

        WWW www = new WWW("http://localhost:8888/sqlconnect/savesolution.php", form);
        yield return www;

        if (www.text != "0")
            Debug.LogError($"Error in saving solution: {www.text}");
        else
            Debug.Log("Success!");
    }

    // Splits up the algorithm string and displays it on screen
    private void InitAlgorithm()
    {
        algorithm = algorithmText.Split();
        foreach(string move in algorithm)
        {
            GameObject moveText = Instantiate(moveTextPrefab, algorithmDisplay);
            moveText.GetComponent<Text>().text = move;
        }
    }

    // Set the initial cube state
    private void Setup(string[] alg)
    {
        foreach(string s in alg.Reverse())
        {
            string inverse = s;
            if (inverse.Length == 1) inverse += "'";
            else if (inverse[1] == '\'') inverse = inverse[0].ToString();
            ParseMove(inverse);
        }

        while(moves.Count > 0)
        {
            var m = moves.Dequeue();
            StartCoroutine(RotateLayer(m.layer, m.cw, false));
        }
    }

    private void Update()
    {
        if (!rotating)
        {
            if (moves.Count > 0)
            {
                var m = moves.Dequeue();
                StartCoroutine(RotateLayer(m.layer, m.cw));
            }
            else if (playing)
                Step();
        }

        // Keyboard controls
        if(!playing)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                Step();
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                Undo();
        }
        if (Input.GetKeyDown(KeyCode.Space))
            Play();
    }

    // Called by undo button
    public void Undo()
    {
        // If there are no moves to undo
        if (nextMove == 0)
            return;
        // Update display
        algorithmDisplay.GetChild(nextMove - 1).GetComponent<Text>().color = defaultColour;
        if(nextMove != 1)
            algorithmDisplay.GetChild(nextMove - 2).GetComponent<Text>().color = highlightColour;

        // Apply inverse move
        string inverse = algorithm[nextMove - 1];
        if (inverse.Length == 1) inverse += "'";
        else if (inverse[1] == '\'') inverse = inverse[0].ToString();
        ParseMove(inverse);

        nextMove--;
    }

    // Called by step button
    public void Step()
    {
        // If there are no more moves to execute
        if (nextMove == algorithm.Length)
        {
            if(playing)
                Play();
            return;
        }
        // Update display
        if(nextMove != 0)
            algorithmDisplay.GetChild(nextMove - 1).GetComponent<Text>().color = defaultColour;
        algorithmDisplay.GetChild(nextMove).GetComponent<Text>().color = highlightColour;
        // Set the next move to be executed
        ParseMove(algorithm[nextMove]);
        nextMove++;
    }

    // Toggles play/pause
    public void Play()
    {
        if (playing)
            buttonImg.sprite = play;
        else
            buttonImg.sprite = pause;

        step.interactable = !step.interactable;
        undo.interactable = !undo.interactable;
        playing = !playing;
    }

    // Parses a move from a string in WCA notation
    private void ParseMove(string move)
    {
        Layer layer = (Layer)"RLUDBF".IndexOf(move[0]);
        if (move.Length == 1)
            moves.Enqueue((layer, true));
        else if (move[1] == '2')
        {
            moves.Enqueue((layer, true));
            moves.Enqueue((layer, true));
        }
        else
            moves.Enqueue((layer, false));
    }

    // Called by 'Back' button
    public void Back()
    {
        SceneManager.LoadSceneAsync("Main");
    }

    // Animate rotating a layer on the cube
    IEnumerator RotateLayer(Layer layer, bool cw, bool animate = true)
    {
        rotating = true;

        // Reset pivot
        pivot.localPosition = Vector3.zero;
        pivot.localRotation = Quaternion.identity;
        // Set cubies to be children of the pivot
        int axis = (int)layer / 2;
        int mul = 1 - ((int)layer % 2) * 2;
        foreach(Transform cubie in cubies)
        {
            if (Mathf.RoundToInt(cubie.position[axis]) == mul)
                cubie.SetParent(pivot);
        }
        // Rotate the layer
        Vector3 rotAxis = Vector3.zero;
        rotAxis[axis] = mul;
        Quaternion target = Quaternion.AngleAxis(90 * (cw ? 1 : -1), rotAxis);
        for(float t = 0f; t <= 1f && animate; t += rotSpeed * Time.deltaTime)
        {
            pivot.localRotation = Quaternion.Slerp(Quaternion.identity, target, t);
            yield return null;
        }
        // Snap the rotation into place
        pivot.localRotation = target;
        // Unparent cubies
        while(pivot.childCount > 0)
            pivot.GetChild(0).SetParent(transform);

        rotating = false;
    }
}
