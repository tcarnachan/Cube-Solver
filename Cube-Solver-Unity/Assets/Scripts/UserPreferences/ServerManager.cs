using System.Diagnostics;
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
    private void Awake()
    {
        // Keep the server manager between scenes
        DontDestroyOnLoad(gameObject);
        // Start server
        ExecuteCommand("cd /Applications/MAMP/bin;./start.sh");
        // Load login scene
        SceneManager.LoadScene("Login");
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
