using AscentLanguage;
using System;

public class AscentDomain
{
	public static IMatcherProvider matcherProvider = new UnityMatcherProvider();

	public static void Main(string[] args)
	{
		for (int i = 0; i < 100000; i++)
		{
			var x = AscentEvaluator.Evaluate("1 + 1", out _);
			//Console.WriteLine(x);
		}
	}
}
