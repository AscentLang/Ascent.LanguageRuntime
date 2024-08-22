using AscentLanguage.Tokenizer;
using System.Collections.Generic;

namespace AscentLanguage.Splitter
{
    public static class AscentSplitter
    {
        public static TokenContainer SplitTokens(List<Token> tokens)
        {
            var rootContainer = new MultipleTokenContainer(null);
            var position = 0;
            var buffer = new List<Token>();
            var currentScope = rootContainer;
            var split = true;
            while (position < tokens.Count)
            {
                var token = tokens[position];
                if (token.Type == TokenType.LeftScope)
                {
                    if (buffer.Count > 0)
                    {
                        buffer.Add(token);
                        position++;
                        token = tokens[position];
                        currentScope?.TokenContainers.Add(new SingleTokenContainer(currentScope, buffer.ToArray()));
                        buffer.Clear();
                    }
                }
                switch (token.Type)
                {
                    case TokenType.RightScope:
                    {
                        if (buffer.Count > 0)
                        {
                            currentScope?.TokenContainers.Add(new SingleTokenContainer(currentScope, buffer.ToArray()));
                            buffer.Clear();

                        }
                        currentScope?.TokenContainers.Add(new SingleTokenContainer(currentScope, new Token[] { token }));
                        currentScope = currentScope?.ParentContainer as MultipleTokenContainer;
                        break;
                    }
                    case TokenType.FunctionDefinition:
                    {
                        var newScope = new MultipleTokenContainer(currentScope);
                        currentScope?.TokenContainers.Add(newScope);
                        currentScope = newScope;
                        break;
                    }
                    case TokenType.ForLoop:
                    {
                        var newScope = new MultipleTokenContainer(currentScope);
                        currentScope?.TokenContainers.Add(newScope);
                        currentScope = newScope;
                        break;
                    }
                    case TokenType.WhileLoop:
                    {
                        var newScope = new MultipleTokenContainer(currentScope);
                        currentScope?.TokenContainers.Add(newScope);
                        currentScope = newScope;
                        break;
                    }
                    case TokenType.LeftParenthesis:
                        split = false;
                        break;
                    case TokenType.RightParenthesis:
                        split = true;
                        break;
                }

                if (token.Type != TokenType.SemiColon)
                {
                    if (token.Type != TokenType.RightScope)
                        buffer.Add(token);
                }
                else
                {
                    if (split)
                    {
                        currentScope?.TokenContainers.Add(new SingleTokenContainer(currentScope, buffer.ToArray()));
                        buffer.Clear();
                    }
                    else
                    {
                        buffer.Add(token);
                    }
                }
                position++;
            }

            if (buffer.Count <= 0) return rootContainer;
            currentScope?.TokenContainers.Add(new SingleTokenContainer(currentScope, buffer.ToArray()));
            buffer.Clear();
            return rootContainer;
        }
    }
}
