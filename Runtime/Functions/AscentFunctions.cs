#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using AscentLanguage.Var;

namespace AscentLanguage.Functions
{
	public static class AscentFunctions
	{
		private static readonly Dictionary<string, Function> functions = new ()
		{
			{ "sin", new SinFunction() },
			{ "cos", new CosFunction() },
			{ "tan", new TanFunction() },
			{ "clamp", new ClampFunction() },
			{ "sign", new SignFunction() },
			{ "sqrt", new SqrtFunction() },
			{ "int", new IntFunction() },
			{ "abs", new AbsFunction() },
			{ "pow", new PowFunction() },
			{ "exp", new ExpFunction() },
			{ "frac", new FracFunction() },
			{ "lerp", new LerpFunction() },
			{ "bez_curve_x", new BezierCurveXFunction() },
			{ "bez_curve_y", new BezierCurveYFunction() },
			{ "debug", new DebugFunction() },
		};

		public static Function? GetFunction(string name)
		{
			return functions.TryGetValue(name, out var function) ? function : null;
		}
		public abstract class Function
		{
			public abstract Variable Evaluate(Variable[] input);
		}

		private class SinFunction : Function
		{
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 1) return 0f;
				return (float)Math.Sin(input[0].GetValue<float>());
			}
		}

		private class CosFunction : Function
		{
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 1) return 0f;
				return (float)Math.Cos(input[0].GetValue<float>());
			}
		}

		private class TanFunction : Function
		{
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 1) return 0f;
				return (float)Math.Tan(input[0].GetValue<float>());
			}
		}

		private class ClampFunction : Function
		{
			private float Clamp(float value, float min, float max)
			{
				return Math.Max(min, Math.Min(max, value));
			}
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 3) return 0f;
				return Clamp(input[0].GetValue<float>(), input[1].GetValue<float>(), input[2].GetValue<float>());
			}
		}

		private class SignFunction : Function
		{
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 1) return 0f;
				return Math.Sign(input[0].GetValue<float>());
			}
		}

		private class SqrtFunction : Function
		{
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 1) return 0f;
				return (float)Math.Sqrt(input[0].GetValue<float>());
			}
		}

		private class IntFunction : Function
		{
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 1) return 0f;
				return (int)input[0].GetValue<float>();
			}
		}

		private class AbsFunction : Function
		{
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 1) return 0f;
				return Math.Abs(input[0].GetValue<float>());
			}
		}

		private class LerpFunction : Function
		{
			private static float Lerp(float a, float b, float t)
			{
				return a + t * (b - a);
			}
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 3) return 0f;
				return Lerp(input[0].GetValue<float>(), input[1].GetValue<float>(), input[2].GetValue<float>());
			}
		}

		private class PowFunction : Function
		{
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 2) return 0f;
				return (float)Math.Pow(input[0].GetValue<float>(), input[1].GetValue<float>());
			}
		}

		private class ExpFunction : Function
		{
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 1) return 0f;
				return (float)Math.Exp(input[0].GetValue<float>());
			}
		}

		private class FracFunction : Function
		{
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 1) return 0f;
				return input[0].GetValue<float>() - (float)Math.Floor(input[0].GetValue<float>());
			}
		}

		private class BezierCurveXFunction : Function
		{
			private static float BezierCurveX(float x0, float x1, float x2, float x3, float t)
			{
				double x = Math.Pow(1 - t, 3) * x0 +
						   3 * Math.Pow(1 - t, 2) * t * x1 +
						   3 * (1 - t) * Math.Pow(t, 2) * x2 +
						   Math.Pow(t, 3) * x3;
				return (float)x;
			}
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 5) return 0f;
				return BezierCurveX(input[0].GetValue<float>(), input[1].GetValue<float>(), input[2].GetValue<float>(), input[3].GetValue<float>(), input[4].GetValue<float>());
			}
		}

		private class BezierCurveYFunction : Function
		{
			private static float BezierCurveY(float y0, float y1, float y2, float y3, float t)
			{
				double y = Math.Pow(1 - t, 3) * y0 +
						   3 * Math.Pow(1 - t, 2) * t * y1 +
						   3 * (1 - t) * Math.Pow(t, 2) * y2 +
						   Math.Pow(t, 3) * y3;
				return (float)y;
			}
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 5) return 0f;
				return BezierCurveY(input[0].GetValue<float>(), input[1].GetValue<float>(), input[2].GetValue<float>(), input[3].GetValue<float>(), input[4].GetValue<float>());
			}
		}

		private class DebugFunction : Function
		{
			public override Variable Evaluate(Variable[] input)
			{
				if (input.Length < 1) return 0f;
				AscentLog.WriteLine("DEBUG " + input.Select(x => x.ToString()).Aggregate((x1, x2) => x1 + ", " + x2));
				return 0f;

			}
		}
	}
}
