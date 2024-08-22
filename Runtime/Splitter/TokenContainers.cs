#nullable enable
using AscentLanguage.Tokenizer;
using System.Collections.Generic;

namespace AscentLanguage.Splitter
{
    public abstract class TokenContainer
    {
        protected TokenContainer(TokenContainer? parentContainer)
        {
            ParentContainer = parentContainer;
        }
        public TokenContainer? ParentContainer { get; protected set; }
    }
    public class SingleTokenContainer : TokenContainer
    {
        public SingleTokenContainer(TokenContainer parentContainer, Token[] expression) : base(parentContainer)
        {
            Expression = expression;
        }
        public Token[] Expression { get; }
    }

    public class MultipleTokenContainer : TokenContainer
    {
        public MultipleTokenContainer(TokenContainer? parentContainer) : base(parentContainer)
        {
            ParentContainer = parentContainer;
        }
        public List<TokenContainer> TokenContainers { get; set; } = new ();
    }
}
