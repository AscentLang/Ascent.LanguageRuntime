namespace AscentLanguage.Tokenizer
{
    public enum TokenType // All types of tokens.
    {
        Query,
        Constant,
        Increment,
        Decrement,
        AdditionAssignment,
        SubtractionAssignment,
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Modulus,
        LeftParenthesis,
        RightParenthesis,
        LeftBracket,
        RightBracket,
        Pow,
        LesserThen,
        GreaterThan,
        TernaryConditional,
        Colon,
        Comma,
        Definition,
        Assignment,
        Variable,
        SemiColon,
        Function,
        FunctionDefinition,
        LeftScope,
        RightScope,
        FunctionArgument,
        ForLoop,
        WhileLoop,
        Return,
        True,
        False,
        String,
        Namespace,
        Using,
        Import,
        Access,
    }
    
    public struct Token
    {
        public readonly TokenType Type;
        public readonly string TokenBuffer; // Useful Buffer for token. For operators, it's a single char for the operation. For variables, it's the variable name. For numbers, it's the number. For Function Defs it's the function name. Etc.

        public Token(TokenType type, string tokenBuffer)
        {
            Type = type;
            TokenBuffer = tokenBuffer;
        }

        public Token(TokenType type, char token)
        {
            Type = type;
            TokenBuffer = "" + token;
        }
    }
}
