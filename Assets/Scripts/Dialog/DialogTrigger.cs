using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    [SerializeField] private string[] dialogLines = new string[] 
    {
        "Hello warrior!",
        "Press SPACE to continue through dialog.",
        "This is a test of our dialog system!"
    };

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DialogManager.Instance.StartDialog(dialogLines);
        }
    }
}
