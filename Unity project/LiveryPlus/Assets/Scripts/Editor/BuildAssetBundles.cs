using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildAssetBundles
{
    [MenuItem("LiveryPlus/Build AssetBundles")]
    public static void BuildAllWithExtension()
    {
        const string outputPath = "Assets/AssetBundles";
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var names = AssetDatabase.GetAllAssetBundleNames();
        var builds = names.Select(name => new AssetBundleBuild 
		{
			assetBundleName = name + ".liveryplus",
			assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(name)
		}).ToArray();

        BuildPipeline.BuildAssetBundles(outputPath, builds, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget );
        Debug.Log($"Built {builds.Length} bundles to {outputPath}");
    }
}