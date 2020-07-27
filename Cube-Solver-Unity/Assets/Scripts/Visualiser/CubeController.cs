using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private string algorithmText = "F U' L' B2 L' D2 L' R2 B2 D2 R B2 R' D R2 B L' R2 F2 L'";
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

        InitAlgorithm();
        defaultColour = moveTextPrefab.GetComponent<Text>().color;
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
            return;
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

    IEnumerator RotateLayer(Layer layer, bool cw)
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
        for(float t = 0f; t <= 1f; t += rotSpeed * Time.deltaTime)
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
