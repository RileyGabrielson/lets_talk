using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Diagnostics;

enum LineType {
    SECTION_HEADER,
    DIALOGUE,
    CHOICE,
    SYSTEM_INSTRUCTION,
    REDIRECT
}

struct UnpopulatedSection {
    public UnpopulatedSection(Section section) {
        this.section = section;
        this.eventLines = new List<string>();
    }

    public Section section;
    public List<string> eventLines;
}

struct UnpopulatedModule {
    public UnpopulatedModule(Module module) {
        this.module = module;
        this.unpopulatedSections = new List<UnpopulatedSection>();
    }

    public Module module;
    public List<UnpopulatedSection> unpopulatedSections;
}

public class StoryParser
{
    private Action<List<DialoguePhrase>> onDisplayPhrases;
    private Action<List<StoryChoice>> onDisplayChoices;
    private Action<Section> onNewSection;
    private Action onNextEvent;
    private VariableRegistry registry;

    public StoryParser(VariableRegistry registry) {
        this.onDisplayPhrases = _ => {};
        this.onDisplayChoices = _ => {};
        this.onNewSection = _ => {};
        this.onNextEvent = () => {};
        this.registry = registry;
    }

    public void SetOnDisplayPhrases(Action<List<DialoguePhrase>> onDisplayPhrases) {
        this.onDisplayPhrases = onDisplayPhrases;
    }

    public void SetOnDisplayChoices(Action<List<StoryChoice>> onDisplayChoices) {
        this.onDisplayChoices = onDisplayChoices;
    }

    public void SetOnNewSection(Action<Section> onNewSection) {
        this.onNewSection = onNewSection;
    }

    public void SetOnNextEvent(Action onNextEvent) {
        this.onNextEvent = onNextEvent;
    }

    public List<Module> ReadFiles(string[] paths)
    {
        List<UnpopulatedModule> unpopulatedModules = new List<UnpopulatedModule>();

        foreach (var path in paths)
        {
            var lines = this.ReadLinesFromFile(path);
            var unpopulatedSections = this.CreateUnpopulatedSections(lines);
            var pathParts = path.Split("/");
            var withoutSuffix = pathParts[pathParts.Length - 1].Split(".")[0];
            var unpopulatedModule = new UnpopulatedModule(new Module(withoutSuffix));
            unpopulatedModule.unpopulatedSections = unpopulatedSections;
            unpopulatedModules.Add(unpopulatedModule);
        }

        var populatedModules = this.PopulateModules(unpopulatedModules);
        return populatedModules;
    }

    private string[] ReadLinesFromFile(string path) {
        StreamReader reader = new StreamReader(path); 
        string data = reader.ReadToEnd();

        string[] lines = data.Split('\n');
        string[] dataLines = lines.Where(line => line != "").ToArray();

        reader.Close();

        return dataLines;
    }

    private List<UnpopulatedSection> CreateUnpopulatedSections(string[] lines) {
        var sections = new List<UnpopulatedSection>();

        foreach (var line in lines) {
            var lineType = this.GetLineType(line);
            if(lineType == LineType.SECTION_HEADER) {
                var newSection = new UnpopulatedSection(this.CreateSection(line));
                sections.Add(newSection);
            }
            else if(sections.Count > 0) {
                sections[sections.Count - 1].eventLines.Add(line);
            }
        }

        return sections;
    }

    private List<Module> PopulateModules(List<UnpopulatedModule> unpopulatedModules) {
        var modules = new List<Module>();

        foreach (var unpopulated in unpopulatedModules)
        {
            modules.Add(unpopulated.module);
            var sections = new List<Section>();
            foreach (var unpopulatedSection in unpopulated.unpopulatedSections)
            {
                sections.Add(unpopulatedSection.section);
            }
            unpopulated.module.SetSections(sections);
        }

        foreach (var unpopulated in unpopulatedModules)
        {
            List<Section> populatedSections = this.PopulateSections(unpopulated.unpopulatedSections, modules);
            unpopulated.module.SetSections(populatedSections);
        }

        return modules;
    }

