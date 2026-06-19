using UnityEngine;
using UnityEditor;
using System.IO;

public class ItemDataCreator : EditorWindow
{
    private string itemName = "NewItem";
    private DefaultAsset targetFolder;
    private Sprite selectedSprite;

    [MenuItem("Tools/Item Data Creator")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataCreator>("Item Data Creator");
    }

    void OnGUI()
    {
        GUILayout.Label("Create ItemData Asset", EditorStyles.boldLabel);

        itemName = EditorGUILayout.TextField("Item Name", itemName);
        selectedSprite = (Sprite)EditorGUILayout.ObjectField("Icon Sprite", selectedSprite, typeof(Sprite), false);
        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField("Target Folder", targetFolder, typeof(DefaultAsset), false);

        if (GUILayout.Button("Create ItemData"))
        {
            CreateItemData();
        }

        if (GUILayout.Button("Create from Aseprite in Folder"))
        {
            CreateFromAsepriteFolder();
        }
    }

    void CreateItemData()
    {
        if (selectedSprite == null)
        {
            Debug.LogWarning("Pilih sprite dulu!");
            return;
        }

        string path = GetTargetPath();
        if (string.IsNullOrEmpty(path)) return;

        ItemData item = ScriptableObject.CreateInstance<ItemData>();
        item.itemName = itemName;
        item.icon = selectedSprite;

        string assetPath = Path.Combine(path, itemName + ".asset");
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(item, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"ItemData created: {assetPath}");
    }

    void CreateFromAsepriteFolder()
    {
        string path = GetTargetPath();
        if (string.IsNullOrEmpty(path)) return;

        string[] asepriteFiles = Directory.GetFiles(path, "*.aseprite", SearchOption.TopDirectoryOnly);

        foreach (string asepriteFile in asepriteFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(asepriteFile);
            string relativePath = asepriteFile.Replace(Application.dataPath, "Assets");

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);
            if (sprite == null)
            {
                Debug.LogWarning($"Sprite not found for {relativePath}. Make sure it's imported properly.");
                continue;
            }

            ItemData item = ScriptableObject.CreateInstance<ItemData>();
            item.itemName = fileName;
            item.icon = sprite;

            string assetPath = Path.Combine(path, fileName + ".asset");
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(item, assetPath);
            Debug.Log($"Created ItemData: {assetPath} with icon from {fileName}.aseprite");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    string GetTargetPath()
    {
        if (targetFolder == null)
        {
            Debug.LogWarning("Pilih target folder dulu!");
            return null;
        }

        string path = AssetDatabase.GetAssetPath(targetFolder);
        if (!AssetDatabase.IsValidFolder(path))
        {
            Debug.LogWarning("Target harus folder!");
            return null;
        }

        return path;
    }
}
