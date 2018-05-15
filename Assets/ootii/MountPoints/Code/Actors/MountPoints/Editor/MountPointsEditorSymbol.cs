using System.Linq;
using UnityEditor;

[InitializeOnLoad]
public class MountPointsEditorSymbol : Editor
{
    /// <summary>
    /// Symbol that will be added to the editor
    /// </summary>
    private static string _EditorSymbol = "OOTII_MP";

    /// <summary>
    /// Add a new symbol as soon as Unity gets done compiling.
    /// </summary>
    static MountPointsEditorSymbol()
    {
        string lSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (!lSymbols.Split(';').Contains(_EditorSymbol))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, lSymbols + ";" + _EditorSymbol);
        }
    }
}
