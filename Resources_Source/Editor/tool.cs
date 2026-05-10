using UnityEditor;
using System.IO;
using UnityEngine;

public class BuildShaders
{
	[MenuItem("Assets/Pack Assets")]
	static void BuildMultiPlatformAssetBundle()
	{
		var selected = Selection.objects;
		if (selected == null || selected.Length == 0)
		{
			EditorUtility.DisplayDialog("Error", "Please select at least one asset.", "OK");
			return;
		}

		string bundleName = "motionblur";

		foreach (var obj in selected)
		{
			string path = AssetDatabase.GetAssetPath(obj);
			AssetImporter importer = AssetImporter.GetAtPath(path);
			if (importer != null)
				importer.assetBundleName = bundleName;
		}

		string baseDir = "Assets/AssetBundles";
		BuildTarget[] targets = {
			BuildTarget.StandaloneWindows64,
			BuildTarget.StandaloneOSXUniversal,
			BuildTarget.StandaloneLinux64
		};
		string[] platformFolders = { "Windows64", "MacOSX", "Linux64" };

		for (int i = 0; i < targets.Length; i++)
		{
			string outDir = Path.Combine(baseDir, platformFolders[i]);
			if (!Directory.Exists(outDir))
				Directory.CreateDirectory(outDir);

			BuildPipeline.BuildAssetBundles(outDir, BuildAssetBundleOptions.None, targets[i]);
		}

		foreach (var obj in selected)
		{
			string path = AssetDatabase.GetAssetPath(obj);
			AssetImporter importer = AssetImporter.GetAtPath(path);
			if (importer != null)
				importer.assetBundleName = null;
		}

		AssetDatabase.Refresh();
		EditorUtility.DisplayDialog("Finished", baseDir, "OK");
	}
}