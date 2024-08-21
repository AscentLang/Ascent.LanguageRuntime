using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Registry;
using TextMateSharp.Themes;
using UnityEngine;
using FontStyle = TextMateSharp.Themes.FontStyle;

public static class AscentProcessor
{
    //Adds Line numbers to the text and syntax highlighting
    public static string Process(string text)
    {
        var processedText = HighlightSyntax(text);
        processedText = AddLineNumbers(processedText);
        return processedText;
    }

    private static string HighlightSyntax(string input)
    {
        input = input.Replace("function ", "<color=#569CD6>function</color> ");
        input = input.Replace("{", "<color=#C586C0>{</color>");
        input = input.Replace("}", "<color=#C586C0>}</color>");
        input = input.Replace("(", "<color=#C586C0>(</color>");
        input = input.Replace(")", "<color=#C586C0>)</color>");
        input = input.Replace("if", "<color=#C586C0>if</color>");
        input = input.Replace("return", "<color=#C586C0>return</color>");
        input = input.Replace("while", "<color=#C586C0>while</color>");
        input = input.Replace("for", "<color=#C586C0>for</color>");
        input = input.Replace("let", "<color=#569CD6>let</color>");
        return input;
    }

    // Return number string with leading spaces to make it right-aligned
    private static string RightAlignNumber(int number, int totalWidth)
    {
        string numberString = number.ToString();
        return numberString.PadLeft(totalWidth);
    }

    private static string AddLineNumbers(string input)
    {
        StringBuilder sb = new StringBuilder();
        StringReader reader = new StringReader(input);
        int i = 0;
        while (reader.ReadLine() is string line)
        {
            i++;
            sb.Append("<size=12><color=#A9B7C6>");
            sb.Append(RightAlignNumber(i, 4));
            sb.Append("</color></size>    ");
            sb.Append(line);
            if (reader.Peek() != -1)
            {
                sb.Append("\n");
            }
        }
        return sb.ToString();
    }
}
