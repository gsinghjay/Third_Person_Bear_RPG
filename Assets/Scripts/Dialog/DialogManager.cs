using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }
    
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private float textSpeed = 0.05f;
    
    private Action onDialogComplete;
    private bool isDisplayingDialog;
    private bool isTyping;
    private Coroutine typingCoroutine;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (dialogText == null)
        {
            Debug.LogError("DialogText (TMP) reference is missing!");
        }
        
        if (dialogPanel == null)
        {
            Debug.LogError("DialogPanel reference is missing!");
        }
    }

    private void Update()
    {
        if (!isDisplayingDialog) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // Skip typing animation
                if (typingCoroutine != null)
                    StopCoroutine(typingCoroutine);
                isTyping = false;
                dialogText.text = currentLine;
            }
            else
            {
                // Move to next line
                NextLine();
            }
        }
    }
    
    private string currentLine;
    private string[] allLines;
    private int currentLineIndex;

    public void StartDialog(string[] lines, Action onComplete = null)
    {
        Debug.Log("Starting dialog with " + lines.Length + " lines");
        allLines = lines;
        currentLineIndex = 0;
        onDialogComplete = onComplete;
        isDisplayingDialog = true;
        dialogPanel.SetActive(true);
        DisplayNextLine();
    }
    
    private void DisplayNextLine()
    {
        if (currentLineIndex < allLines.Length)
        {
            currentLine = allLines[currentLineIndex];
            typingCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            EndDialog();
        }
    }

    private void NextLine()
    {
        currentLineIndex++;
        DisplayNextLine();
    }
    
    private IEnumerator TypeLine()
    {
        isTyping = true;
        dialogText.text = "";
        
        if (string.IsNullOrEmpty(currentLine))
        {
            Debug.LogError("Current line is null or empty!");
            yield break;
        }
        
        Debug.Log($"Typing line: {currentLine}");
        
        foreach (char c in currentLine)
        {
            if (dialogText == null)
            {
                Debug.LogError("Dialog text component is null!");
                yield break;
            }
            
            dialogText.text += c;
            Debug.Log($"Current text: {dialogText.text}");
            yield return new WaitForSeconds(textSpeed);
        }
        
        isTyping = false;
    }
    
    private void EndDialog()
    {
        isDisplayingDialog = false;
        dialogPanel.SetActive(false);
        onDialogComplete?.Invoke();
        Debug.Log("Dialog completed");
    }
} 