using UnityEngine;
using UnityEngine.InputSystem; // Essential for using the new Input System

/// <summary>
/// Controls the pausing and unpausing of the game and toggles the visibility
/// of the VR pause menu UI.
/// 
/// How to Use:
/// 1. Create a new GameObject (e.g., 'GameManager') and attach this script.
/// 2. Drag your World Space UI Canvas (the menu) into the 'Pause Menu UI' slot.
/// 3. In the Inspector, assign your Input Action Asset (e.g., 'XRI Default Input Actions')
///    to the 'Menu Toggle Action' slot, and select the specific 'Menu' or 'Primary Button' 
///    action you want to use.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    // --- Public Inspector Fields ---

    [Tooltip("The root GameObject of the pause menu UI canvas. This should be a World Space Canvas.")]
    public GameObject pauseMenuUI;

    [Tooltip("The Input Action Property representing the button used to toggle the menu (e.g., the 'Menu' button on the controller).")]
    public InputActionProperty menuToggleAction;

    // --- Private State ---
    private bool isPaused = false;
    private const float PausedTimeScale = 0f;
    private const float RunningTimeScale = 1f;

    // --- Unity Lifecycle Methods ---

    void Awake()
    {
        // Initialize: Ensure the menu is hidden and the game is running on start
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        Time.timeScale = RunningTimeScale;
        isPaused = false;
    }

    void OnEnable()
    {
        // Subscribe to the input action. This ensures the function is called 
        // only when the button is pressed (performed).
        if (menuToggleAction.action != null)
        {
            menuToggleAction.action.Enable();
            menuToggleAction.action.performed += TogglePause;
        }
    }

    void OnDisable()
    {
        // Important: Unsubscribe when the script is disabled to prevent errors
        if (menuToggleAction.action != null)
        {
            menuToggleAction.action.performed -= TogglePause;
            menuToggleAction.action.Disable();
        }
    }

    // --- Core Logic ---

    /// <summary>
    /// Event handler for the input action. Toggles the pause state.
    /// </summary>
    /// <param name="context">The callback context provided by the Input System (unused but required by the signature).</param>
    private void TogglePause(InputAction.CallbackContext context)
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Resumes the game (Time.timeScale = 1) and hides the UI.
    /// </summary>
    public void ResumeGame()
    {
        if (pauseMenuUI != null)
        {
            // 1. Hide the Menu UI
            pauseMenuUI.SetActive(false);
        }

        // 2. Set the time scale back to normal
        Time.timeScale = RunningTimeScale;
        isPaused = false;

        Debug.Log("Game Resumed.");
        
        // Note: If you have a separate 'Resume' button on your UI, 
        // you can call this public method from that button's OnClick event.
    }

    /// <summary>
    /// Pauses the game (Time.timeScale = 0) and shows the UI.
    /// </summary>
    public void PauseGame()
    {
        if (pauseMenuUI != null)
        {
            // 1. Show the Menu UI
            pauseMenuUI.SetActive(true);
        }

        // 2. Set the time scale to zero to pause all physics and updates
        Time.timeScale = PausedTimeScale;
        isPaused = true;

        Debug.Log("Game Paused. Menu Displayed.");

        // Tip for VR: In the PauseGame state, you will likely want to make sure 
        // your UI Ray Interactor (laser pointer) is enabled so the user can interact 
        // with the menu buttons, and you might disable the Continuous Move Provider
        // to stop player movement.
    }
}