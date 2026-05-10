using UnityEditor;
using System.IO;
using System.Linq;

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

		string outputDir;
		MoveMotionBlurFiles(baseDir, out outputDir);

		EditorUtility.DisplayDialog("Finished", "AssetBundles built to:" + System.Environment.NewLine + outputDir, "OK");
	}
		
	private static void MoveMotionBlurFiles(string baseDir, out string outputDir)
	{
		string outputRoot = "output";
		outputDir = Path.GetFullPath(outputRoot);

		if (!Directory.Exists(baseDir))
		{
			return;
		}
			
		var filesToMove = Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories)
			.Where(f => {
				string fileName = Path.GetFileName(f);
				bool isMotionBlur = string.Equals(fileName, "motionblur", System.StringComparison.OrdinalIgnoreCase);
				return isMotionBlur;
			})
			.ToArray();

		if (filesToMove.Length == 0)
		{
			return;
		}

		if (!Directory.Exists(outputRoot))
			Directory.CreateDirectory(outputRoot);
		else
			Directory.Delete(outputDir, true);

		int movedCount = 0;
		foreach (string fullPath in filesToMove)
		{
			string relativePath = GetRelativePath(baseDir, fullPath);
			string destPath = Path.Combine(outputRoot, relativePath);
			string destDir = Path.GetDirectoryName(destPath);
			if (!Directory.Exists(destDir))
				Directory.CreateDirectory(destDir);

			if (File.Exists(destPath))
				File.Delete(destPath);
			File.Copy(fullPath, destPath);
			movedCount++;
		}
			
		AssetDatabase.Refresh();
	}
		
	private static string GetRelativePath(string basePath, string fullPath)
	{
		basePath = Path.GetFullPath(basePath);
		fullPath = Path.GetFullPath(fullPath);

		if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
			basePath += Path.DirectorySeparatorChar;

		var baseUri = new System.Uri(basePath);
		var fullUri = new System.Uri(fullPath);

		return System.Uri.UnescapeDataString(
			baseUri.MakeRelativeUri(fullUri).ToString()
		).Replace('/', Path.DirectorySeparatorChar);
	}
}