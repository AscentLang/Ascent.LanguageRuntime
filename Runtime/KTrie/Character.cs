namespace KTrie
{
    public readonly struct Character
    {
        internal readonly char C;

        public Character(char c)
        {
            this.C = c;
        }
        
        public static Character Any { get; } = new();

        public static implicit operator Character(char c) => new(c);

        public static bool operator ==(Character a, Character b) => a.C == b.C;

        public static bool operator !=(Character a, Character b) => a.C != b.C;
    }
}