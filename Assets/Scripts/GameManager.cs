using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TextDisplay textDisplay;
    public ChoicesDisplay choicesDisplay;

    private StoryParser storyParser;
    private VariableRegistry variableRegistry;
    private Section currentSection;

    void Start()
    {
        this.variableRegistry = new VariableRegistry();

        this.textDisplay.SetOnNextEvent(() => this.NextEvent());
        this.textDisplay.SetVariableRegistry(this.variableRegistry);
        this.choicesDisplay.SetOnNewSection(section => this.NewSection(section));

        this.storyParser = new StoryParser(this.variableRegistry);
        this.storyParser.SetOnDisplayPhrases(phrases => this.textDisplay.ShowDialogue(phrases));
        this.storyParser.SetOnDisplayChoices(choices => this.choicesDisplay.ShowChoices(choices));
        this.storyParser.SetOnNewSection(section => this.NewSection(section));
        this.storyParser.SetOnNextEvent(() => this.NextEvent());

        this.StartStory();
    }

    private void NextEvent()
    {
        this.currentSection.ExecuteNextEvent();
    }

    private void NewSection(Section section)
    {
        this.currentSection = section;
        section.StartSection();
    }

    private void StartStory()
    {
        var modules = this.storyParser.ReadFiles(
            new string[]{
                "Assets/Stories/Main_Menu.md", 
                "Assets/Stories/Intro.md",
                "Assets/Stories/odysseus.md",
                "Assets/Stories/town.md",
                "Assets/Stories/tavern.md",
                "Assets/Stories/apothecary.md"
            }
        );
        this.currentSection = modules[0].GetSections()[0];
        this.currentSection.StartSection();
    }

}
