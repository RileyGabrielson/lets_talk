using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ChoicesDisplay : MonoBehaviour
{
    public Transform choicesParent;
    public GameObject choiceButtonPrefab;
    private List<GameObject> currentChoiceButtons = new List<GameObject>();
    private Action<Section> onNewSection;

    public void SetOnNewSection(Action<Section> onNewSection) {
        this.onNewSection = onNewSection;
    }

    public void ShowChoices(List<StoryChoice> choices) {
        this.ClearChoices();

        foreach (var choice in choices)
        {
            var buttonObject = Instantiate(this.choiceButtonPrefab, this.choicesParent);
            var choiceButton = buttonObject.GetComponent<ChoiceButton>();
            choiceButton.SetTitle(choice.title);
            choiceButton.SetOnStartSelect(() => this.SelectChoice(choice, buttonObject));
            choiceButton.SetOnFinishSelect(() => this.ClearChoices());
            this.currentChoiceButtons.Add(buttonObject);
        }
    }

    private void SelectChoice(StoryChoice choice, GameObject selectedButton) {
        this.onNewSection(choice.sectionToGoto);
        foreach (var choiceButton in this.currentChoiceButtons)
        {
            if(choiceButton != selectedButton) {
                choiceButton.GetComponent<ChoiceButton>().Hide();
            }
        }
    }

    private void ClearChoices() {
        if(this.currentChoiceButtons.Count <= 0) return;

        foreach (var choiceButton in this.currentChoiceButtons)
        {
            Destroy(choiceButton);
        }
        this.currentChoiceButtons.Clear();
    }
}
