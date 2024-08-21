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
#if UNITY
using UnityEngine;
#endif

namespace AscentLanguage.Parser
{
    public abstract class Expression
    {
        public abstract bool Static { get; }
        public abstract Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData);
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            var LeftValue = Left.Evaluate(ascentVariableMap, ascentScriptData);
            var RightValue = Right.Evaluate(ascentVariableMap, ascentScriptData);

            //Ensure float for all operations and string for concatenation
            if (Operator.type == TokenType.Multiplication)
            {
                if (LeftValue.Type == VarType.Float && RightValue.Type == VarType.Float)
                {
                    Var result = new Var();
                    result.SetValue(LeftValue.GetValue<float>() * RightValue.GetValue<float>(), VarType.Float);
                    return result;
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Multiplication requires two floats. Left Type {0}. Right Type {1}.", LeftValue.Type.ToString(), RightValue.Type.ToString()));
                }
            }

            if (Operator.type == TokenType.Division)
            {
                if (LeftValue.Type == VarType.Float && RightValue.Type == VarType.Float)
                {
                    Var result = new Var();
                    result.SetValue(LeftValue.GetValue<float>() / RightValue.GetValue<float>(), VarType.Float);
                    return result;
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Division requires two floats. Left Type {0}. Right Type {1}.", LeftValue.Type.ToString(), RightValue.Type.ToString()));
                }
            }

            if (Operator.type == TokenType.Addition)
            {
                if (LeftValue.Type == VarType.Float && RightValue.Type == VarType.Float)
                {
                    Var result = new Var();
                    result.SetValue(LeftValue.GetValue<float>() + RightValue.GetValue<float>(), VarType.Float);
                    return result;
                }
                else
                {
                    //check for implicit addition.
                    var leftType = LeftValue.Value.GetType();
                    var implicitAdd = leftType.GetMethod("op_Addition");
                    if (implicitAdd != null)
                    {
                        var resultValue = implicitAdd.Invoke(null, new object[] { LeftValue.Value, RightValue.Value });
                        Var var = new Var();
                        var.SetValue(resultValue);
                        return var;
                    }
                    else
                    {
                        Var result = new Var();
                        result.SetValue(LeftValue.ToString() + RightValue.ToString(), VarType.String);
                        return result;
                    }
                }
            }

            if (Operator.type == TokenType.Subtraction)
            {
                if (LeftValue.Type == VarType.Float && RightValue.Type == VarType.Float)
                {
                    Var result = new Var();
                    result.SetValue(LeftValue.GetValue<float>() - RightValue.GetValue<float>(), VarType.Float);
                    return result;
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Subtraction requires two floats. Left Type {0}. Right Type {1}.", LeftValue.Type.ToString(), RightValue.Type.ToString()));
                }
            }

            if (Operator.type == TokenType.Pow)
            {
                if (LeftValue.Type == VarType.Float && RightValue.Type == VarType.Float)
                {
                    Var result = new Var();
                    result.SetValue((float)Math.Pow(LeftValue.GetValue<float>(), RightValue.GetValue<float>()), VarType.Float);
                    return result;
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Pow requires two floats. Left Type {0}. Right Type {1}.", LeftValue.Type.ToString(), RightValue.Type.ToString()));
                }
            }

            if (Operator.type == TokenType.Modulus)
            {
                if (LeftValue.Type == VarType.Float && RightValue.Type == VarType.Float)
                {
                    Var result = new Var();
                    result.SetValue(LeftValue.GetValue<float>() % RightValue.GetValue<float>(), VarType.Float);
                    return result;
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Modulus requires two floats. Left Type {0}. Right Type {1}.", LeftValue.Type.ToString(), RightValue.Type.ToString()));
                }
            }

            if (Operator.type == TokenType.GreaterThan)
            {
                if (LeftValue.Type == VarType.Float && RightValue.Type == VarType.Float)
                {
                    Var result = new Var();
                    result.SetValue(LeftValue.GetValue<float>() > RightValue.GetValue<float>(), VarType.Bool);
                    return result;
                }
                else
                {
                    throw new InvalidOperationException(String.Format("GreaterThan requires two floats. Left Type {0}. Right Type {1}.", LeftValue.Type.ToString(), RightValue.Type.ToString()));
                }
            }

