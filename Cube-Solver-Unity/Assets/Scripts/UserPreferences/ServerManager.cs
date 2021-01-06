using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

class ServerManager : MonoBehaviour
{
    public string username { get; private set; }

    public bool loggedIn { get => !string.IsNullOrEmpty(username); }

    // Start server and go to login scene when application is launched
    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // Keep the server manager between scenes
        ExecuteCommand("cd /Applications/MAMP/bin;./start.sh");
        SceneManager.LoadScene("Login");
    }

    // Stop server when application is closed
    private void OnApplicationQuit()
    {
        ExecuteCommand("cd /Applications/MAMP/bin;./stop.sh");
        UnityEngine.Debug.Log("Stopped server");
    }

    public void LogIn(string username)
    {
        this.username = username;
        SceneManager.LoadScene("Main");
    }

    public void LogOut()
    {
        username = null;
        SceneManager.LoadScene("Login");
    }

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
