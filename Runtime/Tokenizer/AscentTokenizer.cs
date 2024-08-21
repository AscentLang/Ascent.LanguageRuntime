﻿using AscentLanguage.Parser;
using AscentLanguage.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine.Rendering;

namespace AscentLanguage.Tokenizer
{
    public static class AscentTokenizer
    {
        private static readonly List<Tokenizer> tokenizers = new List<Tokenizer>
        {
            new QueryTokenizer(),
            new WordMatchTokenizer("++", TokenType.Increment),
            new WordMatchTokenizer("--", TokenType.Decrement),
            new WordMatchTokenizer("+=", TokenType.AdditionAssignment),
            new WordMatchTokenizer("-=", TokenType.SubtractionAssignment),
            new SingleCharTokenizer('+', TokenType.Addition, true),
            new SubtractionTokenizer(),
            new SingleCharTokenizer('*', TokenType.Multiplication, true),
            new SingleCharTokenizer('/', TokenType.Division, true),
            new SingleCharTokenizer('^', TokenType.Pow, true),
            new SingleCharTokenizer('%', TokenType.Pow, true),
            new SingleCharTokenizer('(', TokenType.LeftParenthesis, true),
            new SingleCharTokenizer(')', TokenType.RightParenthesis, false),
            new SingleCharTokenizer('[', TokenType.LeftBracket, true),
            new SingleCharTokenizer(']', TokenType.RightBracket, false),
            new SingleCharTokenizer('<', TokenType.LesserThen, true),
            new SingleCharTokenizer('>', TokenType.GreaterThan, true),
            new SingleCharTokenizer('?', TokenType.TernaryConditional, true),
            new SingleCharTokenizer(':', TokenType.Colon, true),
            new SingleCharTokenizer(';', TokenType.SemiColon, false),
            new SingleCharTokenizer(',', TokenType.Comma, false),
            new SingleCharTokenizer('{', TokenType.LeftScope, false),
            new SingleCharTokenizer('}', TokenType.RightScope, false),
            new WordMatchTokenizer("true", TokenType.True),
            new WordMatchTokenizer("false", TokenType.False),
            new WordMatchTokenizer("return", TokenType.Return),
            new AccessTokenizer(),
            new ForLoopTokenizer(),
            new WhileLoopTokenizer(),
            new FunctionDefinitionTokenizer(),
            new FunctionArgumentTokenizer(),
            new FunctionTokenizer(),
            new DefinitionTokenizer(),
            new AssignmentTokenizer(),
            new VariableTokenizer(),
            new NumberTokenizer(),
            new StringTokenizer(),
            new NamespaceTokenizer(),
            new UsingTokenizer(),
            new ImportTokenizer(),
            new SingleCharTokenizer('=', TokenType.Assignment, true) // Only shows for access assignments
		};

        public static Token[] Tokenize(string expression)
        {
            List<string> variableDefinitions = new List<string>();
            List<FunctionDefinition> functionDefinitions = new List<FunctionDefinition>();
            List<Token> tokens = new List<Token>();
            Stack<string> scope = new();
            scope.Push("GLOBAL");

            string trimmedExpression = "";

            var cr = expression.ToCharArray();
            int quoteIdx = 0;
            for (int i = 0; i < cr.Length; i++)
            {
                if (cr[i] == '"')
                {
                    quoteIdx++;
                    trimmedExpression += "";
                }

                if (!char.IsWhiteSpace(cr[i]) || quoteIdx % 2 == 1)
                {
                    trimmedExpression += cr[i];
                }
            }

            int strLength = trimmedExpression.Length;
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(trimmedExpression));
            BinaryReader br = new BinaryReader(stream, Encoding.UTF8);
            while (stream.Position < strLength)
            {
                int peek = br.PeekChar(); //Store peek char for efficiency
                bool succeeded = false;
                foreach (var tokenizer in tokenizers)
                {
                    long position = stream.Position;
                    if (tokenizer.IsMatch(peek, br, stream, ref variableDefinitions, ref functionDefinitions, scope.Peek(), tokens))
                    {
                        stream.Position = position;
                        tokens.Add(tokenizer.GetToken(peek, br, stream, ref variableDefinitions, ref functionDefinitions, scope.Peek()));
                        if (tokenizer is FunctionDefinitionTokenizer)
                        {
                            var token = tokens.Last();
                            scope.Push(token.tokenBuffer);
                        }
                        if (tokenizer is ForLoopTokenizer)
                        {
                            var token = tokens.Last();
                            scope.Push(scope.Peek() + "_" + token.tokenBuffer);
                        }
                        if (tokenizer is WhileLoopTokenizer)
                        {
                            var token = tokens.Last();
                            scope.Push(scope.Peek() + "_" + token.tokenBuffer);
                        }
                        if (tokenizer is SingleCharTokenizer singleCharTokenizer && singleCharTokenizer.Token == '}')
                        {
                            scope.Pop();
                        }
                        succeeded = true;
                        break;
                    }
                    stream.Position = position;
                }
                if (!succeeded)
                    br.ReadChar(); // Prevent stack overflow
            }

            return tokens.ToArray();
        }
    }
}