    private List<Section> PopulateSections(List<UnpopulatedSection> unpopulatedSections, List<Module> modules) {
        List<Section> populatedSections = new List<Section>();

        foreach (var unpopulated in unpopulatedSections) {
            for (int i = 0; i < unpopulated.eventLines.Count; i++) {
                var (newEvent, indexOffset) = this.BuildEvent(unpopulated, i, modules);
                unpopulated.section.AddEvent(newEvent);
                i += indexOffset;
            }

            populatedSections.Add(unpopulated.section);
        }

        return populatedSections;
    }

    private (StoryEvent, int) BuildEvent(UnpopulatedSection unpopulated, int i, List<Module> modules) {
        var line = unpopulated.eventLines[i];
        var lineType = this.GetLineType(line);

        if(lineType == LineType.CHOICE) {
            List<string> choiceLines = new List<string>();
            int indexOffset = 0;

            while (lineType == LineType.CHOICE) {
                choiceLines.Add(line);
                indexOffset += 1;
                i += 1;

                if(i >= unpopulated.eventLines.Count) break;
                line = unpopulated.eventLines[i];
                lineType = this.GetLineType(line);
            };

            return (this.CreateChoiceEvent(choiceLines, modules), indexOffset);
        }

        if(lineType == LineType.REDIRECT) {
            return (this.CreateRedirectEvent(line, modules), 0);
        }
        else if(lineType == LineType.SYSTEM_INSTRUCTION) {
            return this.CreateSystemInstructionEvent(unpopulated, i, modules);
        }
        else {
            return (this.CreateDialogueEvent(line), 0);
        }
    }

    private (StoryEvent, int) CreateSystemInstructionEvent(UnpopulatedSection unpopulated, int i, List<Module> modules) {
        var line = unpopulated.eventLines[i];

        if(line.Substring(1, 3) == "SET") return (this.BuildChangeVariableEvent(line), 0);
        else {
            return this.BuildConditionalEvent(unpopulated, i, modules);
        }
    }

    private (ConditionalEvent, int) BuildConditionalEvent(UnpopulatedSection unpopulated, int i, List<Module> modules) {
        var line = unpopulated.eventLines[i];
        var contents = line.Substring(1, line.Length - 2);
        var tokens = contents.Split(" ");

        var firstOperandString = tokens[1];
        var operatorString = tokens[2];
        var secondOperandString = tokens[3];

        BooleanOperator booleanOperator = 
            operatorString == "<" ? BooleanOperator.LESS_THAN : 
            operatorString == ">" ? BooleanOperator.GREATER_THAN : 
            operatorString == "<=" ? BooleanOperator.LESS_THAN_OR_EQUAL_TO : 
            operatorString == ">=" ? BooleanOperator.GREATER_THAN_OR_EQUAL_TO : 
            operatorString == "!=" ? BooleanOperator.NOT_EQUAL : BooleanOperator.EQUAL;

        var x = 0;
        var isFirstNumber = int.TryParse(firstOperandString, out x);

        AlgebraicOperand firstOperand = 
            isFirstNumber ? 
                new IntOperand(x) : 
                new VariableOperand(firstOperandString, this.registry);

        var y = 0;
        var isSecondNumber = int.TryParse(secondOperandString, out x);

        AlgebraicOperand secondOperand = 
            isFirstNumber ? 
                new IntOperand(y) : 
                new VariableOperand(secondOperandString, this.registry);
        
        BooleanExpression booleanExpression = new BooleanExpression(firstOperand, booleanOperator, secondOperand);

        var (childEvent, indexOffset) = this.BuildEvent(unpopulated, i + 1, modules);
        return (new ConditionalEvent(childEvent, booleanExpression, this.onNextEvent), indexOffset + 1);
    }

