using System.Collections.Generic;

public class Section
{
    public List<StoryEvent> storyEvents;
    private int currentEvent;
    private string title;
    private bool showAll;

    public Section(string title, bool showAll) {
        this.currentEvent = -1;
        this.storyEvents = new List<StoryEvent>();
        this.title = title;
        this.showAll = showAll;
    }

    public void AddEvent(StoryEvent storyEvent) {
        this.storyEvents.Add(storyEvent);
    }

    public void StartSection() {
        if(this.showAll) {
            foreach (var storyEvent in this.storyEvents)
            {
                storyEvent.Execute();
            }
            return;
        }

        this.currentEvent = -1;
        this.ExecuteNextEvent();
    }

    public void ExecuteNextEvent() {
        this.currentEvent += 1;
        if(this.currentEvent < this.storyEvents.Count) this.storyEvents[this.currentEvent].Execute();
    }

    public string GetTitle() {
        return this.title;
    }
}
