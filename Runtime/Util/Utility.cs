using AscentLanguage.Parser;
using AscentLanguage.Splitter;
using System;
using System.Collections.Generic;
using System.Linq;
using KTrie;
using UnityEditor.Build;

namespace AscentLanguage.Util
{
    public static class Utility
    {
        // Ensures that a key leads to a plausible route but does not actually contain the key yet.
        // Used in the tokenizer to prevent over-reading past the trie key.
        public static bool PartialContains(this Trie trie, string key)
        {
            return trie.StartsWith(key).Any() && !trie.Contains(key);
        }
        public static bool PartialContains<T>(this TrieDictionary<T> trie, string key)
        {
            return trie.StartsWith(key).Any() && !trie.ContainsKey(key);
        }
        
        //TODO: Can we optimize this?
        public static float ConvertToFloat(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Input string is null or empty.");
            }

            var isNegative = value[0] == '-';
            var startIndex = isNegative ? 1 : 0;
            var valueLength = value.Length;

            if (startIndex == valueLength)
            {
                throw new FormatException("Input string contains only a negative sign.");
            }

            var result = 0;
            var isFractionalPart = false;
            var fractionalDivisor = 1;

            for (var i = startIndex; i < valueLength; i++)
            {
                var c = value[i];

                switch (c)
                {
                    case '.' when isFractionalPart:
                        throw new FormatException("Input string contains multiple decimal points.");
                    case '.':
                        isFractionalPart = true;
                        continue;
                    case < '0':
                    case > '9':
                        throw new FormatException($"Invalid character '{c}' in input string.");
                }

                var digit = c - '0';

                if (isFractionalPart)
                {
                    fractionalDivisor *= 10;
                    result += digit / fractionalDivisor;
                }
                else
                {
                    result = result * 10 + digit;
                }
            }

            return isNegative ? -result : result;
        }

        public static void PrintExpression(Expression expr)
        {
            PrintExpression(expr, 0);
        }

