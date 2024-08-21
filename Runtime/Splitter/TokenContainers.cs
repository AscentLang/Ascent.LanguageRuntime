using AscentLanguage.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AscentLanguage.Splitter
{
    public abstract class TokenContainer
    {
        public TokenContainer(TokenContainer? parentContainer)
        {
            this.parentContainer = parentContainer;
        }
        public TokenContainer? parentContainer { get; set; }
    }
    public class SingleTokenContainer : TokenContainer
    {
        public SingleTokenContainer(TokenContainer parentContainer, Token[] expression) : base(parentContainer)
        {
            this.Expression = expression;
        }
        public Token[] Expression { get; set; }
    }

    public class MultipleTokenContainer : TokenContainer
    {
        public MultipleTokenContainer(TokenContainer? parentContainer) : base(parentContainer)
        {
            this.parentContainer = parentContainer;
        }
        public List<TokenContainer> tokenContainers { get; set; } = new();
    }
}
