using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SolutionLoader : MonoBehaviour
{
    public GameObject stateDisplayPrefab;
    public Transform stateDisplayContent;

    private string username;

    private IEnumerator Start()
    {
        username = FindObjectOfType<ServerManager>().username;
        WWWForm form = new WWWForm();
        form.AddField("name", username);

        WWW www = new WWW("http://localhost:8888/sqlconnect/loadsolutions.php", form);
        yield return www;

        // If there are no solutions
        if(www.text == "No solutions")
            SceneManager.LoadScene("Main");

        // Parse response
        string[] solutions = www.text.TrimEnd('\n').Split('\n');
        var data = new Dictionary<int, (string state, List<string> solutions)>();
        foreach(string s in solutions)
        {
            string[] split = s.TrimEnd('\t').Split('\t');
            int stateID = int.Parse(split[0]);
            if(!data.ContainsKey(stateID))
                data[stateID] = (split[1], new List<string>());
            data[stateID].solutions.Add(split[2]);
        }
        // Display
        ColourManager colourManager = FindObjectOfType<ColourManager>();
        foreach(var kvp in data)
        {
            GameObject go = Instantiate(stateDisplayPrefab, stateDisplayContent);
            (string stateData, var stateSolutions) = kvp.Value;
            go.AddComponent<Button>().onClick.AddListener(() => LoadMainScene(stateData, stateSolutions));
            int ix = 0;
            foreach(Transform face in go.transform)
            {
                foreach(Transform facelet in face)
                {
                    int col = (int)(stateData[ix++] - '0');
                    facelet.GetComponent<Image>().color = colourManager.colours[col];
                }
            }
        }
    }

    private void LoadMainScene(string state, IEnumerable<string> solutions)
    {
        // Set state
        PlayerPrefs.SetString(username, state);
        // Set solutions
        PlayerPrefs.SetString(username + "-solutions", string.Join("\n", solutions));
        // Load main scene
        SceneManager.LoadScene("Main");
    }
}
