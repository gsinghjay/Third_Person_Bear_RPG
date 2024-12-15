using UnityEngine;
using Yarn.Unity;

public class YarnDialogController : MonoBehaviour
{
    public static YarnDialogController Instance { get; private set; }
    
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private GameObject dialoguePanel;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Remove direct command registration
            // Let QuestManager handle the commands
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartDialogue(string nodeName)
    {
        dialoguePanel.SetActive(true);
        dialogueRunner.StartDialogue(nodeName);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
