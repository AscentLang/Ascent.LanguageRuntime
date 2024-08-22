using AscentLanguage.Tokenizer;
using AscentLanguage.Splitter;
using System;
using System.Collections.Generic;
using System.Linq;
using AscentLanguage.Data;

namespace AscentLanguage.Parser
{
    public class AscentParser
    {
        private int _position;
        private readonly List<TokenContainer> _containerStack;
        private Token[] _currentTokens;

        // Define operator precedence levels
        private static readonly Dictionary<TokenType, int> precedence = new()
        {
            { TokenType.Pow, 9 },
            { TokenType.TernaryConditional, 5 },
            { TokenType.GreaterThan, 6 },
            { TokenType.LesserThen, 6 },
            { TokenType.Modulus, 3 },
            { TokenType.Multiplication, 3 },
            { TokenType.Division, 3 },
            { TokenType.Addition, 2 },
            { TokenType.Subtraction, 2 },
            { TokenType.Access, 1 },
        };

        public AscentParser(TokenContainer rootContainer)
        {
            _position = 0;
            _containerStack = new List<TokenContainer> { rootContainer };
            _currentTokens = Array.Empty<Token>();
            LoadNextContainer();
        }

        private void LoadNextContainer()
        {
            while (_position >= _currentTokens.Length && _containerStack.Count > 0)
            {
                var currentContainer = _containerStack[0];
                _containerStack.RemoveAt(0);

                switch (currentContainer)
                {
                    case SingleTokenContainer single:
                        _currentTokens = single.Expression;
                        _position = 0;
                        break;
                    case MultipleTokenContainer multiple:
                        _containerStack.InsertRange(0, multiple.TokenContainers);
                        break;
                }
            }
        }

        public List<Expression> Parse(AscentVariableMap variableMap, AscentScriptData scriptData)
        {
            var expressions = new List<Expression>();

            while (_containerStack.Count > 0 || _currentTokens.Length > 0)
            {
                LoadNextContainer();
                while (_position < _currentTokens.Length)
                {
                    var expression = ParseExpression(variableMap, scriptData);
                    if (expression != null)
                    {
                        expressions.Add(expression);
                    }
                    _currentTokens = Array.Empty<Token>();
                    _position++;
                }
            }

            return expressions;
        }

        private Expression ParseExpression(AscentVariableMap variableMap, AscentScriptData scriptData)
        {
            return ParseBinary(0, variableMap, scriptData);
        }

        private Expression ParseBinary(int opPrecedence, AscentVariableMap variableMap, AscentScriptData scriptData)
        {
            var left = ParsePrimary(variableMap, scriptData);

            while (true)
            {
                if (_position >= _currentTokens.Length || !precedence.ContainsKey(_currentTokens[_position].Type))
                {
                    break;
                }

                var operatorToken = _currentTokens[_position];
                var tokenPrecedence = precedence[operatorToken.Type];
                if (tokenPrecedence < opPrecedence)
                {
                    break;
                }

                _position++;
                if (operatorToken.Type == TokenType.TernaryConditional)
                {
                    var trueExpression = ParseExpression(variableMap, scriptData);

                    if (!CurrentTokenIs(TokenType.Colon))
                    {
                        throw new FormatException("Expected ':' in ternary expression");
                    }
                    _position++; // consume ':'

                    var falseExpression = ParseExpression(variableMap, scriptData);

                    return new TernaryExpression(left, trueExpression, falseExpression);
                }

                var right = ParseBinary(tokenPrecedence + 1, variableMap, scriptData);
                left = new BinaryExpression(left, operatorToken, right);
            }

            return left;
        }

