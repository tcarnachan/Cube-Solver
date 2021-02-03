using System.Diagnostics;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class in charge of starting and stopping the database server
/// </summary>
class ServerManager : MonoBehaviour
{
    public string username { get; private set; }

    public bool loggedIn { get => !string.IsNullOrEmpty(username); }

    // Start server and go to login scene when application is launched
    private IEnumerator Start()
    {
        // Keep the server manager between scenes
        DontDestroyOnLoad(gameObject);
        // Start server
        ExecuteCommand("cd /Applications/MAMP/bin;./start.sh");
        // Load login scene
        SceneManager.LoadSceneAsync("Login");
        // Initialise database at the same time
        WWW www = new WWW("http://localhost:8888/sqlconnect/initdatabase.php");
        yield return www;

        if (www.text != "0")
            UnityEngine.Debug.LogError($"Error in initialising database: {www.text}");
        else
            UnityEngine.Debug.Log("Database initialised");
    }

    // Stop server when application is closed
    private void OnApplicationQuit()
    {
        ExecuteCommand("cd /Applications/MAMP/bin;./stop.sh");
        UnityEngine.Debug.Log("Stopped server");
    }

    // Set username and load main scene
    public void LogIn(string username)
    {
        this.username = username;
        SceneManager.LoadScene("Main");
    }

    // Reset username and load login scene
    public void LogOut()
    {
        username = null;
        SceneManager.LoadScene("Login");
    }

    // Execute a terminal command
    private void ExecuteCommand(string cmd)
    {
        Process process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{cmd}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        process.WaitForExit();
    }

}
