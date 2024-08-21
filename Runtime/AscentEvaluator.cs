using AscentLanguage.Parser;
using AscentLanguage.Splitter;
using AscentLanguage.Tokenizer;
using AscentLanguage.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AscentLanguage
{
    public static class AscentEvaluator
    {
        private class CacheData
        {
            internal Expression[] expressions;
            internal Dictionary<string, FunctionDefinition> functions;

            public CacheData(Expression[] expressions, Dictionary<string, FunctionDefinition> functions)
            {
                this.expressions = expressions;
                this.functions = functions;
            }
        }
        private static Dictionary<string, CacheData> cachedExpressions = new Dictionary<string, CacheData>();
        public static void ClearCache(string expression)
        {
            cachedExpressions.Remove(expression);
        }
        public static Var Evaluate(string expression, out AscentScriptData ascentScriptData, AscentVariableMap? variableMap = null, bool cache = true, bool debug = false)
        {
            if (variableMap == null)
            {
                variableMap = new AscentVariableMap(new Dictionary<string, Var>());
            }

            ascentScriptData = new AscentScriptData();

            List<Expression> toEvaluate = new List<Expression>();
            if (cachedExpressions.ContainsKey(expression) && cache)
            {
                toEvaluate = cachedExpressions[expression].expressions.ToList();
				ascentScriptData.Functions = cachedExpressions[expression].functions;
            }
            else
            {
                var tokens = AscentTokenizer.Tokenize(expression);

                if (debug)
                {
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        Debug.Log($"Token {i}: {tokens[i].type} - {new string(tokens[i].tokenBuffer)}");
                    }
                }

                var containers = AscentSplitter.SplitTokens(tokens.ToList());
                if (debug)
                {
                    Utility.PrintTokenContainer(containers);
                    Debug.Log("\n");
                }

                var parser = new AscentParser(containers as MultipleTokenContainer);

                var parsedExpressions = parser.Parse(variableMap, ascentScriptData);

                if (debug)
                {
                    Debug.Log($"Parsed {parsedExpressions.Count} expressions");
                }

                for (int i = 0; i < parsedExpressions.Count; i++)
                {
                    if (debug)
                    {
                        Utility.PrintExpression(parsedExpressions[i]);
                    }

                    toEvaluate.Add(parsedExpressions[i]);
                }
                if (cache)
                    cachedExpressions[expression] = new CacheData(toEvaluate.ToArray(), ascentScriptData.Functions);
            }
            Var result = 0f;
            foreach (var evaluate in toEvaluate)
            {
                var eval = evaluate.Evaluate(variableMap, ascentScriptData);
                if (eval != null) result = eval;
            }
            return result;
        }
    }
}