        private Expression ParsePrimary(AscentVariableMap variableMap, AscentScriptData scriptData)
        {
            Expression left = null;

            if (CurrentTokenIs(TokenType.Constant) || CurrentTokenIs(TokenType.Query) ||
                CurrentTokenIs(TokenType.True) || CurrentTokenIs(TokenType.False) ||
                CurrentTokenIs(TokenType.String))
            {
                var constantToken = _currentTokens[_position++];
                left = new ConstantExpression(constantToken);
            }
            else if (CurrentTokenIs(TokenType.Variable))
            {
                var variableToken = _currentTokens[_position++];
                left = new VariableExpression(variableToken);
            }
            else if (CurrentTokenIs(TokenType.LeftParenthesis))
            {
                _position++; // consume '('
                left = ParseExpression(variableMap, scriptData);
                if (!CurrentTokenIs(TokenType.RightParenthesis))
                {
                    throw new FormatException("Missing closing parenthesis");
                }
                _position++; // consume ')'
            }
            else if (CurrentTokenIs(TokenType.LeftBracket))
            {
                _position++; // consume '['
                left = ParseExpression(variableMap, scriptData);
                if (!CurrentTokenIs(TokenType.RightBracket))
                {
                    throw new FormatException("Missing closing bracket");
                }
                _position++; // consume ']'
            }
            else if (CurrentTokenIs(TokenType.Function))
            {
                var functionToken = _currentTokens[_position++]; // Get the function token

                if (!CurrentTokenIs(TokenType.LeftParenthesis))
                {
                    throw new FormatException("Expected '(' after function");
                }
                _position++; // consume '('

                var arguments = ParseFunctionArguments(false, variableMap, scriptData);

                if (!CurrentTokenIs(TokenType.RightParenthesis))
                {
                    throw new FormatException("Missing closing parenthesis for function call");
                }
                _position++; // consume ')'

                left = new FunctionExpression(functionToken, arguments);
            }
            else if (CurrentTokenIs(TokenType.FunctionDefinition))
            {
                var functionToken = _currentTokens[_position++]; // Get the function token

                if (CurrentTokenIs(TokenType.LeftParenthesis))
                {
                    _position++; // consume '('
                    var arguments = ParseDefinitionArguments();
                    var name = functionToken.TokenBuffer;
                    var definition = new FunctionDefinition(name);
                    scriptData.Functions.Add(name, definition);
                    definition.Args = arguments.ToList();
                    _position++; // consume ')'
                }

                if (!CurrentTokenIs(TokenType.LeftScope))
                {
                    throw new FormatException("Expected '{' after function");
                }
                _position++; // consume '{'

                var contents = ParseFunctionArguments(true, variableMap, scriptData);

                if (!CurrentTokenIs(TokenType.RightScope))
                {
                    throw new FormatException("Missing closing scope for function call");
                }
                _position++; // consume '}'

                left = new FunctionDefinitionExpression(functionToken, contents);
            }
            else if (CurrentTokenIs(TokenType.ForLoop))
            {
                _position++; // consume 'for'
                Expression definition;
                Expression condition;
                Expression suffix;

                if (CurrentTokenIs(TokenType.LeftParenthesis))
                {
                    _position++; // consume '('
                    definition = ParseExpression(variableMap, scriptData);
                    if (CurrentTokenIs(TokenType.SemiColon))
                    {
                        _position++; // consume ';'
                    }
                    condition = ParseExpression(variableMap, scriptData);
                    if (CurrentTokenIs(TokenType.SemiColon))
                    {
                        _position++; // consume ';'
                    }
                    suffix = ParseExpression(variableMap, scriptData);
                    _position++; // consume ')'
                }
                else
                {
                    throw new FormatException("Expected '(' after for loop. Missing definition, condition, and suffix!");
                }

                if (!CurrentTokenIs(TokenType.LeftScope))
                {
                    throw new FormatException("Expected '{' after for loop");
                }
                _position++; // consume '{'

                var contents = ParseFunctionArguments(true, variableMap, scriptData);

                if (!CurrentTokenIs(TokenType.RightScope))
                {
                    throw new FormatException("Missing closing scope for loop");
                }
                _position++; // consume '}'

                left = new ForLoopExpression(definition, condition, suffix, contents);
            }
            else if (CurrentTokenIs(TokenType.WhileLoop))
            {
                _position++; // consume 'while'
                Expression condition;

                if (CurrentTokenIs(TokenType.LeftParenthesis))
                {
                    _position++; // consume '('
                    condition = ParseExpression(variableMap, scriptData);
                    _position++; // consume ')'
                }
                else
                {
                    throw new FormatException("Expected '(' after while loop. Missing condition!");
                }

                if (!CurrentTokenIs(TokenType.LeftScope))
                {
                    throw new FormatException("Expected '{' after while loop");
                }
                _position++; // consume '{'

                var contents = ParseFunctionArguments(true, variableMap, scriptData);

                if (!CurrentTokenIs(TokenType.RightScope))
                {
                    throw new FormatException("Missing closing scope for loop");
                }
                _position++; // consume '}'

                left = new WhileLoopExpression(condition, contents);
            }
            else if (CurrentTokenIs(TokenType.Definition) || CurrentTokenIs(TokenType.Assignment))
            {
                var definitionToken = _currentTokens[_position++];
                var assignment = ParseExpression(variableMap, scriptData);
                left = new AssignmentExpression(definitionToken, assignment);
            }
            else if (CurrentTokenIs(TokenType.Return))
            {
                _position++; // consume 'return'
                var ret = ParseExpression(variableMap, scriptData);
                left = new ReturnExpression(ret);
            }
            else if (CurrentTokenIs(TokenType.Namespace))
            {
                var token = _currentTokens[_position++];
                left = new NamespaceExpression(token);
            }
            else if (CurrentTokenIs(TokenType.Using))
            {
                var token = _currentTokens[_position++];
                left = new UsingExpression(token);
            }
            else if (CurrentTokenIs(TokenType.Import))
            {
                var token = _currentTokens[_position++];
                left = new ImportExpression(token);
            }

            // Handle access expressions (e.g., a.b)
            while (CurrentTokenIs(TokenType.Access) && left != null)
            {
                var accessToken = _currentTokens[_position++]; // consume '.'
                left = new AccessExpression(left, accessToken);
            }

            if (CurrentTokenIs(TokenType.Assignment) && left is AccessExpression accessExpression)
            {
                _position++;
                var assignment = ParseExpression(variableMap, scriptData);
                left = new AccessAssignmentExpression(accessExpression, assignment);
            }

            return left;
        }

