namespace KTrie.TrieNodes
{
    internal class TerminalCharTrieNode : CharTrieNode
    {

        internal TerminalCharTrieNode(char key) : base(key)
        {

        }

        public override bool IsTerminal => true;

        public string Word { get; set; } = null!;

        public override string ToString() => $"Key: {Key}, Word: {Word}";
    }
}