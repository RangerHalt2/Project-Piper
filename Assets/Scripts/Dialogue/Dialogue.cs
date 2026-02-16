using UnityEngine;

[System.Serializable]
public class Dialogue
{
    [TextArea(3, 10)]
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public string speakerID;
    public string sentence;

    public string emotion;
    public AudioClip voiceClip; //here just in case we need later

    public DialogueChoice[] choices;

    public int nextLineIndex = -1;
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public int nextLineIndex; //Index to jump to in dialogue list

    //Don't believe we need this but this is just to change dialogue if there characters have not met yet, honestly don't think we need it
    public string requiredFlag;
    public string setFlagOnSelect;
}