    private VariableChangeEvent BuildChangeVariableEvent(string line) {
        var contents = line.Substring(1, line.Length - 2);
        var tokens = contents.Split(" ");
        var variableName = tokens[1];
        var firstOperandString = tokens[3];

        var x = 0;
        var isFirstNumber = int.TryParse(firstOperandString, out x);

        AlgebraicOperand firstOperand = 
            isFirstNumber ? 
                new IntOperand(x) : 
                new VariableOperand(firstOperandString, this.registry);

        if(tokens.Length < 5) {
            var simpleExpression = new AlgebraicExpression(firstOperand, AlgebraicOperator.ADD, new IntOperand(0));
            return new VariableChangeEvent(variableName, simpleExpression, this.registry, this.onNextEvent);
        }

        var operatorString = tokens[4];
        AlgebraicOperator algebraicOperator = 
            operatorString == "/" ? AlgebraicOperator.DIVIDE : 
            operatorString == "-" ? AlgebraicOperator.SUBTRACT :
            operatorString == "*" ? AlgebraicOperator.MULTIPLY :
            AlgebraicOperator.ADD;

        var secondOperandString = tokens[5];
        var y = 0;
        var isSecondNumber = int.TryParse(secondOperandString, out y);
        AlgebraicOperand secondOperand = 
            isSecondNumber ? 
                new IntOperand(y) : 
                new VariableOperand(secondOperandString, this.registry);

        var expression = new AlgebraicExpression(firstOperand, algebraicOperator, secondOperand);
        return new VariableChangeEvent(variableName, expression, this.registry, this.onNextEvent);
    }

    private DialogueEvent CreateDialogueEvent(string line) {
        var dialogue = new DialogueEvent(this.onDisplayPhrases);

        var phrases = this.GetDialoguePhrases(line);
        foreach (var phrase in phrases)
        {
            dialogue.AddPhrase(phrase);
        }

        return dialogue;
    }

    private List<DialoguePhrase> GetDialoguePhrases(string line) {
        var phrases = new List<DialoguePhrase>();

        if(line.Length > 3 && line.Substring(0, 3) == "## ") {
            var phrase = new DialoguePhrase(line.Substring(3), TextFormatting.H2);
            phrases.Add(phrase);
            return phrases;
        }

        for(int i = 0; i < line.Length; i++) {
            if(i < line.Length - 1 && line.Substring(i, 2) == "**") {
                var (phrase, indexOffset) = this.GetBoldPhrase(line, i);
                phrases.Add(phrase);
                i += indexOffset - 1;
            }
            else if(line[i] == '*') {
                var (phrase, indexOffset) = this.GetItalicPhrase(line, i);
                phrases.Add(phrase);
                i += indexOffset - 1;
            }
            else if(i < line.Length - 1 && line.Substring(i, 2) == "`$") {
                var (phrase, indexOffset) = this.GetVariablePhrase(line, i);
                phrases.Add(phrase);
                i += indexOffset - 1;
            }
            else {
                var (phrase, indexOffset) = this.GetRegularPhrase(line, i);
                phrases.Add(phrase);
                i += indexOffset - 1;
            }
        }

        return phrases;
    }

    private (DialoguePhrase, int) GetRegularPhrase(string line, int startIndex) {
        var nextBold = line.IndexOf('*', startIndex);
        var nextVariable = line.IndexOf('`', startIndex);
        var endIndex = Math.Min(nextBold, nextVariable);

        if(nextBold == -1) endIndex = nextVariable;
        if(nextVariable == -1) endIndex = nextBold;
        if(endIndex == -1) endIndex = line.Length;

        var value = line.Substring(startIndex, endIndex - startIndex);
        return (new DialoguePhrase(value, TextFormatting.REGULAR), value.Length);
    }

    private (DialoguePhrase, int) GetItalicPhrase(string line, int startIndex) {
        var endIndex = line.IndexOf('*', startIndex + 1);
        if(endIndex == -1) endIndex = line.Length;
        var startOfItalicizedText = startIndex + 1;

        var value = line.Substring(startOfItalicizedText, endIndex - startOfItalicizedText);
        return (new DialoguePhrase(value, TextFormatting.ITALIC), value.Length + 2);
    }

