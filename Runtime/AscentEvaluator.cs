#nullable enable
using AscentLanguage.Parser;
using AscentLanguage.Splitter;
using AscentLanguage.Tokenizer;
using AscentLanguage.Util;
using System.Collections.Generic;
using System.Linq;
using AscentLanguage.Data;
using AscentLanguage.Var;

namespace AscentLanguage
{
	public static class AscentEvaluator
	{
		private class CacheData
		{
			internal readonly Expression[] Expressions;
			internal readonly Dictionary<string, FunctionDefinition> Functions;

			public CacheData(Expression[] expressions, Dictionary<string, FunctionDefinition> functions)
			{
				this.Expressions = expressions;
				this.Functions = functions;
			}
		}
		private static readonly Dictionary<string, CacheData> cachedExpressions = new Dictionary<string, CacheData>();
		public static void ClearCache(string expression)
		{
			cachedExpressions.Remove(expression);
		}
		public static Variable Evaluate(string expression, out AscentScriptData ascentScriptData, AscentVariableMap? variableMap = null, bool cache = true, bool debug = false)
		{
			variableMap ??= new AscentVariableMap(new Dictionary<string, Variable>());

			ascentScriptData = new AscentScriptData();

			List<Expression> toEvaluate = new List<Expression>();
			if (cachedExpressions.ContainsKey(expression) && cache)
			{
				toEvaluate = cachedExpressions[expression].Expressions.ToList();
				ascentScriptData.Functions = cachedExpressions[expression].Functions;
			}
			else
			{
				var tokens = AscentTokenizer.Tokenize(expression);

				if (debug)
				{
					for (int i = 0; i < tokens.Length; i++)
					{
						AscentLog.WriteLine($"Token {i}: {tokens[i].Type} - {tokens[i].TokenBuffer}");
					}
				}

				var containers = AscentSplitter.SplitTokens(tokens.ToList());
				if (debug)
				{
					Utility.PrintTokenContainer(containers);
					AscentLog.Write("\n");
				}

				var parser = new AscentParser(containers as MultipleTokenContainer);

				var parsedExpressions = parser.Parse(variableMap, ascentScriptData);

				if (debug)
				{
					AscentLog.WriteLine($"Parsed {parsedExpressions.Count} expressions");
				}

				foreach (var t in parsedExpressions)
				{
					if (debug)
					{
						Utility.PrintExpression(t);
					}

					toEvaluate.Add(t);
				}
				if (cache)
					cachedExpressions[expression] = new CacheData(toEvaluate.ToArray(), ascentScriptData.Functions);
			}
			Variable result = 0f;
			foreach (var evaluate in toEvaluate)
			{
				var eval = evaluate.Evaluate(variableMap, ascentScriptData);
				if (eval != null) result = eval;
			}
			return result;
		}
	}
}
