using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum AlgebraicOperator {
    ADD,
    SUBTRACT,
    MULTIPLY,
    DIVIDE
}

public interface AlgebraicOperand {
    int GetValue();
}

public class VariableOperand: AlgebraicOperand {
    private string variableName;
    private VariableRegistry registry;

    public VariableOperand(string variableName, VariableRegistry registry) {
        this.variableName = variableName;
        this.registry = registry;
    }

    public int GetValue() {
        return this.registry.GetInt(this.variableName).value;
    }
}

public class IntOperand: AlgebraicOperand {
    private int value;

    public IntOperand(int value) {
        this.value = value;
    }

    public int GetValue() {
        return this.value;
    }
}

public struct AlgebraicExpression {
    public AlgebraicExpression(AlgebraicOperand firstOperand, AlgebraicOperator algebraicOperator, AlgebraicOperand secondOperand) {
        this.firstOperand = firstOperand;
        this.algebraicOperator = algebraicOperator;
        this.secondOperand = secondOperand;
    }

    public AlgebraicOperand firstOperand;
    public AlgebraicOperator algebraicOperator;
    public AlgebraicOperand secondOperand;
}

public class VariableChangeEvent: StoryEvent
{
    private string variableName;
    private AlgebraicExpression expression;
    private VariableRegistry registry;
    private Action onExecuteNextEvent;

    public VariableChangeEvent(
        string variableName, 
        AlgebraicExpression expression, 
        VariableRegistry registry, 
        Action onExecuteNextEvent
    ) {
        this.variableName = variableName;
        this.expression = expression;
        this.registry = registry;
        this.onExecuteNextEvent = onExecuteNextEvent;
    }

    public void Execute() {
        if(!this.registry.HasInt(this.variableName)) {
            this.registry.AddInt(this.variableName, 0);
        }
        this.registry.SetInt(this.variableName, this.GetExpressionValue());

        this.onExecuteNextEvent();
    }

    private int GetExpressionValue() {
        int operand1 = this.expression.firstOperand.GetValue();
        int operand2 = this.expression.secondOperand.GetValue();
        var algebraicOperator = this.expression.algebraicOperator;

        if(algebraicOperator == AlgebraicOperator.ADD) return operand1 + operand2;
        else if(algebraicOperator == AlgebraicOperator.SUBTRACT) return operand1 - operand2;
        else if(algebraicOperator == AlgebraicOperator.MULTIPLY) return operand1 * operand2;
        else if(algebraicOperator == AlgebraicOperator.DIVIDE) return operand1 / operand2;

        return 0;
    }
}
