using System;
using System.Collections.Generic;

public struct StoryChoice {
    public StoryChoice(string title, Section sectionToGoto) {
        this.title = title;
        this.sectionToGoto = sectionToGoto;
    }
    public string title;
    public Section sectionToGoto;
}

public class ChoiceEvent : StoryEvent
{
    private List<StoryChoice> choices;
    private Action<List<StoryChoice>> onDisplayChoices;

    public ChoiceEvent(Action<List<StoryChoice>> onDisplayChoices) {
        this.choices = new List<StoryChoice>();
        this.onDisplayChoices = onDisplayChoices;
    }

    public void AddChoice(StoryChoice choice) {
        this.choices.Add(choice);
    }

    public void Execute()
    {
        this.onDisplayChoices(this.choices);
    }
}
