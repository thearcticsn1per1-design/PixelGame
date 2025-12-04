using UnityEngine;
using PixelGame;

/// <summary>
/// Simple script to start a test game run
/// Attach this to GameManager for testing
/// </summary>
public class TestGameStarter : MonoBehaviour
{
    [Header("Test Configuration")]
    public CharacterClass testClass;

    [Header("Debug")]
    public bool debugMode = true;

    void Start()
    {
        if (debugMode)
        {
            Debug.Log("TestGameStarter.Start(): Script is running!");
        }

        if (testClass == null)
        {
            Debug.LogError("TestGameStarter: No test class assigned! Please assign TestWarrior in the Inspector.");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("TestGameStarter: GameManager.Instance is NULL! Make sure GameManager exists in scene.");
            return;
        }

        Debug.Log($"TestGameStarter: Starting new run with class: {testClass.className}");
        GameManager.Instance.StartNewRun(testClass);
    }
}
