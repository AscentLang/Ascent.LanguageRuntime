using System.Text;

public static class AscentLog
{
	private static readonly StringBuilder buffer = new StringBuilder();

	private static void Flush()
	{
		#if UNITY_5_3_OR_NEWER 
		UnityEngine.Debug.Log(buffer.ToString());
		#else
		System.Console.WriteLine(buffer.ToString());
		#endif
		buffer.Length = 0;
	}

	public static void Write(string value)
	{
		buffer.Append(value);
		
		if (value == null) return;
		
		var len = value.Length;
		if (len <= 0) return;
		
		var lastChar = value[len - 1];
		if (lastChar == '\n')
		{
			Flush();
		}
	}

	public static void Write(char value)
	{
		buffer.Append(value);
		if (value == '\n')
		{
			Flush();
		}
	}

	public static void Write(char[] value, int index, int count)
	{
		Write(new string(value, index, count));
	}

	public static void WriteLine(string value)
	{
		Write(value);
		Flush();
	}
}