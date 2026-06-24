using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

// Editor Window — generate ItemData (ScriptableObject) dari sprite sheet Aseprite
public class ItemDataGenerator : EditorWindow
{
    private Object sourceFile;
    private DefaultAsset targetFolder;

    [MenuItem("Tools/Item Data Generator")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataGenerator>("Item Data Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Buat ItemData dari Aseprite", EditorStyles.boldLabel);
        GUILayout.Space(10);

        sourceFile = EditorGUILayout.ObjectField("Source Aseprite/Texture", sourceFile, typeof(Object), false);
        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField("Target Folder", targetFolder, typeof(DefaultAsset), false);

        GUILayout.Space(10);

        if (GUILayout.Button("Generate ItemData Assets", GUILayout.Height(30)))
        {
            Generate();
        }
    }

    private void Generate()
    {
        if (sourceFile == null || targetFolder == null)
        {
            EditorUtility.DisplayDialog("Error", "Pilih source file dan target folder dulu!", "OK");
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(sourceFile);
        string targetPath = AssetDatabase.GetAssetPath(targetFolder);

        if (!Directory.Exists(targetPath))
        {
            EditorUtility.DisplayDialog("Error", "Target folder tidak valid!", "OK");
            return;
        }

        // load semua sprite dari file source (harus Sprite Mode = Multiple)
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sourcePath)
            .Where(obj => obj is Sprite)
            .Cast<Sprite>()
            .ToArray();

        if (sprites.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Tidak ada sprite ditemukan di file tersebut!\nPastikan Sprite Mode = Multiple dan sudah di-slice.", "OK");
            return;
        }

        int created = 0;
        foreach (Sprite sprite in sprites)
        {
            string assetName = sprite.name;
            string assetPath = Path.Combine(targetPath, assetName + ".asset");

            if (File.Exists(assetPath))
            {
                if (!EditorUtility.DisplayDialog("File sudah ada", $"'{assetName}.asset' sudah ada.\nTimpa?", "Ya", "Lewati"))
                    continue;
            }

            ItemData item = ScriptableObject.CreateInstance<ItemData>();
            item.itemName = assetName;
            item.icon = sprite;
            item.isStackable = true;

            AssetDatabase.CreateAsset(item, assetPath);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Selesai!", $"Berhasil membuat {created} ItemData assets di:\n{targetPath}", "OK");
    }
}
