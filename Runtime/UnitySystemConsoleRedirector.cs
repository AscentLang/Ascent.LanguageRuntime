using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class UnitySystemConsole
{
    private static StringBuilder buffer = new StringBuilder();

    public static void Flush()
    {
        Debug.Log(buffer.ToString());
        buffer.Length = 0;
    }

    public static void Write(string value)
    {
        buffer.Append(value);
        if (value != null)
        {
            var len = value.Length;
            if (len > 0)
            {
                var lastChar = value[len - 1];
                if (lastChar == '\n')
                {
                    Flush();
                }
            }
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