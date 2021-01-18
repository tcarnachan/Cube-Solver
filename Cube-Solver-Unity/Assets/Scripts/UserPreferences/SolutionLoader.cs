using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SolutionLoader : MonoBehaviour
{
    public GameObject stateDisplayPrefab;
    public Transform stateDisplayContent;

    private string username;

    private void Start()
    {
        username = FindObjectOfType<ServerManager>().username;
        StartCoroutine(LoadStates());
    }

    private void LoadMainScene(string state, string[] solutions)
    {
        // Set state
        PlayerPrefs.SetString(username, state);
        // Set solutions
        PlayerPrefs.SetString(username + "-solutions", string.Join("\n", solutions));
        // Load main scene
        SceneManager.LoadScene("Main");
    }

    private IEnumerator LoadStates()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", username);

        WWW www = new WWW("http://localhost:8888/sqlconnect/loadsolutions.php", form);
        yield return www;

        Debug.Log(www.text);
        string[] states = www.text.TrimEnd('\n').Split('\n');
        foreach(string state in states)
        {
            string[] split = state.TrimEnd('\t').Split('\t');
            string stateData = split[0];
            string[] solutions = new string[split.Length - 1];
            Array.Copy(split, 1, solutions, 0, solutions.Length);
            GameObject go = Instantiate(stateDisplayPrefab, stateDisplayContent);
            go.AddComponent<Button>().onClick.AddListener(() => LoadMainScene(stateData, solutions));
        }
    }
}
