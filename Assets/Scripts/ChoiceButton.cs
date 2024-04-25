using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ChoiceButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI buttonText;
    public Color selectedColor;
    public float fadeInTime;
    public float fadeOutTime;

    private bool isSelected;
    private bool isFadingIn;
    private float timer;
    private Action onStartSelect;
    private Action onDoneSelect;

    void Start() {
        this.button.onClick.AddListener(() => this.Select());
        var color = this.buttonText.color;
        color.a = 0f;
        this.buttonText.color = color;
        this.isSelected = false;
        this.isFadingIn = true;
        this.timer = this.fadeInTime;
    }

    public void SetOnStartSelect(Action onStartSelect) {
        this.onStartSelect = onStartSelect;
    }

    public void SetOnFinishSelect(Action onDoneSelect) {
        this.onDoneSelect = onDoneSelect;
    }

    public void SetTitle(string title) {
        this.buttonText.text = title;
    }

    public void Hide() {
        var color = this.buttonText.color;
        color.a = 0f;
        this.buttonText.color = color;
        this.button.onClick.RemoveAllListeners();
        this.isFadingIn = false;
    }

    public void Select() {
        this.onStartSelect();
        this.isSelected = true;
        this.buttonText.color = selectedColor;
        this.button.onClick.RemoveAllListeners();
        this.timer = this.fadeOutTime;
        this.isFadingIn = false;
    }

    public void Update() {
        if(this.isFadingIn) {
            this.timer -= Time.deltaTime;
            if(this.timer <= 0f) {
                this.isFadingIn = false;
            }
            var color = this.buttonText.color;
            color.a = (this.fadeInTime - this.timer) / this.fadeInTime;
            this.buttonText.color = color;
        }

        if(this.isSelected) {
            this.timer -= Time.deltaTime;
            if(this.timer <= 0f) {
                this.onDoneSelect();
                this.isSelected = false;
                return;
            }

            var color = this.buttonText.color;
            color.a = this.timer / this.fadeOutTime;
            this.buttonText.color = color;
        }
    }
}