            if (Operator.type == TokenType.LesserThen)
            {
                if (LeftValue.Type == VarType.Float && RightValue.Type == VarType.Float)
                {
                    Var result = new Var();
                    result.SetValue(LeftValue.GetValue<float>() < RightValue.GetValue<float>(), VarType.Bool);
                    return result;
                }
                else
                {
                    throw new InvalidOperationException(String.Format("LesserThen requires two floats. Left Type {0}. Right Type {1}.", LeftValue.Type.ToString(), RightValue.Type.ToString()));
                }
            }

            throw new InvalidOperationException($"Unsupported operator: {Operator.tokenBuffer}");
        }
    }

    public class ConstantExpression : Expression
    {
        public Token Token { get; set; }

        public ConstantExpression(Token token)
        {
            Token = token;
        }

        public override bool Static => Token.type != TokenType.Query;

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            switch (Token.type)
            {
                case TokenType.Constant:
                    Var result = new Var();
                    result.SetValue(Utility.ConvertToFloat(Token.tokenBuffer), VarType.Float);
                    return result;
                case TokenType.True:
                    Var resultTrue = new Var();
                    resultTrue.SetValue(true, VarType.Bool);
                    return resultTrue;
                case TokenType.False:
                    Var resultFalse = new Var();
                    resultFalse.SetValue(false, VarType.Bool);
                    return resultFalse;
                case TokenType.String:
                    Var resultString = new Var();
                    resultString.SetValue(Token.tokenBuffer, VarType.String);
                    return resultString;
                case TokenType.Query:
                    if (ascentVariableMap != null && ascentVariableMap.QueryVariables.TryGetValue(Token.tokenBuffer, out Var value))
                    {
                        return value;
                    }
                    else
                    {
                        Console.WriteLine($"Variable {Token.tokenBuffer} ({Token.tokenBuffer.Length}) not found in variable map");
                        return 0f;
                    }
                default:
                    break;
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            var name = FunctionToken.tokenBuffer;
            var definition = ascentScriptData.Functions.FirstOrDefault(x => x.Key == name);
            if (definition.Value != null)
            {
                definition.Value.contents = Contents;
                definition.Value.defined = true;
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            Defintion.Evaluate(ascentVariableMap, ascentScriptData);
            while (Condition.Evaluate(ascentVariableMap, ascentScriptData).GetValue<float>() > 0.5f)
            {
                foreach (var expression in Contents)
                {
                    var map = ascentScriptData?.Clone();
                    expression.Evaluate(ascentVariableMap, map);
                    foreach (var item in ascentScriptData.Variables.Select(x => x.Key))
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            while (Condition.Evaluate(ascentVariableMap, ascentScriptData).GetValue<float>() > 0.5f)
            {
                foreach (var expression in Contents)
                {
                    var map = ascentScriptData?.Clone();
                    expression.Evaluate(ascentVariableMap, map);
                    foreach (var item in ascentScriptData.Variables.Select(x => x.Key))
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            var name = FunctionToken.tokenBuffer;
            var function = AscentFunctions.GetFunction(name);
            Var[] args = Arguments.Select(x => x.Evaluate(ascentVariableMap, ascentScriptData)).ToArray();
            if (function == null)
            {
                if (ascentScriptData != null && ascentScriptData.Functions.TryGetValue(name, out var expressions))
                {
                    for (int i = 0; i < expressions.args.Count; i++)
                    {
                        if (args.Length > i)
                        {
                            ascentScriptData.Variables[expressions.args[i]] = args[i];
                        }
                    }
                    Var result = default;
                    foreach (var expression in expressions.contents)
                    {
                        var res = expression.Evaluate(ascentVariableMap, ascentScriptData);
                        if (res != null)
                        {
                            result = res;
                        }
                    }

                    for (int i = 0; i < expressions.args.Count; i++)
                    {
                        if (args.Length > i)
                        {
                            ascentScriptData.Variables.Remove(expressions.args[i]);
                        }
                    }
                    return result;
                }
                else
                {
                    throw new ArgumentException($"Function {name} does not exist!");
                }
            }
            return function.Evaluate(args);
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            if (ascentVariableMap == null)
            {
                throw new InvalidOperationException("Variable map cannot be null");
            }
            if (Assignment == null)
            {
                throw new InvalidOperationException("Assignment Expression cannot be null");
            }
            ascentScriptData.Variables[VariableToken.tokenBuffer] = Assignment?.Evaluate(ascentVariableMap, ascentScriptData) ?? 0f;
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            if (ascentVariableMap == null)
            {
                throw new InvalidOperationException("Variable map cannot be null");
            }
            if (ascentScriptData.Variables.TryGetValue(VariableToken.tokenBuffer, out Var value))
            {
                return value;
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            if (ascentVariableMap == null)
            {
                throw new InvalidOperationException("Variable map cannot be null");
            }
            if (ascentScriptData.Variables.TryGetValue(VariableToken.tokenBuffer, out Var value))
            {
                ascentScriptData.Variables[VariableToken.tokenBuffer] = value.GetValue<float>() + 1;
                return ascentScriptData.Variables[VariableToken.tokenBuffer];
            }
            return null;
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            if (ascentVariableMap == null)
            {
                throw new InvalidOperationException("Variable map cannot be null");
            }
            if (ascentScriptData.Variables.TryGetValue(VariableToken.tokenBuffer, out Var value))
            {
                ascentScriptData.Variables[VariableToken.tokenBuffer] = value.GetValue<float>() - 1;
                return ascentScriptData.Variables[VariableToken.tokenBuffer];
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            if (ascentVariableMap == null)
            {
                throw new InvalidOperationException("Variable map cannot be null");
            }
            if (ascentScriptData.Variables.TryGetValue(VariableToken.tokenBuffer, out Var value))
            {
                ascentScriptData.Variables[VariableToken.tokenBuffer] = value.GetValue<float>() + (Expression.Evaluate(ascentVariableMap, ascentScriptData)?.GetValue<float>() ?? 0f);
                return ascentScriptData.Variables[VariableToken.tokenBuffer];
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            if (ascentVariableMap == null)
            {
                throw new InvalidOperationException("Variable map cannot be null");
            }
            if (ascentScriptData.Variables.TryGetValue(VariableToken.tokenBuffer, out Var value))
            {
                ascentScriptData.Variables[VariableToken.tokenBuffer] = value.GetValue<float>() - (Expression.Evaluate(ascentVariableMap, ascentScriptData)?.GetValue<float>() ?? 0f);
                return ascentScriptData.Variables[VariableToken.tokenBuffer];
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            return Expression.Evaluate(ascentVariableMap, ascentScriptData);
        }
    }

    public class NamespaceExpression : Expression
    {
        public string name { get; }

        public NamespaceExpression(Token token)
        {
            name = token.tokenBuffer;
        }

        public override bool Static => false;

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            return null;
        }
    }

    public class UsingExpression : Expression
    {
        public string predicate { get; }

        public UsingExpression(Token token)
        {
            predicate = token.tokenBuffer;
        }

        public override bool Static => false;

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
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
            var splits = token.tokenBuffer.Split('^');
            name = splits[0];
            type = splits[1];
        }

        public override bool Static => false;

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            if (ascentVariableMap.ImportVariables.TryGetValue(name, out var importVar))
            {
                VarType inputType = (VarType)importVar.type;
                ascentScriptData.Variables[name] = new Var(inputType, importVar.Get());
            }
#if UNITY
            else if (ascentVariableMap.ImportVariablesUnity.TryGetValue(name, out var importVarUnity))
            {
                ascentScriptData.Variables[name] = new Var(VarType.Object, importVarUnity.value);
            }
#endif
            else
            {
                ascentScriptData.Variables[name] = new Var(VarType.Object, null);
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            var result = Left.Evaluate(ascentVariableMap, ascentScriptData);
            var type = result.Value.GetType();
            var field = type.GetField(Right.tokenBuffer);
            if (field == null)
            {
                var property = type.GetProperty(Right.tokenBuffer);
                if (property == null)
                {
                    throw new InvalidOperationException($"Field {Right.tokenBuffer} not found in type {type.Name}");
                }
                else
                {
                    var resultValue = property.GetValue(result.Value);
                    Var var = new Var();
                    var.SetValue(resultValue);
                    return var;
                }
            }
            else
            {
                var resultValue = field.GetValue(result.Value);
                Var var = new Var();
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

        public override Var? Evaluate(AscentVariableMap? ascentVariableMap, AscentScriptData? ascentScriptData)
        {
            var leftObject = Left.Left.Evaluate(ascentVariableMap, ascentScriptData).Value;
            var leftAccess = Left.Right;

            var rightResult = Right.Evaluate(ascentVariableMap, ascentScriptData).Value;

            var type = leftObject.GetType();
            var field = type.GetField(leftAccess.tokenBuffer);
            if (field == null)
            {
                var property = type.GetProperty(leftAccess.tokenBuffer);
                if (property == null)
                {
                    throw new InvalidOperationException($"Field {leftAccess.tokenBuffer} not found in type {type.Name}");
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