    private (DialoguePhrase, int) GetBoldPhrase(string line, int startIndex) {
        var endIndex = line.IndexOf('*', startIndex + 2);
        if(endIndex == -1) endIndex = line.Length;
        var startOfBoldedText = startIndex + 2;

        var value = line.Substring(startOfBoldedText, endIndex - startOfBoldedText);
        return (new DialoguePhrase(value, TextFormatting.BOLD), value.Length + 4);
    }

    private (DialoguePhrase, int) GetVariablePhrase(string line, int startIndex) {
        var endIndex = line.IndexOf('`', startIndex + 2);
        if(endIndex == -1) endIndex = line.Length;
        var startOfVariableName = startIndex + 2;

        var value = line.Substring(startOfVariableName, endIndex - startOfVariableName);
        return (new DialoguePhrase(value, TextFormatting.Variable), value.Length + 3);
    }

    private ChoiceEvent CreateChoiceEvent(List<string> choiceLines, List<Module> modules) {
        var choice = new ChoiceEvent(this.onDisplayChoices);

        foreach (var line in choiceLines)
        {
            var (moduleName, sectionName, title) = this.ParseChoiceDetails(line);

            Module moduleToGoto = modules.Find(module => {
                return module.GetTitle() == moduleName;
            });

            Section sectionToGoto = moduleToGoto.GetSections().Find(section => {
                return section.GetTitle() == sectionName;
            });
            choice.AddChoice(new StoryChoice(title, sectionToGoto));
        }

        return choice;
    }

    private (string, string, string) ParseChoiceDetails(string line) {
        int moduleStart = line.IndexOf('[') + 2;
        int moduleEnd = line.IndexOf('#');
        string moduleName = line.Substring(moduleStart, moduleEnd - moduleStart);

        int sectionStart = line.IndexOf('#') + 1;
        int sectionEnd = line.IndexOf('|');
        string sectionName = line.Substring(sectionStart, sectionEnd - sectionStart);

        int titleStart = line.IndexOf('|') + 1;
        int titleEnd = line.IndexOf(']');
        string title = line.Substring(titleStart, titleEnd - titleStart);

        return (moduleName, sectionName, title);
    }

    private RedirectEvent CreateRedirectEvent(string line, List<Module> modules) {
        var (moduleName, sectionName) = this.ParseRedirectSectionName(line);

        Module moduleToGoto = modules.Find(module => {
            return module.GetTitle() == moduleName;
        });

        Section sectionToGoto = moduleToGoto.GetSections().Find(section => {
            return section.GetTitle() == sectionName;
        });
        var redirect = new RedirectEvent(sectionToGoto, this.onNewSection);

        return redirect;
    }

    private (string, string) ParseRedirectSectionName(string line) {
        int moduleStart = line.IndexOf('[') + 2;
        int moduleEnd = line.IndexOf('#');
        string moduleName = line.Substring(moduleStart, moduleEnd - moduleStart);

        int sectionStart = line.IndexOf('#') + 1;
        int sectionEnd = line.IndexOf(']');
        string sectionName = line.Substring(sectionStart, sectionEnd - sectionStart);

        return (moduleName, sectionName);
    }

    private LineType GetLineType(string line) {
        if(line.Length > 2 && line.Substring(0, 2) == "# ") return LineType.SECTION_HEADER;
        if(line.Length > 2 && line.Substring(0, 2) == "- ") return LineType.CHOICE;
        if(line.Length > 2 && line.Substring(0, 1) == "`") return LineType.SYSTEM_INSTRUCTION;
        if(line.Length > 3 && line.Substring(0, 2) == "[[") return LineType.REDIRECT;

        return LineType.DIALOGUE;
    }

    private Section CreateSection(string sectionLine) {
        var sectionTypes = sectionLine.Split("`");
        string title = sectionTypes[0].Substring(2);
        var showAll = false;

        if(sectionTypes.Length > 1) {
            if(sectionTypes[1] == "show-all") showAll = true;
        }

        return new Section(title, showAll);
    }
}