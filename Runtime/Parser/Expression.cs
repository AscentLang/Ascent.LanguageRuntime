#nullable enable
using AscentLanguage.Functions;
using AscentLanguage.Tokenizer;
using AscentLanguage.Util;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AscentLanguage.Data;
using AscentLanguage.Var;
using UnityEngine;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace AscentLanguage.Parser
{
    public abstract class Expression
    {
        public abstract bool Static { get; }
        public abstract Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData);
    }

    public class BinaryExpression : Expression
    {
        public Expression Left { get; set; }
        public Token Operator { get; set; }
        public Expression Right { get; set; }

        public BinaryExpression(Expression left, Token op, Expression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override bool Static => Left.Static && Right.Static;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            var leftValue = Left.Evaluate(ascentVariableMap, ascentScriptData);
            var rightValue = Right.Evaluate(ascentVariableMap, ascentScriptData);

            if (leftValue == null || rightValue == null)
            {
                throw new FormatException("Left and right values must be non-null");
            }

            switch (Operator.Type)
            {
                //Ensure float for all operations and string for concatenation
                case TokenType.Multiplication when leftValue.Type == VarType.Float && rightValue.Type == VarType.Float:
                {
                    var result = new Variable();
                    result.SetValue(leftValue.GetValue<float>() * rightValue.GetValue<float>(), VarType.Float);
                    return result;
                }
                case TokenType.Multiplication:
                    throw new InvalidOperationException(
                        $"Multiplication requires two floats. Left Type {leftValue.Type.ToString()}. Right Type {rightValue.Type.ToString()}.");
                case TokenType.Division when leftValue.Type == VarType.Float && rightValue.Type == VarType.Float:
                {
                    var result = new Variable();
                    result.SetValue(leftValue.GetValue<float>() / rightValue.GetValue<float>(), VarType.Float);
                    return result;
                }
                case TokenType.Division:
                    throw new InvalidOperationException(String.Format("Division requires two floats. Left Type {0}. Right Type {1}.", leftValue.Type.ToString(), rightValue.Type.ToString()));
                case TokenType.Addition when leftValue.Type == VarType.Float && rightValue.Type == VarType.Float:
                {
                    Variable result = new Variable();
                    result.SetValue(leftValue.GetValue<float>() + rightValue.GetValue<float>(), VarType.Float);
                    return result;
                }
                case TokenType.Addition:
                {
                    //check for implicit addition.
                    var leftType = leftValue.Value.GetType();
                    var implicitAdd = leftType.GetMethod("op_Addition");
                    if (implicitAdd != null)
                    {
                        var resultValue = implicitAdd.Invoke(null, new object[] { leftValue.Value, rightValue.Value });
                        Variable var = new Variable();
                        var.SetValue(resultValue);
                        return var;
                    }
                    else
                    {
                        Variable result = new Variable();
                        result.SetValue(leftValue.ToString() + rightValue.ToString(), VarType.String);
                        return result;
                    }
                }
                case TokenType.Subtraction when leftValue.Type == VarType.Float && rightValue.Type == VarType.Float:
                {
                    Variable result = new Variable();
                    result.SetValue(leftValue.GetValue<float>() - rightValue.GetValue<float>(), VarType.Float);
                    return result;
                }
                case TokenType.Subtraction:
                    throw new InvalidOperationException(String.Format("Subtraction requires two floats. Left Type {0}. Right Type {1}.", leftValue.Type.ToString(), rightValue.Type.ToString()));
                case TokenType.Pow when leftValue.Type == VarType.Float && rightValue.Type == VarType.Float:
                {
                    Variable result = new Variable();
                    result.SetValue((float)Math.Pow(leftValue.GetValue<float>(), rightValue.GetValue<float>()), VarType.Float);
                    return result;
                }
                case TokenType.Pow:
                    throw new InvalidOperationException(String.Format("Pow requires two floats. Left Type {0}. Right Type {1}.", leftValue.Type.ToString(), rightValue.Type.ToString()));
                case TokenType.Modulus when leftValue.Type == VarType.Float && rightValue.Type == VarType.Float:
                {
                    Variable result = new Variable();
                    result.SetValue(leftValue.GetValue<float>() % rightValue.GetValue<float>(), VarType.Float);
                    return result;
                }
                case TokenType.Modulus:
                    throw new InvalidOperationException(String.Format("Modulus requires two floats. Left Type {0}. Right Type {1}.", leftValue.Type.ToString(), rightValue.Type.ToString()));
                case TokenType.GreaterThan when leftValue.Type == VarType.Float && rightValue.Type == VarType.Float:
                {
                    Variable result = new Variable();
                    result.SetValue(leftValue.GetValue<float>() > rightValue.GetValue<float>(), VarType.Bool);
                    return result;
                }
                case TokenType.GreaterThan:
                    throw new InvalidOperationException(String.Format("GreaterThan requires two floats. Left Type {0}. Right Type {1}.", leftValue.Type.ToString(), rightValue.Type.ToString()));
                case TokenType.LesserThen when leftValue.Type == VarType.Float && rightValue.Type == VarType.Float:
                {
                    Variable result = new Variable();
                    result.SetValue(leftValue.GetValue<float>() < rightValue.GetValue<float>(), VarType.Bool);
                    return result;
                }
                case TokenType.LesserThen:
                    throw new InvalidOperationException(String.Format("LesserThen requires two floats. Left Type {0}. Right Type {1}.", leftValue.Type.ToString(), rightValue.Type.ToString()));
                default:
                    throw new InvalidOperationException($"Unsupported operator: {Operator.TokenBuffer}");
            }
        }
    }

    public class ConstantExpression : Expression
    {
        public Token Token { get; set; }

        public ConstantExpression(Token token)
        {
            Token = token;
        }

        public override bool Static => Token.Type != TokenType.Query;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            switch (Token.Type)
            {
                case TokenType.Constant:
                    Variable result = new Variable();
                    result.SetValue(Utility.ConvertToFloat(Token.TokenBuffer), VarType.Float);
                    return result;
                case TokenType.True:
                    Variable resultTrue = new Variable();
                    resultTrue.SetValue(true, VarType.Bool);
                    return resultTrue;
                case TokenType.False:
                    Variable resultFalse = new Variable();
                    resultFalse.SetValue(false, VarType.Bool);
                    return resultFalse;
                case TokenType.String:
                    Variable resultString = new Variable();
                    resultString.SetValue(Token.TokenBuffer, VarType.String);
                    return resultString;
                case TokenType.Query:
                    if (ascentVariableMap.QueryVariables.TryGetValue(Token.TokenBuffer, out Variable value))
                    {
                        return value;
                    }

                    Console.WriteLine($"Variable {Token.TokenBuffer} ({Token.TokenBuffer.Length}) not found in variable map");
                    return 0f;
            }

            return 0.0f;
        }
    }

    public class FunctionDefinitionExpression : Expression
    {
        public Token FunctionToken { get; }
        public Expression[] Contents { get; }

        public FunctionDefinitionExpression(Token functionToken, Expression[] contents)
        {
            FunctionToken = functionToken;
            Contents = contents;
        }

        public override bool Static => false;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            var name = FunctionToken.TokenBuffer;
            var definition = ascentScriptData.Functions.FirstOrDefault(x => x.Key == name);
            if (definition.Value != null)
            {
                definition.Value.Contents = Contents;
            }
            return null;
        }
    }

    public class ForLoopExpression : Expression
    {
        public Expression Defintion { get; }
        public Expression Condition { get; }
        public Expression Suffix { get; }
        public Expression[] Contents { get; }

        public override bool Static => Defintion.Static && Condition.Static && Suffix.Static && Contents.All(x => x.Static);

        public ForLoopExpression(Expression defintion, Expression condition, Expression suffix, Expression[] contents)
        {
            Defintion = defintion;
            Condition = condition;
            Suffix = suffix;
            Contents = contents;
        }

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            Defintion.Evaluate(ascentVariableMap, ascentScriptData);
            while (Condition.Evaluate(ascentVariableMap, ascentScriptData)?.GetValue<bool>() ?? false)
            {
                foreach (var expression in Contents)
                {
                    var map = ascentScriptData?.Clone();
                    if (map == null) continue;
                    expression.Evaluate(ascentVariableMap, map);
                    foreach (var item in ascentScriptData.Variables.Select(x => x.Key).ToList())
                    {
                        ascentScriptData.Variables[item] = map.Variables[item];
                    }
                }
                Suffix.Evaluate(ascentVariableMap, ascentScriptData);
            }
            return null;
        }
    }

    public class WhileLoopExpression : Expression
    {
        public Expression Condition { get; }
        public Expression[] Contents { get; }

        public override bool Static => Condition.Static && Contents.All(x => x.Static);

        public WhileLoopExpression(Expression condition, Expression[] contents)
        {
            Condition = condition;
            Contents = contents;
        }

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            while (Condition.Evaluate(ascentVariableMap, ascentScriptData).GetValue<float>() > 0.5f)
            {
                foreach (var expression in Contents)
                {
                    var map = ascentScriptData?.Clone();
                    expression.Evaluate(ascentVariableMap, map);
                    foreach (var item in ascentScriptData.Variables.Select(x => x.Key).ToList())
                    {
                        ascentScriptData.Variables[item] = map.Variables[item];
                    }
                }
            }
            return null;
        }
    }

    public class TernaryExpression : Expression
    {
        public Expression Condition { get; set; }
        public Expression TrueExpression { get; set; }
        public Expression FalseExpression { get; set; }

        public override bool Static => Condition.Static && TrueExpression.Static && FalseExpression.Static;

        public TernaryExpression(Expression condition, Expression trueExpr, Expression falseExpr)
        {
            Condition = condition;
            TrueExpression = trueExpr;
            FalseExpression = falseExpr;
        }

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            var conditionValue = Condition.Evaluate(ascentVariableMap, ascentScriptData).GetValue<float>();

            // Evaluate based on the condition value
            if (conditionValue == 1f)
            {
                return TrueExpression.Evaluate(ascentVariableMap, ascentScriptData);
            }
            else if (conditionValue == 0f)
            {
                return FalseExpression.Evaluate(ascentVariableMap, ascentScriptData);
            }
            else
            {
                throw new InvalidOperationException("Condition in ternary expression must evaluate to 0 or 1");
            }
        }
    }

    public class FunctionExpression : Expression
    {
        public Token FunctionToken { get; }
        public Expression[] Arguments { get; }

        public override bool Static => false;

        public FunctionExpression(Token functionToken, Expression[] arguments)
        {
            FunctionToken = functionToken;
            Arguments = arguments;
        }

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            var name = FunctionToken.TokenBuffer;
            var function = AscentFunctions.Functions.GetValueOrDefault(name);
            Variable?[] args = Arguments.Select(x => x.Evaluate(ascentVariableMap, ascentScriptData)).ToArray();
            if (function != null) return function.Evaluate(args);
            if (ascentScriptData.Functions.TryGetValue(name, out var expressions))
            {
                for (int i = 0; i < expressions.Args.Count; i++)
                {
                    if (args.Length > i)
                    {
                        ascentScriptData.Variables[expressions.Args.ToList()[i]] = args[i];
                    }
                }
                Variable? result = default;
                foreach (var expression in expressions.Contents)
                {
                    var res = expression.Evaluate(ascentVariableMap, ascentScriptData);
                    if (res != null)
                    {
                        result = res;
                    }
                }

                for (int i = 0; i < expressions.Args.Count; i++)
                {
                    if (args.Length > i)
                    {
                        ascentScriptData.Variables.Remove(expressions.Args.ToList()[i]);
                    }
                }
                return result;
            }
            else
            {
                throw new ArgumentException($"Function {name} does not exist!");
            }
        }
    }

    public class AssignmentExpression : Expression
    {
        public Token VariableToken { get; }
        public Expression Assignment { get; }

        public AssignmentExpression(Token variableToken, Expression assignment)
        {
            VariableToken = variableToken;
            Assignment = assignment;
        }

        public override bool Static => Assignment.Static;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            if (ascentVariableMap == null)
            {
                throw new InvalidOperationException("Variable map cannot be null");
            }
            if (Assignment == null)
            {
                throw new InvalidOperationException("Assignment Expression cannot be null");
            }
            ascentScriptData.Variables[VariableToken.TokenBuffer] = Assignment?.Evaluate(ascentVariableMap, ascentScriptData) ?? 0f;
            return null;
        }
    }

    public class VariableExpression : Expression
    {
        public Token VariableToken { get; }

        public VariableExpression(Token variableToken)
        {
            VariableToken = variableToken;
        }

        public override bool Static => false;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            if (ascentVariableMap == null)
            {
                throw new InvalidOperationException("Variable map cannot be null");
            }
            if (ascentScriptData.Variables.TryGetValue(VariableToken.TokenBuffer, out Variable value))
            {
                return value;
            }
            if (ascentVariableMap.ImportVariables.TryGetValue(VariableToken.TokenBuffer, out ImportVar importValue))
            {
                var variable = new Variable();
                variable.SetValue(importValue.value);
                return variable;
            }
            if (ascentVariableMap.ImportVariablesUnity.TryGetValue(VariableToken.TokenBuffer, out ImportVarUnity unityImportValue))
            {
                return new Variable(VarType.Object, unityImportValue.value);
            }
            return null;
        }
    }

    public class IncrementVariableExpression : Expression
    {
        public Token VariableToken { get; }

        public IncrementVariableExpression(Token variableToken)
        {
            VariableToken = variableToken;
        }

        public override bool Static => false;

        public override Variable? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData ascentScriptData)
        {
            if (ascentVariableMap == null)
            {
                throw new InvalidOperationException("Variable map cannot be null");
            }

            if (!ascentScriptData.Variables.TryGetValue(VariableToken.TokenBuffer, out var value)) return null;
            
            ascentScriptData.Variables[VariableToken.TokenBuffer] = value.GetValue<float>() + 1;
            return ascentScriptData.Variables[VariableToken.TokenBuffer];
        }
    }

    public class DecrementVariableExpression : Expression
    {
        public Token VariableToken { get; }

        public DecrementVariableExpression(Token variableToken)
        {
            VariableToken = variableToken;
        }

        public override bool Static => false;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            if (ascentVariableMap == null)
            {
                throw new InvalidOperationException("Variable map cannot be null");
            }
            if (ascentScriptData.Variables.TryGetValue(VariableToken.TokenBuffer, out Variable value))
            {
                ascentScriptData.Variables[VariableToken.TokenBuffer] = value.GetValue<float>() - 1;
                return ascentScriptData.Variables[VariableToken.TokenBuffer];
            }
            return null;
        }
    }

    public class AdditionAssignmentExpression : Expression
    {
        public Token VariableToken { get; }
        public Expression Expression { get; }

        public AdditionAssignmentExpression(Token variableToken, Expression expression)
        {
            VariableToken = variableToken;
            Expression = expression;
        }

        public override bool Static => false;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            if (ascentVariableMap == null)
            {
                throw new InvalidOperationException("Variable map cannot be null");
            }
            if (ascentScriptData.Variables.TryGetValue(VariableToken.TokenBuffer, out Variable value))
            {
                ascentScriptData.Variables[VariableToken.TokenBuffer] = value.GetValue<float>() + (Expression.Evaluate(ascentVariableMap, ascentScriptData)?.GetValue<float>() ?? 0f);
                return ascentScriptData.Variables[VariableToken.TokenBuffer];
            }
            return null;
        }
    }

    public class SubtractionAssignmentExpression : Expression
    {
        public Token VariableToken { get; }
        public Expression Expression { get; }

        public SubtractionAssignmentExpression(Token variableToken, Expression expression)
        {
            VariableToken = variableToken;
            Expression = expression;
        }

        public override bool Static => false;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            if (ascentVariableMap == null)
            {
                throw new InvalidOperationException("Variable map cannot be null");
            }
            if (ascentScriptData.Variables.TryGetValue(VariableToken.TokenBuffer, out Variable value))
            {
                ascentScriptData.Variables[VariableToken.TokenBuffer] = value.GetValue<float>() - (Expression.Evaluate(ascentVariableMap, ascentScriptData)?.GetValue<float>() ?? 0f);
                return ascentScriptData.Variables[VariableToken.TokenBuffer];
            }
            return null;
        }
    }

    public class NilExpression : Expression
    {
        public Token Token { get; }

        public NilExpression(Token token)
        {
            Token = token;
        }

        public override bool Static => true;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            return null;
        }
    }

    public class ReturnExpression : Expression
    {
        public Expression Expression { get; }

        public ReturnExpression(Expression expression)
        {
            Expression = expression;
        }

        public override bool Static => Expression.Static;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            return Expression.Evaluate(ascentVariableMap, ascentScriptData);
        }
    }

    public class NamespaceExpression : Expression
    {
        public string name { get; }

        public NamespaceExpression(Token token)
        {
            name = token.TokenBuffer;
        }

        public override bool Static => false;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            return null;
        }
    }

    public class UsingExpression : Expression
    {
        public string predicate { get; }

        public UsingExpression(Token token)
        {
            predicate = token.TokenBuffer;
        }

        public override bool Static => false;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            return null;
        }
    }

    public class ImportExpression : Expression
    {
        public string name { get; }
        public string type { get; }

        public ImportExpression(Token token)
        {
            var splits = token.TokenBuffer.Split('^');
            name = splits[0];
            type = splits[1];
        }

        public override bool Static => false;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            if (ascentVariableMap.ImportVariables.TryGetValue(name, out var importVar))
            {
                //Debug.Log("A " + name + " " + importVar.Name + " is imported.");
                VarType inputType = (VarType)importVar.type;
                ascentScriptData.Variables[name] = new Variable(inputType, importVar.Get());
            }
#if UNITY_5_3_OR_NEWER
            else if (ascentVariableMap.ImportVariablesUnity.TryGetValue(name, out var importVarUnity))
            {
                Debug.Log("B " + name);
                ascentScriptData.Variables[name] = new Variable(VarType.Object, importVarUnity.value);
            }
#endif
            else
            {
                Debug.Log("C " + name);
                ascentScriptData.Variables[name] = new Variable(VarType.Object, null);
            }
            return null;
        }
    }

    public class AccessExpression : Expression
    {
        public Expression Left { get; }
        public Token Right { get; }

        public AccessExpression(Expression left, Token right)
        {
            Left = left;
            Right = right;
        }

        public override bool Static => false;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            var result = Left.Evaluate(ascentVariableMap, ascentScriptData);
            var type = result.Value.GetType();
            var field = type.GetField(Right.TokenBuffer);
            if (field == null)
            {
                var property = type.GetProperty(Right.TokenBuffer);
                if (property == null)
                {
                    throw new InvalidOperationException($"Field {Right.TokenBuffer} not found in type {type.Name}");
                }
                else
                {
                    var resultValue = property.GetValue(result.Value);
                    Variable var = new Variable();
                    var.SetValue(resultValue);
                    return var;
                }
            }
            else
            {
                var resultValue = field.GetValue(result.Value);
                Variable var = new Variable();
                var.SetValue(resultValue);
                return var;
            }
        }
    }

    public class AccessAssignmentExpression : Expression
    {
        public AccessExpression Left { get; }
        public Expression Right { get; }

        public AccessAssignmentExpression(AccessExpression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        public override bool Static => false;

        public override Variable? Evaluate(AscentVariableMap ascentVariableMap, AscentScriptData ascentScriptData)
        {
            Debug.Log(Left.GetType().Name);
            Debug.Log(Left.Left.GetType().Name);
            Debug.Log(Left.Left.Evaluate(ascentVariableMap, ascentScriptData).Value.GetType().Name);
            var leftObject = Left.Left.Evaluate(ascentVariableMap, ascentScriptData).Value;
            
            var leftAccess = Left.Right;

            var rightResult = Right.Evaluate(ascentVariableMap, ascentScriptData).Value;

            var type = leftObject.GetType();
            var field = type.GetField(leftAccess.TokenBuffer);
            if (field == null)
            {
                var property = type.GetProperty(leftAccess.TokenBuffer);
                if (property == null)
                {
                    throw new InvalidOperationException($"Field {leftAccess.TokenBuffer} not found in type {type.Name}");
                }
                else
                {
                    property.SetValue(leftObject, rightResult);
                }
            }
            else
            {
                field.SetValue(leftObject, rightResult);
            }
            return null;
        }
    }
}