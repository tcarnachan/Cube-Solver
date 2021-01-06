using System.Diagnostics;
using UnityEngine.SceneManagement;

public static class DBManager
{
    public static string username { get; private set; }

    public static bool loggedIn { get => !string.IsNullOrEmpty(username); }

    public static bool serverStarted = false;

    public static void LogIn(string username)
    {
        DBManager.username = username;
        SceneManager.LoadScene("Main");
    }

    public static void LogOut()
    {
        username = null;
        SceneManager.LoadScene("Login");
    }

    public static void StartServer()
    {
        if (serverStarted) return;
        ExecuteCommand("cd /Applications/MAMP/bin;./start.sh");
        serverStarted = true;
    }

    public static void StopServer()
    {
        if (!serverStarted) return;
        ExecuteCommand("cd /Applications/MAMP/bin;./stop.sh");
        serverStarted = false;
    }

    private static void ExecuteCommand(string cmd)
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
