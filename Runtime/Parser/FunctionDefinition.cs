using System.Collections.Generic;
using KTrie;

namespace AscentLanguage.Parser
{
    public class FunctionDefinition
    {
        public Trie Args { get; set; } = new();
        public Expression[] Contents { get; set; }
    }
}
