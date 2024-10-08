﻿using AscentLanguage.Parser;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KTrie;

namespace AscentLanguage.Tokenizer
{
	public static class AscentTokenizer
	{
		private static readonly List<Tokenizer> tokenizers = new ()
		{
			new SingleCharTokenizer(';', TokenType.SemiColon, false),
			new KeywordTokenizer(new Dictionary<string, TokenType>
			{
				{ "true", TokenType.True },
				{ "false", TokenType.False },
				{ "return", TokenType.Return },
				{ "for", TokenType.ForLoop },
				{ "while", TokenType.WhileLoop },
			}),
			new SingleCharTokenizer('{', TokenType.LeftScope, false),
			new SingleCharTokenizer('}', TokenType.RightScope, false),
			new SingleCharTokenizer('(', TokenType.LeftParenthesis, true),
			new SingleCharTokenizer(')', TokenType.RightParenthesis, false),
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
			new SingleCharTokenizer('[', TokenType.LeftBracket, true),
			new SingleCharTokenizer(']', TokenType.RightBracket, false),
			new SingleCharTokenizer('<', TokenType.LesserThen, true),
			new SingleCharTokenizer('>', TokenType.GreaterThan, true),
			new SingleCharTokenizer('?', TokenType.TernaryConditional, true),
			new SingleCharTokenizer(':', TokenType.Colon, true),
			new SingleCharTokenizer(',', TokenType.Comma, false),
			new NumberTokenizer(),
			new StringTokenizer(),
			new NamespaceTokenizer(),
			new UsingTokenizer(),
			new ImportTokenizer(),
			new QueryTokenizer(),
			new AccessTokenizer(),
			new FunctionDefinitionTokenizer(),
			new FunctionTokenizer(),
			new DefinitionTokenizer(),
			new AssignmentTokenizer(),
			new VariableTokenizer(),
			new FunctionArgumentTokenizer(),
			new SingleCharTokenizer('=', TokenType.Assignment, true) // Only shows for access assignments
		};

		public static Token[] Tokenize(string expression)
		{
			var variableDefinitions = new Trie();
			var functionDefinitions = new TrieDictionary<FunctionDefinition>(null);
			var tokens = new List<Token>();
			var scope = new Stack<string>();
			
			scope.Push("GLOBAL"); //Our base scope outside of functions and for loops.

			var trimmedExpression = new StringBuilder();
			var charArray = expression.ToCharArray();
			var quoteIdx = 0;
			foreach (var chr in charArray)
			{
				if (chr == '"')
					quoteIdx++;

				if (!char.IsWhiteSpace(chr) || quoteIdx % 2 == 1)
					trimmedExpression.Append(chr);
			}

			var strLength = trimmedExpression.Length;
			var stream = new MemoryStream(Encoding.UTF8.GetBytes(trimmedExpression.ToString()));
			var br = new BinaryReader(stream, Encoding.UTF8);
			while (stream.Position < strLength)
			{
				var peek = br.PeekChar(); //Store peek char for efficiency
				var succeeded = false;
				foreach (var tokenizer in tokenizers)
				{
					var position = stream.Position;
					if (tokenizer.IsMatch(peek, br, stream, ref variableDefinitions, ref functionDefinitions, scope.Peek(), tokens))
					{
						stream.Position = position;
						var token = tokenizer.GetToken(peek, br, stream, ref variableDefinitions, ref functionDefinitions, scope.Peek());
						tokens.Add(token);
						
						if (token.Type == TokenType.FunctionDefinition)
							scope.Push(token.TokenBuffer);
						else if (token.Type == TokenType.RightBracket) 
							scope.Pop();

						succeeded = true;
						break;
					}
					stream.Position = position;
				}

				if (!succeeded)
				{
					AscentLog.WriteLine("bleh " + br.ReadChar()); // Prevent stack overflow
				}
			}

			return tokens.ToArray();
		}
	}
}
