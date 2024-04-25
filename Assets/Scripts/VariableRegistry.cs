using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IntVariable {
    public IntVariable(string name, int value) {
        this.name = name;
        this.value = value;
    }
    
    public string name;
    public int value;

    public void SetValue(int newValue) {
        this.value = newValue;
    }
}

public class VariableRegistry
{
    private List<IntVariable> intVariables = new List<IntVariable>();

    public void ClearAll() {
        this.intVariables.Clear();
    }

    public IntVariable GetInt(string name) {
        var existingInt = this.intVariables.Find(variable => variable.name == name);
        return existingInt;
    }

    public void SetInt(string name, int value) {
        var existingIndex = this.intVariables.FindIndex(variable => variable.name == name);
        if(existingIndex == -1) return;

        this.intVariables.RemoveAt(existingIndex);
        this.intVariables.Add(new IntVariable(name, value));
    }

    public IntVariable AddInt(string name, int value) {
        var newVariable = new IntVariable(name, value);
        this.intVariables.Add(newVariable);
        return newVariable;
    }

    public bool HasInt(string name) {
        return this.intVariables.Exists(variable => variable.name == name);
    }

    public string GetVariable(string name) {
        return this.intVariables.Find(v => v.name == name).value.ToString();
    }
}
