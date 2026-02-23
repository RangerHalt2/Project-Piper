using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{

    [SerializeField] private TextAsset dialogueJSON;
    [SerializeField] private bool isDialogueOnStart = true;
    [SerializeField] private bool isManuallyContinuing = true;

    private DialogueManager manager;
    private Dialogue dialog;

    private void Start()
    {
        manager = GetComponent<DialogueManager>();
        if(manager == null ) manager = GameObject.FindAnyObjectByType<DialogueManager>();
        dialog = manager.LoadDialogueFromJSON(dialogueJSON);
        if(isDialogueOnStart)
            manager.StartDialogue(dialog);
    }

    public void StartDialogue()
    {
        manager.isManuallyContinuing = isManuallyContinuing;
        manager.StartDialogue(dialog);
    }

}
