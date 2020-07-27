using Cube_Solver.Cubes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CubeController : MonoBehaviour
{
    private const int DIM = 3;

    public GameObject cubiePrefab;
    // The cubies giong from bottom to top, left to right, front to back
    private Transform[] cubies;
    // Pivot for making moves
    private Transform pivot;

    // Moves to execute
    private enum Layer { R, L, U, D, B, F };
    private Queue<(Layer layer, bool cw)> moves = new Queue<(Layer, bool)>();
    // Whether or not the cube is currently executing a move
    private bool rotating = false;
    // Speed to animate rotation
    private float rotSpeed = 5.0f;

    private string algorithmText;
    private int nextMove = 0;
    private string[] algorithm;
    public Transform algorithmDisplay;
    public GameObject moveTextPrefab;

    private Color defaultColour;
    private Color highlightColour = Color.red;

    private bool playing = false;
    public Sprite play, pause;
    public Image buttonImg;
    public Button step, undo;

    private void Start()
    {
        cubies = new Transform[DIM * DIM * DIM];
        for(int i = 0; i < cubies.Length; i++)
        {
            cubies[i] = Instantiate(cubiePrefab, transform).transform;
            cubies[i].localPosition = new Vector3(i % 3, (i / 9) % 3, (i / 3) % 3) - Vector3.one;
        }

        pivot = new GameObject("Pivot").transform;
        pivot.SetParent(transform);

        algorithmText = PlayerPrefs.GetString("alg");
        InitAlgorithm();
        defaultColour = moveTextPrefab.GetComponent<Text>().color;

        Setup(algorithm);
    }

    private void InitAlgorithm()
    {
        algorithm = algorithmText.Split();
        foreach(string move in algorithm)
        {
            GameObject moveText = Instantiate(moveTextPrefab, algorithmDisplay);
            moveText.GetComponent<Text>().text = move;
        }
    }

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

    public void Undo()
    {
        if (nextMove == 0)
            return;
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

    public void Step()
    {
        if (nextMove == algorithm.Length)
        {
            if(playing)
                Play();
            return;
        }
        if(nextMove != 0)
            algorithmDisplay.GetChild(nextMove - 1).GetComponent<Text>().color = defaultColour;
        algorithmDisplay.GetChild(nextMove).GetComponent<Text>().color = highlightColour;
        ParseMove(algorithm[nextMove]);
        nextMove++;
    }

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

    public void Back()
    {
        SceneManager.LoadSceneAsync("Main");
    }

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
