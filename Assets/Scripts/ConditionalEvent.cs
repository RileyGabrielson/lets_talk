using System;

public enum BooleanOperator {
    LESS_THAN,
    GREATER_THAN,
    LESS_THAN_OR_EQUAL_TO,
    GREATER_THAN_OR_EQUAL_TO,
    EQUAL,
    NOT_EQUAL
}

public struct BooleanExpression {
    public BooleanExpression(AlgebraicOperand firstOperand, BooleanOperator booleanOperator, AlgebraicOperand secondOperand) {
        this.firstOperand = firstOperand;
        this.booleanOperator = booleanOperator;
        this.secondOperand = secondOperand;
    }

    public AlgebraicOperand firstOperand;
    public BooleanOperator booleanOperator;
    public AlgebraicOperand secondOperand;
}

public class ConditionalEvent : StoryEvent
{
    private BooleanExpression booleanExpression;
    private StoryEvent childEvent;
    private Action onExecuteNextEvent;

    public ConditionalEvent(StoryEvent childEvent, BooleanExpression booleanExpression, Action onExecuteNextEvent) {
        this.childEvent = childEvent;
        this.booleanExpression = booleanExpression;
        this.onExecuteNextEvent = onExecuteNextEvent;
    }

    public void Execute() {
        if(this.IsConditionTrue()) {
            this.childEvent.Execute();
        } else {
            this.onExecuteNextEvent();
        }
    }

    private bool IsConditionTrue() {
        if(this.booleanExpression.booleanOperator == BooleanOperator.LESS_THAN) {
            return this.booleanExpression.firstOperand.GetValue() < this.booleanExpression.secondOperand.GetValue();
        }
        else if(this.booleanExpression.booleanOperator == BooleanOperator.GREATER_THAN) {
            return this.booleanExpression.firstOperand.GetValue() > this.booleanExpression.secondOperand.GetValue();
        }
        else if(this.booleanExpression.booleanOperator == BooleanOperator.LESS_THAN_OR_EQUAL_TO) {
            return this.booleanExpression.firstOperand.GetValue() <= this.booleanExpression.secondOperand.GetValue();
        }
        else if(this.booleanExpression.booleanOperator == BooleanOperator.GREATER_THAN_OR_EQUAL_TO) {
            return this.booleanExpression.firstOperand.GetValue() >= this.booleanExpression.secondOperand.GetValue();
        }
        else if(this.booleanExpression.booleanOperator == BooleanOperator.EQUAL) {
            return this.booleanExpression.firstOperand.GetValue() == this.booleanExpression.secondOperand.GetValue();
        }
        else {
            return this.booleanExpression.firstOperand.GetValue() != this.booleanExpression.secondOperand.GetValue();
        }
    }
}