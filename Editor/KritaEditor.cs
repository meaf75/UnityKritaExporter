using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Inspector for .kra assets
/// </summary>
[CustomEditor(typeof(DefaultAsset))]
public class KritaEditor : Editor {
    
    private enum AvailableExportType {
        PNG,JPG,JPEG,JPE,WEBP,PSD,
        ORA,GBR,VBR,GIH,HEIC,HEIF,KPP,EXR,R16,R32,R2,SCML,QML,QMLTYPES,QUMLPROJECT,CSV,ICO,BMP,DIB,PBM,BGM,PPM,TGA,
        ICB,TPIC,VDA,VST,TIF,TIFF,XBM,XPM,
    }

    private static AvailableExportType exportType;
    
    // private const string KRITA_INSTALLATION_PATH_EXAMPLE = "C:\\Program Files\\Krita (x64)\bin\\krita.exe"; 
    private const string KRITA_INSTALLATION_PATH = "C:\\Program Files\\Krita (x64)\bin\\krita.exe"; 
    private bool isKraFile = false;

    private void OnEnable() {
        // Save the result on select the asset and avoid recalculate the result by a repaint
        isKraFile = AssetDatabase.GetAssetPath(target).EndsWith(".kra");
    }

    public override void OnInspectorGUI() {
        // .kra files are imported as a DefaultAsset.
        // Need to determine that this default asset is an .kra file
        if (isKraFile) {
            KraInspectorGUI();
        } else {
            base.OnInspectorGUI();
        }
    }

    private void KraInspectorGUI() {
        
        GUI.enabled = true; // I Use PlasticSCM, for some reason the inspector gui still locked after checkout the asset

        GUILayout.Space(10);
        exportType = (AvailableExportType) EditorGUILayout.EnumPopup("Export type: ",exportType);
        string exportTypeStg = exportType.ToString().ToLower();
        GUILayout.Space(10);
        
        if (GUILayout.Button($"Export as .{exportTypeStg} here")) {
            ExporKritaFile();
        }

        if (GUILayout.Button($"Export as .{exportTypeStg} at ...")) {
            var path = EditorUtility.SaveFilePanel(
                "Krita export target path",
                "",
                Path.GetFileNameWithoutExtension(GetKritaFilePath()),
                exportType.ToString().ToLower()
            );
            
            if(string.IsNullOrEmpty(path))
                return;
            
            ExporKritaFile(path);
        }
    }

    /// <summary> This does the magic, provide a path for a custom export type or export in a specific path </summary>
    /// <param name="kraFileTargetPath">keep empty to export the krita file in the same directory as png</param>
    private void ExporKritaFile(string kraFileTargetPath = "") {

        if (string.IsNullOrEmpty(KRITA_INSTALLATION_PATH)) {
            string errMsg = "You must set the krita installation path (KRITA_INSTALLATION_PATH variable) before export any .kra file";
            Debug.LogError(errMsg);
            EditorUtility.DisplayDialog("Missing Krita installation path",errMsg,"ok, sorry");
            return;
        }
        
        string assetPath = AssetDatabase.GetAssetPath(target);
        
        if(string.IsNullOrEmpty(kraFileTargetPath))
            kraFileTargetPath = Application.dataPath.Replace("/Assets", "/" + assetPath.Replace(".kra", $".{exportType.ToString().ToLower()}"));

        string command = $"\"{GetKritaFilePath()}\" --export --export-filename \"{kraFileTargetPath}\"";
            
        var process = new Process();
        process.StartInfo = new ProcessStartInfo {
            FileName = $"\"{KRITA_INSTALLATION_PATH}\"",
            Arguments = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
            
        process.Start();
        process.WaitForExit();
            
        AssetDatabase.Refresh();
        Debug.Log("Asset exported at: "+kraFileTargetPath);
        
    }
    
    private string GetKritaFilePath() {
        return Application.dataPath.Replace("/Assets", "/" + AssetDatabase.GetAssetPath(target));
    }
}
