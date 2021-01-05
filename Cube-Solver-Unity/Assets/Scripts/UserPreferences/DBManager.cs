using System.Diagnostics;
using UnityEngine.SceneManagement;

public static class DBManager
{
    private static string username;

    public static bool loggedIn { get => !string.IsNullOrEmpty(username); }

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
        ExecuteCommand("cd /Applications/MAMP/bin;./start.sh");
    }

    public static void StopServer()
    {
        ExecuteCommand("cd /Applications/MAMP/bin;./stop.sh");
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
