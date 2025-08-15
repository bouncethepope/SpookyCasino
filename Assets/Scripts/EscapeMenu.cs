using UnityEngine;

/// <summary>
/// Toggles a simple menu when the player presses the Escape key.
/// The menu can contain buttons such as Quit Game.
/// </summary>
public class EscapeMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject menuRoot; // Assign the menu UI root object in the inspector.

    private bool isShowing;

    private void Start()
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    /// <summary>
    /// Toggles the menu's visibility.
    /// </summary>
    public void ToggleMenu()
    {
        if (menuRoot == null) return;
        isShowing = !isShowing;
        menuRoot.SetActive(isShowing);
    }

    /// <summary>
    /// Quits the game. Hook this up to the Quit button's OnClick event.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        // This will stop play mode in the editor.
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}