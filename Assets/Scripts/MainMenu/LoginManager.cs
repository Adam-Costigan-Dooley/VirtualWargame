using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button signUpButton;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private async void Start()
    {
        await InitializeUnityServices();

        loginButton.onClick.AddListener(OnLoginButtonClicked);
        signUpButton.onClick.AddListener(OnSignUpButtonClicked);

        SetStatusText("", Color.white);
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            SetStatusText("Initializing...", Color.white);
            
            await UnityServices.InitializeAsync();
            
            SetStatusText("Ready to login", Color.green);
            Debug.Log("Unity Services initialized successfully");
        }
        catch (System.Exception e)
        {
            SetStatusText("Initialization failed", Color.red);
            Debug.LogError($"Unity Services initialization failed: {e.Message}");
        }
    }

    private async void OnLoginButtonClicked()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username))
        {
            SetStatusText("Please enter a username", Color.red);
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetStatusText("Please enter a password", Color.red);
            return;
        }

        SetButtonsInteractable(false);
        SetStatusText("Logging in...", Color.white);

        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            
            SetStatusText("Login successful!", Color.green);
            Debug.Log($"Logged in as: {AuthenticationService.Instance.PlayerId}");
            
            await Task.Delay(1000);
            LoadMainMenu();
        }
        catch (AuthenticationException ex)
        {
            SetStatusText($"Login failed: {ex.Message}", Color.red);
            Debug.LogError($"Login error: {ex.Message}");
            SetButtonsInteractable(true);
        }
        catch (System.Exception ex)
        {
            SetStatusText("Login failed. Please try again.", Color.red);
            Debug.LogError($"Unexpected error: {ex.Message}");
            SetButtonsInteractable(true);
        }
    }

    private async void OnSignUpButtonClicked()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username))
        {
            SetStatusText("Please enter a username", Color.red);
            return;
        }

        if (username.Length < 3)
        {
            SetStatusText("Username must be at least 3 characters", Color.red);
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetStatusText("Please enter a password", Color.red);
            return;
        }

        if (password.Length < 6)
        {
            SetStatusText("Password must be at least 6 characters", Color.red);
            return;
        }

        SetButtonsInteractable(false);
        SetStatusText("Creating account...", Color.white);

        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            
            SetStatusText("Account created! Logging in...", Color.green);
            Debug.Log("New account created successfully");
            
            await Task.Delay(1000);
            LoadMainMenu();
        }
        catch (AuthenticationException ex)
        {
            if (ex.Message.Contains("already exists") || ex.Message.Contains("conflict"))
            {
                SetStatusText("Username already taken. Try logging in instead.", Color.red);
            }
            else
            {
                SetStatusText($"Sign up failed: {ex.Message}", Color.red);
            }
            Debug.LogError($"Sign up error: {ex.Message}");
            SetButtonsInteractable(true);
        }
        catch (System.Exception ex)
        {
            SetStatusText("Sign up failed. Please try again.", Color.red);
            Debug.LogError($"Unexpected error: {ex.Message}");
            SetButtonsInteractable(true);
        }
    }



    private void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SetStatusText(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        loginButton.interactable = interactable;
        signUpButton.interactable = interactable;
    }

    private void OnDestroy()
    {
        loginButton.onClick.RemoveListener(OnLoginButtonClicked);
        signUpButton.onClick.RemoveListener(OnSignUpButtonClicked);
    }
}