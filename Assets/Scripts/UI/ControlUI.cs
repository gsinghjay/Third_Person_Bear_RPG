using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ControlsUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private TextMeshProUGUI controlsText;
    [SerializeField] private Button closeButton;
    
    [Header("Settings")]
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private bool showOnlyFirstTime = true;
    
    private const string CONTROLS_SHOWN_KEY = "ControlsShown";
    
    private void Start()
    {
        if (showOnlyFirstTime && PlayerPrefs.HasKey(CONTROLS_SHOWN_KEY))
        {
            gameObject.SetActive(false);
            return;
        }

        InitializeControls();
        StartCoroutine(AutoHideControls());
    }

    private void InitializeControls()
    {
        if (controlsText != null)
        {
            controlsText.text = GetControlsText();
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideControls);
        }

        // Mark controls as shown
        PlayerPrefs.SetInt(CONTROLS_SHOWN_KEY, 1);
        PlayerPrefs.Save();
    }

    private string GetControlsText()
    {
        return "Basic Controls:\n\n" +
               "WASD - Move Character\n" +
               "Space - Jump\n" +
               "Left Mouse - Basic Attack\n" +
               "Right Mouse - Special Attack\n" +
               "1,2,3 - Switch Elements\n" +
               "E - Interact\n" +
               "Mouse - Look Around\n\n" +
               "Press ESC to close this window";
    }

    private void HideControls()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator AutoHideControls()
    {
        yield return new WaitForSeconds(displayDuration);
        HideControls();
    }

    private void Update()
    {
        // Allow closing with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideControls();
        }
    }

    // Public method to show controls again if needed
    public void ShowControls()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
        StartCoroutine(AutoHideControls());
    }
}