using AscentLanguage;
using AscentLanguage.Parser;
using AscentLanguage.Splitter;
using AscentLanguage.Tokenizer;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AscentDebuggerEditorWindow : EditorWindow
{
    private ASCAsset debuggingAsset = null;

    private string cachedString = "";
    private string cachedProcessedString = "";
    private Token[] cachedTokens = new Token[0];
    private TokenContainer cachedTokenContainer = null;
    private bool TokenizerFailed = false;
    private Expression[] cachedExpressions = null;
    private bool ParserFailed = false;
    private Object lastSelected = null;
    private bool showSemiColonTokens = true;

    private bool showContent;
    private bool showTokens;
    private bool showTokenContainers;
    private bool showExpresssions;
    private Selection selection;
    private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Ascent/Tools/Script Debugger", false, 1)]
    public static void ShowExample()
    {
        AscentDebuggerEditorWindow wnd = GetWindow<AscentDebuggerEditorWindow>();
        wnd.titleContent = new GUIContent("Ascent Script Debugger");
    }

    enum Selection
    {
        PickAsset,
        FromSelected
    }

    public void Update()
    {
        if (UnityEditor.Selection.activeObject != lastSelected)
        {
            Repaint();
        }
    }

    public void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.scrollView);
        //style.margin = new RectOffset(10, 0, 0, 0);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, style);

        style = new GUIStyle(GUI.skin.button);
        style.fontSize = 40;
        style.richText = true;
        style.fixedHeight = 50;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("<color=#E84855>Ascent</color> <color=#272635>Script  <size=15><i>Debugger</i></size></color>", style);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(40);

        selection = (Selection)EditorGUILayout.EnumPopup(selection);

        EditorGUILayout.Space(8);

        var headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 15;

        if (selection == Selection.PickAsset)
        {
            GUILayout.Label("Insert .asc Script Here!", headerStyle);

            debuggingAsset = EditorGUILayout.ObjectField(debuggingAsset, typeof(ASCAsset), false) as ASCAsset;

            GUILayout.Space(25);
        }
        else
        {
            //Selected
            debuggingAsset = UnityEditor.Selection.activeObject as ASCAsset;
            lastSelected = UnityEditor.Selection.activeObject;
        }

        if (debuggingAsset == null)
        {
            GUILayout.Space(25);

            debuggingAsset = null;
            EditorGUILayout.LabelField("No ASCAsset selected!", headerStyle);
            cachedString = "";
            cachedProcessedString = "";
            cachedTokens = null;
            EditorGUILayout.EndScrollView();
            return;
        }

        GUILayout.Space(8);

        string fileName = Path.GetFileName(AssetDatabase.GetAssetPath(debuggingAsset));
        GUILayout.Label($"Selected {fileName}", headerStyle);

        GUILayout.Space(25);

        var content = debuggingAsset.text;
        if (cachedString != content || cachedTokens == null || cachedTokenContainer == null || cachedExpressions == null)
        {
            cachedString = content;
            cachedProcessedString = AscentProcessor.Process(cachedString);
            TokenizerFailed = false;
            ParserFailed = false;
            try
            {
                cachedTokens = AscentTokenizer.Tokenize(cachedString);
                cachedTokenContainer = AscentSplitter.SplitTokens(cachedTokens.ToList());
            }
            catch (System.Exception e)
            {
                TokenizerFailed = true;
                throw e;
            }
            if (!TokenizerFailed)
            {
                try
                {
                    cachedExpressions = new AscentParser(cachedTokenContainer as MultipleTokenContainer).Parse(new AscentVariableMap()).ToArray();
                }
                catch (System.Exception e)
                {
                    ParserFailed = true;
                    throw e;
                }
            }
        }

        style = new GUIStyle(EditorStyles.foldout);
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 15;

        showContent = EditorGUILayout.Foldout(showContent, "Script Content", style);

        if (showContent)
        {
            style = new GUIStyle(GUI.skin.textField);
            style.fontSize = 15;
            style.padding = new RectOffset(10, 10, 10, 10);
            style.normal.textColor = Color.white;
            style.hover.textColor = Color.white;
            style.fontStyle = FontStyle.Normal;
            style.alignment = TextAnchor.UpperLeft;
            style.richText = true;

            GUILayout.Label(cachedProcessedString, style);
        }

        EditorGUILayout.Space(20);

        style = new GUIStyle(EditorStyles.foldout);
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 15;

        showSemiColonTokens = EditorGUILayout.Toggle("Show SemiColon Tokens", showSemiColonTokens);

        showTokens = EditorGUILayout.Foldout(showTokens, "Tokens", style);

        EditorGUILayout.Space(8);

        if (TokenizerFailed)
        {
            EditorGUILayout.LabelField("Tokenizer failed to tokenize the script!", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 15 });
        }

        if (showTokens && !TokenizerFailed)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 12;
            style.richText = true;
            style.fixedWidth = Screen.width / 3f;
            for (int i = 0; i < cachedTokens.Length; i++)
            {
                if (!showSemiColonTokens && cachedTokens[i].type == TokenType.SemiColon)
                {
                    continue;
                }
                GUILayout.BeginHorizontal(GUILayout.Width(Screen.width / 2f));
                EditorGUILayout.LabelField($"Token <color=#AAAAFF>{i}</color>:", style);
                EditorGUILayout.LabelField($"<color=#DD99DD>{cachedTokens[i].type}</color>", style);
                EditorGUILayout.LabelField($"<color=#BBEE88>{new string(cachedTokens[i].tokenBuffer)}</color>", style);
                //EditorGUILayout.LabelField($"Token <color=#AAAAFF>{i}</color>:     <color=#DD99DD>{cachedTokens[i].type}</color> - <color=#BBEE88>{new string(cachedTokens[i].tokenBuffer)}</color>", style);
                GUILayout.EndHorizontal();
                GuiLine();
            }
        }

        EditorGUILayout.Space(20);

        style = new GUIStyle(EditorStyles.foldout);
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 15;

        showTokenContainers = EditorGUILayout.Foldout(showTokenContainers, "Token Containers", style);

        EditorGUILayout.Space(8);

        if (TokenizerFailed)
        {
            EditorGUILayout.LabelField("Tokenizer failed to tokenize the script!", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 15 });
        }

        if (showTokenContainers && !TokenizerFailed)
        {
            DrawTokenContainer(0, cachedTokenContainer);
        }

        EditorGUILayout.Space(20);

        style = new GUIStyle(EditorStyles.foldout);
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 15;

        showExpresssions = EditorGUILayout.Foldout(showExpresssions, "Expressions", style);

        EditorGUILayout.Space(8);

        if (ParserFailed)
        {
            EditorGUILayout.LabelField("Parser failed to parse the script!", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 15 });
        }

        if (showExpresssions && !TokenizerFailed && !ParserFailed)
        {
            for (int i = 0; i < cachedExpressions.Length; i++)
            {
                DrawExpression(0, cachedExpressions[i]);
            }
        }

        EditorGUILayout.Space(40);

        EditorGUILayout.EndScrollView();
    }

    void DrawTokenContainer(int x, TokenContainer tokenContainer)
    {
        var style = new GUIStyle(GUI.skin.label);
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 15;
        style.padding = new RectOffset(x, 0, 0, 0);
        GUILayout.Label($"Token Container: {tokenContainer.GetType().Name}" + (x == 0 ? " (Root)" : ""), style);
        if (tokenContainer is SingleTokenContainer single)
        {
            style.fontSize = 12;
            style.fontStyle = FontStyle.Normal;
            style.richText = true;
            style.padding = new RectOffset(x + 15, 0, 0, 0);
            for (int i = 0; i < single.Expression.Length; i++)
            {
                GUILayout.BeginHorizontal(style, GUILayout.Width(Screen.width / 4f));
                EditorGUILayout.LabelField($"<color=#DD99DD> - {single.Expression[i].type}</color>", style);
                EditorGUILayout.LabelField($"<color=#BBEE88>{new string(single.Expression[i].tokenBuffer)}</color>", style);
                GUILayout.EndHorizontal();
            }
        }
        if (tokenContainer is MultipleTokenContainer multiple)
        {
            for (int i = 0; i < multiple.tokenContainers.Count; i++)
            {
                DrawTokenContainer(x + 15, multiple.tokenContainers[i]);
            }
        }
    }

    void DrawExpression(int x, Expression expression)
    {
        var style = new GUIStyle(GUI.skin.label);
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 15;
        style.padding = new RectOffset(x, 0, 0, 0);
        if (expression is ConstantExpression constExpr)
        {
            GUILayout.Label($"Constant Expression: {constExpr.Token.tokenBuffer} {(expression.Static ? "(Static)" : "")}", style);
        }
        else if (expression is BinaryExpression binaryExpr)
        {
            GUILayout.Label($"Binary Expression: {binaryExpr.Operator.tokenBuffer} {(expression.Static ? "(Static)" : "")}", style);
            DrawExpression(x + 15, binaryExpr.Left);
            DrawExpression(x + 15, binaryExpr.Right);
        }
        else if (expression is TernaryExpression ternaryExpr)
        {
            GUILayout.Label($"Ternary Expression: {(expression.Static ? "(Static)" : "")}", style);
            DrawExpression(x + 15, ternaryExpr.Condition);
            GUILayout.Label($"  True Expression:", style);
            DrawExpression(x + 15, ternaryExpr.TrueExpression);
            GUILayout.Label($"  False Expression:", style);
            DrawExpression(x + 15, ternaryExpr.FalseExpression);
        }
        else if (expression is FunctionExpression functionExpr)
        {
            GUILayout.Label($"Function Expression: {functionExpr.FunctionToken.tokenBuffer} {(expression.Static ? "(Static)" : "")}", style);
            for (int i = 0; i < functionExpr.Arguments.Length; i++)
            {
                DrawExpression(x + 15, functionExpr.Arguments[i]);
            }
        }
        else if (expression is FunctionDefinitionExpression functionDefExpr)
        {
            GUILayout.Label($"Function Definition: {functionDefExpr.FunctionToken.tokenBuffer} {(expression.Static ? "(Static)" : "")}", style);
            for (int i = 0; i < functionDefExpr.Contents.Length; i++)
            {
                DrawExpression(x + 15, functionDefExpr.Contents[i]);
            }
        }
        else if (expression is AssignmentExpression assignmentExpr)
        {
            GUILayout.Label($"Assignment Expression: {assignmentExpr.VariableToken.tokenBuffer} {(expression.Static ? "(Static)" : "")}", style);
            DrawExpression(x + 15, assignmentExpr.Assignment);
        }
        else if (expression is VariableExpression variableExpr)
        {
            GUILayout.Label($"Variable Expression: {variableExpr.VariableToken.tokenBuffer} {(expression.Static ? "(Static)" : "")}", style);
        }
        else if (expression is IncrementVariableExpression variableIncrementExpr)
        {
            GUILayout.Label($"Increment Variable Expression: {variableIncrementExpr.VariableToken.tokenBuffer} {(expression.Static ? "(Static)" : "")}", style);
        }
        else if (expression is DecrementVariableExpression variableDecrementExpr)
        {
            GUILayout.Label($"Decrement Variable Expression: {variableDecrementExpr.VariableToken.tokenBuffer} {(expression.Static ? "(Static)" : "")}", style);
        }
        else if (expression is ReturnExpression returnExpr)
        {
            GUILayout.Label($"Return Expression: {(expression.Static ? "(Static)" : "")}", style);
            DrawExpression(x + 15, returnExpr.Expression);
        }
        else if (expression is ForLoopExpression forExpr)
        {
            GUILayout.Label($"For Loop Expression: {(expression.Static ? "(Static)" : "")}", style);
            DrawExpression(x + 15, forExpr.Defintion);
            DrawExpression(x + 15, forExpr.Condition);
            DrawExpression(x + 15, forExpr.Suffix);
            for (int i = 0; i < forExpr.Contents.Length; i++)
            {
                DrawExpression(x + 15, forExpr.Contents[i]);
            }
        }
        else if (expression is WhileLoopExpression whileExpr)
        {
            GUILayout.Label($"While Loop Expression: {(expression.Static ? "(Static)" : "")}", style);
            DrawExpression(x + 15, whileExpr.Condition);
            for (int i = 0; i < whileExpr.Contents.Length; i++)
            {
                DrawExpression(x + 15, whileExpr.Contents[i]);
            }
        }
        else if (expression is AccessExpression accessExpr)
        {
            GUILayout.Label($"Access Expression: {(expression.Static ? "(Static)" : "")}", style);
            GUILayout.Label($"  Accessing variable: {accessExpr.Right.tokenBuffer}", style);
            DrawExpression(x + 15, accessExpr.Left);

        }
        else if (expression is NilExpression nilExpr)
        {
        }
    }

    void GuiLine(int i_height = 1)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);
        rect.height = i_height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }
}
