using System.Collections.Generic;

public class Module
{
    private List<Section> sections;
    private string title;
    private int currentSection;

    public Module(string title) {
        this.currentSection = -1;
        this.sections = new List<Section>();
        this.title = title;
    }

    public void SetSections(List<Section> sections) {
        this.sections = sections;
    }

    public void AddSection(Section section) {
        this.sections.Add(section);
    }

    public void StartModule() {
        this.currentSection = -1;
        this.ExecuteNextSection();
    }

    public void ExecuteNextSection() {
        this.currentSection += 1;
        if(this.currentSection < this.sections.Count) this.sections[this.currentSection].StartSection();
    }

    public string GetTitle() {
        return this.title;
    }

    public List<Section> GetSections() {
        return this.sections;
    }
}