        private string[] ParseDefinitionArguments()
        {
            var arguments = new List<string>();

            var checks = 0;

            // Parse comma-separated list of arguments
            while (_position < _currentTokens.Length && !CurrentTokenIs(TokenType.RightParenthesis) && checks < 30)
            {
                checks++;
                var argument = _currentTokens[_position++];
                arguments.Add(argument.TokenBuffer);

                if (CurrentTokenIs(TokenType.Comma))
                {
                    _position++; // consume ','
                }
            }

            return arguments.ToArray();
        }

        private Expression[] ParseFunctionArguments(bool scoped, AscentVariableMap variableMap, AscentScriptData scriptData)
        {
            var arguments = new List<Expression>();

            var checks = 0;

            // Parse comma-separated list of arguments
            while (!CurrentTokenIs(scoped ? TokenType.RightScope : TokenType.RightParenthesis) && checks < 30)
            {
                LoadNextContainer();
                if (CurrentTokenIs(scoped ? TokenType.RightScope : TokenType.RightParenthesis)) break;
                checks++;
                var argument = ParseExpression(variableMap, scriptData);
                arguments.Add(argument);

                if (CurrentTokenIs(TokenType.Comma))
                {
                    _position++; // consume ','
                }
            }

            return arguments.ToArray();
        }

        private bool CurrentTokenIs(TokenType type)
        {
            return _position < _currentTokens.Length && _currentTokens[_position].Type == type;
        }
    }
}