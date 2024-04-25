using System;
using System.Collections.Generic;

public enum TextFormatting {
    REGULAR,
    BOLD,
    ITALIC,
    H2,
    Variable
}

public struct DialoguePhrase {
    public DialoguePhrase(string message, TextFormatting format) {
        this.message = message;
        this.format = format;
    }
    public string message;
    public TextFormatting format;
}

public class DialogueEvent : StoryEvent
{
    private List<DialoguePhrase> phrases;
    private Action<List<DialoguePhrase>> onDisplayPhrases;

    public DialogueEvent(Action<List<DialoguePhrase>> onDisplayPhrases) {
        this.phrases = new List<DialoguePhrase>();
        this.onDisplayPhrases = onDisplayPhrases;
    }

    public void AddPhrase(DialoguePhrase phrase) {
        this.phrases.Add(phrase);
    }

    public void Execute()
    {
        this.onDisplayPhrases(this.phrases);
    }
}