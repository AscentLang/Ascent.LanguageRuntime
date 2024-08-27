using AscentLanguage.Functions;
using AscentLanguage.Parser;
using AscentLanguage.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using KTrie;

namespace AscentLanguage.Tokenizer
{
    public abstract class Tokenizer
    {
        public abstract Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope);
        public abstract bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null);
    }

    public class SingleCharTokenizer : Tokenizer
    {
        public char Token { get; }
        private TokenType Type { get; }
        private bool HasOperand { get; }

        public SingleCharTokenizer(char token, TokenType type, bool hasOperand)
        {
            Token = token;
            Type = type;
            HasOperand = hasOperand;
        }

        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            if (stream.Position >= stream.Length && HasOperand) throw new FormatException("Missing Operand!");
            return new Token(Type, br.ReadChar());
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            return peekChar == Token;
        }
    }

    public class SubtractionTokenizer : Tokenizer
    {
        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            if (stream.Position >= stream.Length) throw new FormatException("Missing Operand!");
            return new Token(TokenType.Subtraction, '-');
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            if (existingTokens is not { Count: > 0 }) return false;
            var lastToken = existingTokens[^1];
            if (lastToken.Type is TokenType.Constant or TokenType.Variable or TokenType.Query)
            {
                return peekChar == '-';
            }
            return false;
        }
    }

    public class NumberTokenizer : Tokenizer
    {
        private static bool IsNumber(int c)
        {
            return c >= '0' && c <= '9' || c == '.' || c == '-';
        }
        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            var stringBuilder = new StringBuilder();
            while (IsNumber(br.PeekChar()))
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            return new Token(TokenType.Constant, stringBuilder.ToString());
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            return IsNumber(peekChar) && peekChar != '.';
        }
    }

    public class WordMatchTokenizer : Tokenizer
    {
        public string Word { get; set; }
        public TokenType Type { get; set; }

        public WordMatchTokenizer(string word, TokenType type)
        {
            Word = word;
            Type = type;
        }

        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            char[] buffer = br.ReadChars(Word.Length);
            return new Token(Type, new string(buffer));
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            if (peekChar == Word[0] && stream.Position < stream.Length)
            {
                var stringBuilder = new StringBuilder();

                for (int i = 0; i < Word.Length; i++)
                {
                    if (Word[i] == br.PeekChar())
                    {
                        stringBuilder.Append(br.ReadChar());
                        continue;
                    }
                    break;
                }

                if (stringBuilder.ToString() == Word)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class QueryTokenizer : Tokenizer
    {
        private static readonly Tokenizer qTokenizer = new SingleCharTokenizer('q', TokenType.Query, false);
        private static readonly Tokenizer queryTokenizer = new WordMatchTokenizer("query", TokenType.Query);

        private static bool ContinueFeedingQuery(int chara)
        {
            return (chara >= 'a' && chara <= 'z') || (chara >= 'A' && chara <= 'Z');
        }

        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            var stringBuilder = new StringBuilder();
            while (ContinueFeedingQuery(br.PeekChar()))
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            return new Token(TokenType.Query, stringBuilder.ToString());
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            var qMatch = qTokenizer.IsMatch(peekChar, br, stream, ref variableDefs, ref functionDefs, scope, existingTokens);
            var queryMatch = queryTokenizer.IsMatch(peekChar, br, stream, ref variableDefs, ref functionDefs, scope, existingTokens);
            return (queryMatch || qMatch) && br.ReadChar() == '.';
        }
    }

    public class DefinitionTokenizer : Tokenizer
    {
        private static readonly Tokenizer letTokenizer = new WordMatchTokenizer("let", TokenType.Definition);
        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            br.ReadChars(3);
            var stringBuilder = new StringBuilder();
            while (br.PeekChar() != '=')
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            variableDefs.Add(stringBuilder.ToString());
            return new Token(TokenType.Definition, stringBuilder.ToString());
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            //TODO: Should this be more robust?
            return letTokenizer.IsMatch(peekChar, br, stream, ref variableDefs, ref functionDefs, scope, existingTokens);
        }
    }

    public class AssignmentTokenizer : Tokenizer
    {
        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            var stringBuilder = new StringBuilder();
            while (br.PeekChar() != '=')
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            return new Token(TokenType.Assignment, stringBuilder.ToString());
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            if (!variableDefs.StartsWith((char)peekChar).Any()) return false;

            var stringBuilder = new StringBuilder();
            while (br.PeekChar() != '=')
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            
            var match = stringBuilder.ToString();
            return variableDefs.Contains(match);
        }
    }

    public class VariableTokenizer : Tokenizer
    {
        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            var hasArgs = functionDefs.TryGetValue(scope, out var def);
            
            var args = hasArgs ? def.Args : null;
            
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(br.ReadChar());
            
            while (variableDefs.PartialContains(stringBuilder.ToString()) || (hasArgs && args.PartialContains(stringBuilder.ToString())))
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            
            return new Token(TokenType.Variable, stringBuilder.ToString());
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            var hasArgs = functionDefs.TryGetValue(scope, out var def);
            
            var args = hasArgs ? def.Args : null;
            if (!(hasArgs && args.StartsWith((char)peekChar).Any()) && !variableDefs.StartsWith((char)peekChar).Any()) return false;
            
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(br.ReadChar());
            
            while (variableDefs.PartialContains(stringBuilder.ToString()) || (hasArgs && args.PartialContains(stringBuilder.ToString())))
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            
            var match = stringBuilder.ToString();
            return variableDefs.Contains(match) || (hasArgs && args.Contains(match));
        }
    }

    public class FunctionTokenizer : Tokenizer
    {
        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            var stringBuilder = new StringBuilder();
            while (AscentFunctions.Functions.PartialContains(stringBuilder.ToString()) || functionDefs.PartialContains(stringBuilder.ToString()))
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            return new Token(TokenType.Function, stringBuilder.ToString());
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            if (!AscentFunctions.Functions.StartsWith((char)peekChar).Any() && !functionDefs.StartsWith((char)peekChar).Any()) return false;
            var stringBuilder = new StringBuilder();
            while (AscentFunctions.Functions.PartialContains(stringBuilder.ToString()) || functionDefs.PartialContains(stringBuilder.ToString()))
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            var match = stringBuilder.ToString();
            return AscentFunctions.Functions.ContainsKey(match) || functionDefs.ContainsKey(match);
        }
    }

    public class FunctionDefinitionTokenizer : Tokenizer
    {
        private static readonly Tokenizer functionTokenizer = new WordMatchTokenizer("function", TokenType.FunctionDefinition);
        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            functionTokenizer.GetToken(peekChar, br, stream, ref variableDefs, ref functionDefs, scope);
            var stringBuilder = new StringBuilder();
            while (br.PeekChar() != '(' && br.PeekChar() != '{')
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }

            var match = stringBuilder.ToString();
            functionDefs.Add(match, new FunctionDefinition());
            return new Token(TokenType.FunctionDefinition, match);
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            return functionTokenizer.IsMatch(peekChar, br, stream, ref variableDefs, ref functionDefs, scope, existingTokens);
        }
    }

    public class FunctionArgumentTokenizer : Tokenizer
    {
        private string name = "";
        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            var stringBuilder = new StringBuilder();
            while (br.PeekChar() != ',' && br.PeekChar() != ')')
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }

            var match = stringBuilder.ToString();
            functionDefs[name].Args.Add(match);
            return new Token(TokenType.FunctionArgument, match);
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            TokenType[] allowedTokens = { TokenType.LeftParenthesis, TokenType.Comma, TokenType.FunctionArgument };
            int back = 0;
            while (existingTokens != null && existingTokens.Count > back && allowedTokens.Contains(existingTokens[existingTokens.Count - back - 1].Type))
            {
                back++;
            }
            if (existingTokens == null || existingTokens.Count < back + 1) return false;
            var def = existingTokens[existingTokens.Count - back - 1];
            if (existingTokens[existingTokens.Count - back - 1].Type == TokenType.FunctionDefinition)
            {
                name = def.TokenBuffer;
                return true;
            }
            return false;
        }
    }

    public class StringTokenizer : Tokenizer
    {
        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            StringBuilder stringBuilder = new StringBuilder();
            br.Read(); // Skip the opening quote
            while (br.PeekChar() != '"' && stream.Position < stream.Length)
            {
                stringBuilder.Append(br.ReadChar());
            }
            br.Read(); // Skip the closing quote
            return new Token(TokenType.String, stringBuilder.ToString());
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            return peekChar == '"';
        }
    }

    public class NamespaceTokenizer : Tokenizer
    {
        private static readonly Tokenizer namespaceTokenizer = new WordMatchTokenizer("namespace", TokenType.Namespace);
        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            br.ReadChars(9);
            var stringBuilder = new StringBuilder();
            while (br.PeekChar() != ';')
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            
            return new Token(TokenType.Namespace, stringBuilder.ToString());
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            if (namespaceTokenizer.IsMatch(peekChar, br, stream, ref variableDefs, ref functionDefs, scope, existingTokens))
            {
                //TODO: Should this be more robust?
                return true;
            }
            return false;
        }
    }

    public class UsingTokenizer : Tokenizer
    {
        private static readonly Tokenizer usingTokenizer = new WordMatchTokenizer("using", TokenType.Using);
        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            br.ReadChars(5);
            var stringBuilder = new StringBuilder();
            while (br.PeekChar() != ';')
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            //variableDefs.Add(stringBuilder.ToString());
            return new Token(TokenType.Using, stringBuilder.ToString());
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            if (usingTokenizer.IsMatch(peekChar, br, stream, ref variableDefs, ref functionDefs, scope, existingTokens))
            {
                //TODO: Should this be more robust?
                return true;
            }
            return false;
        }
    }

    public class ImportTokenizer : Tokenizer
    {
        private static readonly Tokenizer importTokenizer = new WordMatchTokenizer("import", TokenType.Import);
        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            br.ReadChars(6);
            var stringBuilder = new StringBuilder();
            while (br.PeekChar() != '-')
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            variableDefs.Add(stringBuilder.ToString());
            stringBuilder.Append("^");
            br.ReadChars(2);
            while (br.PeekChar() != ';')
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            return new Token(TokenType.Import, stringBuilder.ToString());
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            if (importTokenizer.IsMatch(peekChar, br, stream, ref variableDefs, ref functionDefs, scope, existingTokens))
            {
                //TODO: Should this be more robust?
                return true;
            }
            return false;
        }
    }

    public class AccessTokenizer : Tokenizer
    {
        private static bool ContinueFeedingTerm(int chara)
        {
            return (chara >= 'a' && chara <= 'z') || (chara >= 'A' && chara <= 'Z');
        }

        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            br.ReadChars(1);
            var stringBuilder = new StringBuilder();
            while (ContinueFeedingTerm(br.PeekChar()))
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            return new Token(TokenType.Access, stringBuilder.ToString());
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            //TODO: Should this be more robust?
            return peekChar == '.';
        }
    }

    // Optimized tokenizer to use a Trie for fast tokenization.
    // TODO: Should implement this into variable and function matching.
    public class KeywordTokenizer : Tokenizer
    {
        private readonly Dictionary<string, TokenType> _keywords;
        private readonly Trie trie = new();
        public KeywordTokenizer(Dictionary<string, TokenType> keywords)
        {
            _keywords = keywords;
            
            //not ideal
            keywords.Keys.ToList().ForEach(k => trie.Add(k));
        }

        public override Token GetToken(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope)
        {
            var stringBuilder = new StringBuilder();
            while (trie.PartialContains(stringBuilder.ToString()))
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            var keyword = stringBuilder.ToString();
            return new Token(_keywords[keyword], keyword);
        }

        public override bool IsMatch(int peekChar, BinaryReader br, MemoryStream stream, ref Trie variableDefs, ref TrieDictionary<FunctionDefinition> functionDefs, string scope, List<Token>? existingTokens = null)
        {
            var peekMatches = trie.StartsWith((char)peekChar).Any();
            if (!peekMatches) return false;
            var stringBuilder = new StringBuilder();
            while (trie.PartialContains(stringBuilder.ToString()))
            {
                stringBuilder.Append(br.ReadChar());
                if (stream.Position >= stream.Length) break;
            }
            return trie.Contains(stringBuilder.ToString());
        }
    }
}
