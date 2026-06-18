using System.IO;
using UnityEditor;
using UnityEngine;

namespace TaloGameServices.Editor
{
    // used in CI, see create-release.yml
    public static class TaloPackageExporter
    {
        private const string SourceFolder = "Assets/Talo Game Services";
        private const string DefaultOutputPath = "Release/talo.unitypackage";

        public static void Export()
        {
            var outputPath = GetOutputPath();
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Debug.Log($"Exporting Unity package from \"{SourceFolder}\" to \"{outputPath}\".");

            try
            {
                AssetDatabase.ExportPackage(
                    new[] { SourceFolder },
                    outputPath,
                    ExportPackageOptions.Recurse);

                if (!File.Exists(outputPath))
                {
                    throw new IOException($"Expected output file was not created: \"{outputPath}\".");
                }

                Debug.Log("Unity package exported successfully.");
                EditorApplication.Exit(0);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to export Unity package: {ex}");
                EditorApplication.Exit(1);
            }
        }

        private static string GetOutputPath()
        {
            var args = System.Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-outputPath")
                {
                    return args[i + 1];
                }
            }

            return DefaultOutputPath;
        }
    }
}
