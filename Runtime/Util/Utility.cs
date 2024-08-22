using AscentLanguage.Parser;
using AscentLanguage.Splitter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AscentLanguage.Util
{
    public static class Utility
    {
        public static bool SearchForPotential(char c, IEnumerable<string> strings)
        {
            return strings.Any(x => x.StartsWith(c.ToString()));
        }

        public static float ConvertToFloat(string value)
        {
            var valueLength = value.Length;

            if (value == null || valueLength == 0)
            {
                throw new ArgumentException("Input char array is null or empty.");
            }

            bool isNegative = value[0] == '-';
            int startIndex = isNegative ? 1 : 0;

            if (startIndex == valueLength)
            {
                throw new FormatException("Input char array contains only a negative sign.");
            }

            float result = 0;
            bool isFractionalPart = false;
            float fractionalDivisor = 10;

            for (int i = startIndex; i < valueLength; i++)
            {
                char c = value[i];

                if (c == '.')
                {
                    if (isFractionalPart)
                    {
                        throw new FormatException("Input char array contains multiple decimal points.");
                    }
                    isFractionalPart = true;
                    continue;
                }

                if (c < '0' || c > '9')
                {
                    throw new FormatException($"Invalid character '{c}' in input char array.");
                }

                int digit = c - '0';

                try
                {
                    checked
                    {
                        if (isFractionalPart)
                        {
                            result += digit / fractionalDivisor;
                            fractionalDivisor *= 10;
                        }
                        else
                        {
                            result = result * 10 + digit;
                        }
                    }
                }
                catch (OverflowException)
                {
                    throw new OverflowException("Overflow occurred while converting char array to float.");
                }
            }

            return isNegative ? -result : result;
        }

        public static int FindLengthToUse(char[] charArray)
        {
            int length = 0;
            while (length < charArray.Length && charArray[length] != '\0')
            {
                length++;
            }
            return length;
        }
        public static void PrintExpression(Expression expr)
        {
            PrintExpression(expr, 0);
        }

        private static void PrintExpression(Expression expr, int indentLevel)
        {
            if (expr is ConstantExpression numberExpr)
            {
                ;
                Console.WriteLine($"{GetIndent(indentLevel)}Constant: {numberExpr.Token.TokenBuffer}");
            }
            else if (expr is BinaryExpression binaryExpr)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}Binary Expression:");
                Console.WriteLine($"{GetIndent(indentLevel + 2)}Operator: {binaryExpr.Operator.TokenBuffer}");
                PrintExpression(binaryExpr.Left, indentLevel + 2);
                PrintExpression(binaryExpr.Right, indentLevel + 2);
            }
            else if (expr is TernaryExpression ternaryExpr)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}Ternary Expression:");
                PrintExpression(ternaryExpr.Condition, indentLevel + 2);
                Console.WriteLine($"{GetIndent(indentLevel + 2)}True Expression:");
                PrintExpression(ternaryExpr.TrueExpression, indentLevel + 4);
                Console.WriteLine($"{GetIndent(indentLevel + 2)}False Expression:");
                PrintExpression(ternaryExpr.FalseExpression, indentLevel + 4);
            }
            else if (expr is FunctionExpression functionExpr)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}Function:");
                Console.WriteLine($"{GetIndent(indentLevel + 4)}Type: {functionExpr.FunctionToken.TokenBuffer}");
                Console.WriteLine($"{GetIndent(indentLevel + 2)}Argument Expression:");
                for (int i = 0; i < functionExpr.Arguments.Length; i++)
                {
                    PrintExpression(functionExpr.Arguments[i], indentLevel + 4);
                }
            }
            else if (expr is FunctionDefinitionExpression functionDefExpr)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}Function Definition:");
                Console.WriteLine($"{GetIndent(indentLevel + 4)}Name: {functionDefExpr.FunctionToken.TokenBuffer}");
                Console.WriteLine($"{GetIndent(indentLevel + 2)}Expressions:");
                for (int i = 0; i < functionDefExpr.Contents.Length; i++)
                {
                    PrintExpression(functionDefExpr.Contents[i], indentLevel + 4);
                }
            }
            else if (expr is AssignmentExpression assignmentExpr)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}Assignment:");
                Console.WriteLine($"{GetIndent(indentLevel + 4)}Variable: {assignmentExpr.VariableToken.TokenBuffer}");
                Console.WriteLine($"{GetIndent(indentLevel + 2)}Setting Expression:");
                PrintExpression(assignmentExpr.Assignment, indentLevel + 4);
            }
            else if (expr is VariableExpression variableExpr)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}GrabVariable:");
                Console.WriteLine($"{GetIndent(indentLevel + 4)}Variable: {variableExpr.VariableToken.TokenBuffer}");
            }
            else if (expr is IncrementVariableExpression variableIncrementExpr)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}GrabVariable(Incremement):");
                Console.WriteLine($"{GetIndent(indentLevel + 4)}Variable: {variableIncrementExpr.VariableToken.TokenBuffer}");
            }
            else if (expr is DecrementVariableExpression variableDecrementExpr)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}GrabVariable(Decrement):");
                Console.WriteLine($"{GetIndent(indentLevel + 4)}Variable: {variableDecrementExpr.VariableToken.TokenBuffer}");
            }
            else if (expr is ReturnExpression returnExpr)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}Return:");
                PrintExpression(returnExpr.Expression, indentLevel + 2);
            }
            else if (expr is ForLoopExpression forExpr)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}For Loop:");
                Console.WriteLine($"{GetIndent(indentLevel + 2)}Definition:");
                PrintExpression(forExpr.Defintion, indentLevel + 4);
                Console.WriteLine($"{GetIndent(indentLevel + 2)}Condition:");
                PrintExpression(forExpr.Condition, indentLevel + 4);
                Console.WriteLine($"{GetIndent(indentLevel + 2)}Suffix:");
                PrintExpression(forExpr.Suffix, indentLevel + 4);
                Console.WriteLine($"{GetIndent(indentLevel + 2)}Expressions:");
                for (int i = 0; i < forExpr.Contents.Length; i++)
                {
                    PrintExpression(forExpr.Contents[i], indentLevel + 4);
                }
            }
            else if (expr is WhileLoopExpression whileExpr)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}While Loop:");
                Console.WriteLine($"{GetIndent(indentLevel + 2)}Condition:");
                PrintExpression(whileExpr.Condition, indentLevel + 4);
                Console.WriteLine($"{GetIndent(indentLevel + 2)}Expressions:");
                for (int i = 0; i < whileExpr.Contents.Length; i++)
                {
                    PrintExpression(whileExpr.Contents[i], indentLevel + 4);
                }
            }
            else if (expr is NilExpression nilExpr)
            {
            }
            else
            {
            }
        }

        private static string GetIndent(int indentLevel)
        {
            return new string(' ', indentLevel * 2);
        }

        public static void PrintTokenContainer(TokenContainer container, int indentLevel = 0)
        {
            if (container is SingleTokenContainer single)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}SingleTokenContainer:");
                Console.Write($"{GetIndent(indentLevel + 2)}");
                for (int i = 0; i < single.Expression.Length; i++)
                {
                    Console.Write($"{single.Expression[i].Type}, ");
                }
                Console.Write("\n");
            }
            else if (container is MultipleTokenContainer multiple)
            {
                Console.WriteLine($"{GetIndent(indentLevel)}MultipleTokenContainer:");
                for (int i = 0; i < multiple.TokenContainers.Count; i++)
                {
                    PrintTokenContainer(multiple.TokenContainers[i], indentLevel + 2);
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid container type");
            }
        }
    }
}
