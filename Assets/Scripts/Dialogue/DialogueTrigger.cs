using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{

    [SerializeField] private TextAsset dialogueJSON;
    

    private DialogueManager manager;

    private void Start()
    {
        manager = GetComponent<DialogueManager>();
        Dialogue dialog = manager.LoadDialogueFromJSON(dialogueJSON);
        manager.StartDialogue(dialog);
    }

}
