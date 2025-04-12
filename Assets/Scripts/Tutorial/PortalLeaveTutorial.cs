using System.Diagnostics;
using System;
using Projectiles;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalLeaveTutorial : MonoBehaviour
{

    private void RestartApplication()
    {
        UnityEngine.Debug.Log("Restarting Application...");

        // Get the path to the current executable
        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        // Start a new process to launch the application again
        ProcessStartInfo startInfo = new ProcessStartInfo(exePath);
        startInfo.UseShellExecute = true;

        try
        {
            Process.Start(startInfo);

            // Close the current application
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // In editor, just stop play mode
#else
          Application.Quit(); // In build, exit the application
#endif
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to restart application: {ex.Message}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        // Check if the player enters the trigger zone
        if (other.CompareTag("Player"))
        {
            LeaveTut();
        }
    }

    private void LeaveTut()
    {
        RestartApplication();
    }


}
