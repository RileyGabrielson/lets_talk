using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;


public class TextDisplay : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public float timePerCharacter;

    public AudioClip characterSound;
    public AudioClip endDisplaySound;
    public int minCharsTillSound;
    public int maxCharsTillSound;
    public float maxPitch;
    public float minPitch;
    public AudioManager audioManager;
    public Color boldColor;
    public int defaultTextSize;
    public int h2TextSize;

    private string currentText;
    private int currentIndex;
    private bool isShowingText = false;
    private float timer = 0f;
    private string[] instantDisplayStrings;
    private int numCharsTillSound;
    private Action onNextEvent;
    private VariableRegistry registry;

    public void Awake() {
        this.instantDisplayStrings = new string[4]{"<color=#" + this.boldColor.ToHexString() + ">", "</color>", "<i>", "</i>"};
    }

    public void SetOnNextEvent(Action onNextEvent) {
        this.onNextEvent = onNextEvent;
    }

    public void SetVariableRegistry(VariableRegistry registry) {
        this.registry = registry;
    }

    public void ShowDialogue(List<DialoguePhrase> phrases)
    {
        this.isShowingText = false;
        this.dialogueText.text = "";
        this.currentIndex = 0;
        this.currentText = "";
        this.timer = this.timePerCharacter;
        this.numCharsTillSound = 0;

        foreach (var phrase in phrases)
        {
            if(phrase.format == TextFormatting.H2) {
                this.dialogueText.fontSize = this.h2TextSize;
            } else {
                this.dialogueText.fontSize = this.defaultTextSize;
            }

            if(phrase.format == TextFormatting.BOLD) {
                this.currentText += "<color=#" + this.boldColor.ToHexString() + ">";
                this.currentText += phrase.message;
                this.currentText += "</color>";
            } 
            else if (phrase.format == TextFormatting.ITALIC) {
                this.currentText += "<i>";
                this.currentText += phrase.message;
                this.currentText += "</i>";
            }
            else if (phrase.format == TextFormatting.Variable) {
                this.currentText += this.registry.GetVariable(phrase.message);
            }
            else {
                this.currentText += phrase.message;
            }
        }

        this.isShowingText = true;
    }

    public void Next()
    {
        if(this.isShowingText) {
            this.isShowingText = false;
            this.dialogueText.text = this.currentText;
            this.audioManager.PlaySoundEffect(this.endDisplaySound);
        } else {
            this.onNextEvent();
        }
    }

    public void Update()
    {
        if(this.isShowingText) {
            this.timer -= Time.deltaTime;
            if(this.timer < 0f) {
                foreach (var instantString in this.instantDisplayStrings)
                {
                    if(
                        instantString.Length + this.currentIndex < this.currentText.Length + 1 && 
                        this.currentText.Substring(this.currentIndex, instantString.Length) == instantString
                    ) {
                        this.dialogueText.text += instantString;
                        this.currentIndex += instantString.Length;
                        return;
                    }
                }

                if(this.currentIndex >= this.currentText.Length) {
                    this.audioManager.PlaySoundEffect(this.endDisplaySound);
                    this.isShowingText = false;
                    return;
                }

                this.dialogueText.text += this.currentText[this.currentIndex];
                this.timer = this.timePerCharacter;
                this.currentIndex += 1;
                this.numCharsTillSound -= 1;

                if(this.numCharsTillSound <= 0) {
                    this.audioManager.PlaySoundEffect(this.characterSound, this.minPitch, this.maxPitch);
                    this.numCharsTillSound = UnityEngine.Random.Range(this.minCharsTillSound, this.maxCharsTillSound);
                }
            }
        }
    }
}