        private static void PrintExpression(Expression expr, int indentLevel)
        {
            switch (expr)
            {
                case ConstantExpression numberExpr:
                    Console.WriteLine($"{GetIndent(indentLevel)}Constant: {numberExpr.Token.TokenBuffer}");
                    break;
                case BinaryExpression binaryExpr:
                    Console.WriteLine($"{GetIndent(indentLevel)}Binary Expression:");
                    Console.WriteLine($"{GetIndent(indentLevel + 2)}Operator: {binaryExpr.Operator.TokenBuffer}");
                    PrintExpression(binaryExpr.Left, indentLevel + 2);
                    PrintExpression(binaryExpr.Right, indentLevel + 2);
                    break;
                case TernaryExpression ternaryExpr:
                    Console.WriteLine($"{GetIndent(indentLevel)}Ternary Expression:");
                    PrintExpression(ternaryExpr.Condition, indentLevel + 2);
                    Console.WriteLine($"{GetIndent(indentLevel + 2)}True Expression:");
                    PrintExpression(ternaryExpr.TrueExpression, indentLevel + 4);
                    Console.WriteLine($"{GetIndent(indentLevel + 2)}False Expression:");
                    PrintExpression(ternaryExpr.FalseExpression, indentLevel + 4);
                    break;
                case FunctionExpression functionExpr:
                {
                    Console.WriteLine($"{GetIndent(indentLevel)}Function:");
                    Console.WriteLine($"{GetIndent(indentLevel + 4)}Type: {functionExpr.FunctionToken.TokenBuffer}");
                    Console.WriteLine($"{GetIndent(indentLevel + 2)}Argument Expression:");
                    foreach (var arg in functionExpr.Arguments)
                    {
                        PrintExpression(arg, indentLevel + 4);
                    }

                    break;
                }
                case FunctionDefinitionExpression functionDefExpr:
                {
                    Console.WriteLine($"{GetIndent(indentLevel)}Function Definition:");
                    Console.WriteLine($"{GetIndent(indentLevel + 4)}Name: {functionDefExpr.FunctionToken.TokenBuffer}");
                    Console.WriteLine($"{GetIndent(indentLevel + 2)}Expressions:");
                    foreach (var content in functionDefExpr.Contents)
                    {
                        PrintExpression(content, indentLevel + 4);
                    }

                    break;
                }
                case AssignmentExpression assignmentExpr:
                    Console.WriteLine($"{GetIndent(indentLevel)}Assignment:");
                    Console.WriteLine($"{GetIndent(indentLevel + 4)}Variable: {assignmentExpr.VariableToken.TokenBuffer}");
                    Console.WriteLine($"{GetIndent(indentLevel + 2)}Setting Expression:");
                    PrintExpression(assignmentExpr.Assignment, indentLevel + 4);
                    break;
                case VariableExpression variableExpr:
                    Console.WriteLine($"{GetIndent(indentLevel)}GrabVariable:");
                    Console.WriteLine($"{GetIndent(indentLevel + 4)}Variable: {variableExpr.VariableToken.TokenBuffer}");
                    break;
                case IncrementVariableExpression variableIncrementExpr:
                    Console.WriteLine($"{GetIndent(indentLevel)}GrabVariable(Incremement):");
                    Console.WriteLine($"{GetIndent(indentLevel + 4)}Variable: {variableIncrementExpr.VariableToken.TokenBuffer}");
                    break;
                case DecrementVariableExpression variableDecrementExpr:
                    Console.WriteLine($"{GetIndent(indentLevel)}GrabVariable(Decrement):");
                    Console.WriteLine($"{GetIndent(indentLevel + 4)}Variable: {variableDecrementExpr.VariableToken.TokenBuffer}");
                    break;
                case ReturnExpression returnExpr:
                    Console.WriteLine($"{GetIndent(indentLevel)}Return:");
                    PrintExpression(returnExpr.Expression, indentLevel + 2);
                    break;
                case ForLoopExpression forExpr:
                {
                    Console.WriteLine($"{GetIndent(indentLevel)}For Loop:");
                    Console.WriteLine($"{GetIndent(indentLevel + 2)}Definition:");
                    PrintExpression(forExpr.Defintion, indentLevel + 4);
                    Console.WriteLine($"{GetIndent(indentLevel + 2)}Condition:");
                    PrintExpression(forExpr.Condition, indentLevel + 4);
                    Console.WriteLine($"{GetIndent(indentLevel + 2)}Suffix:");
                    PrintExpression(forExpr.Suffix, indentLevel + 4);
                    Console.WriteLine($"{GetIndent(indentLevel + 2)}Expressions:");
                    foreach (var content in forExpr.Contents)
                    {
                        PrintExpression(content, indentLevel + 4);
                    }

                    break;
                }
                case WhileLoopExpression whileExpr:
                {
                    Console.WriteLine($"{GetIndent(indentLevel)}While Loop:");
                    Console.WriteLine($"{GetIndent(indentLevel + 2)}Condition:");
                    PrintExpression(whileExpr.Condition, indentLevel + 4);
                    Console.WriteLine($"{GetIndent(indentLevel + 2)}Expressions:");
                    foreach (var content in whileExpr.Contents)
                    {
                        PrintExpression(content, indentLevel + 4);
                    }

                    break;
                }
            }
        }

        private static string GetIndent(int indentLevel)
        {
            return new string(' ', indentLevel * 2);
        }

        public static void PrintTokenContainer(TokenContainer container, int indentLevel = 0)
        {
            switch (container)
            {
                case SingleTokenContainer single:
                {
                    Console.WriteLine($"{GetIndent(indentLevel)}SingleTokenContainer:");
                    Console.Write($"{GetIndent(indentLevel + 2)}");
                    for (var i = 0; i < single.Expression.Length; i++)
                    {
                        Console.Write($"{single.Expression[i].Type}, ");
                    }
                    Console.Write("\n");
                    break;
                }
                case MultipleTokenContainer multiple:
                {
                    Console.WriteLine($"{GetIndent(indentLevel)}MultipleTokenContainer:");
                    foreach (var printContainer in multiple.TokenContainers)
                    {
                        PrintTokenContainer(printContainer, indentLevel + 2);
                    }

                    break;
                }
                default:
                    throw new InvalidOperationException("Invalid container type");
            }
        }
    }
}
