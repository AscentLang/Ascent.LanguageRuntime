using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine.UI;
using AscentLanguage.Tokenizer;
using System.Linq;

[ScriptedImporter(1, "asc")]
public class ASCImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        ASCAsset subAsset = ScriptableObject.CreateInstance<ASCAsset>();

        bool grabbed = false;

        while (!grabbed)
        {
            try
            {
                subAsset.text = File.ReadAllText(ctx.assetPath);
                grabbed = true;
            }
            catch
            (IOException)
            {
                grabbed = false;
                continue;
            }
        }

        var tokens = AscentTokenizer.Tokenize(subAsset.text);

        subAsset.imports = tokens.Where(t => t.type == TokenType.Import).Select(t => t.tokenBuffer).ToArray();

        subAsset.predicates = tokens.Where(t => t.type == TokenType.Using).Select(t => t.tokenBuffer).ToArray();

        ctx.AddObjectToAsset("asc", subAsset);
        ctx.SetMainObject(subAsset);
        AssetDatabase.SaveAssetIfDirty(subAsset);
    }
}