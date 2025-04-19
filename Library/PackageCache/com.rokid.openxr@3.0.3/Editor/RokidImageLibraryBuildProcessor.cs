using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.ARSubsystems;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.Features;

namespace Rokid.UXR.Editor
{
    class RokidImageLibraryBuildProcessor : IPreprocessBuildWithReport, ARBuildProcessor.IPreprocessBuild
    {
        public int callbackOrder => 2;

        static void Rethrow(XRReferenceImageLibrary library, string message, Exception innerException) =>
            throw new Exception($"\n-\n-\n-\nError building {nameof(XRReferenceImageLibrary)} {AssetDatabase.GetAssetPath(library)}: {message}\n-\n-\n-", innerException);

        static void BuildAssets()
        {
            
            
            var assets = AssetDatabase.FindAssets($"t:{nameof(XRReferenceImageLibrary)}");
            var libraries = assets
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>);

            var index = 0;
            
            Debug.Log("RK-ARFoundation-Build: BuildAssets() libraries.size="+libraries.Count());
            
            foreach (var library in libraries)
            {
                index++;
                
                EditorUtility.DisplayProgressBar(
                    $"Compiling {nameof(XRReferenceImageLibrary)} ({index} of {assets.Length})",
                    $"{AssetDatabase.GetAssetPath(library)} ({library.count} image{(library.count == 1 ? "" : "s")})",
                    (float)index / assets.Length);

                try
                {
                    library.SetDataForKey(RokidImageDatabase.dataStoreKey, RokidImage.BuildDb(library));
                }
                catch (RokidImage.MissingTextureException e)
                {
                    Rethrow(library, $"RK-ARFoundation-Build: {nameof(XRReferenceImage)} named '{e.referenceImage.name}' is missing a texture.", e);
                }
                catch (RokidImage.EncodeToPNGFailedException e)
                {
                    Rethrow(library, $"RK-ARFoundation-Build: {nameof(XRReferenceImage)} named '{e.referenceImage.name}' could not be encoded to a PNG. Please check other errors in the console window.", e);
                }
                catch (RokidImage.BuildDatabaseFailedException e)
                {
                    Rethrow(library, $"RK-ARFoundation-Build: The rokidimage command line tool exited with code ({e.exitCode}) and stderr:\n{e.stdErr}", e);
                }
                catch (RokidImage.EmptyDatabaseException e)
                {
                    Rethrow(library, $"RK-ARFoundation-Build: The rokidimage command line tool ran successfully but did not produce an image database. This is likely a bug. Please provide these details to Unity:\n=== begin rokidimage output ===\nstdout:\n{e.stdOut}\nstderr:\n{e.stdErr}\n=== end rokidimage output===\n", e);
                }
            }
            EditorUtility.ClearProgressBar();
        }

        void ARBuildProcessor.IPreprocessBuild.OnPreprocessBuild(PreprocessBuildEventArgs buildEventArgs)
        {
            Debug.Log("RK-ARFoundation-Build: ARBuildProcessor.IPreprocessBuild.OnPreprocessBuild");

            BuildAssets();
        }

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("RK-ARFoundation-Build: IPreprocessBuildWithReport.OnPreprocessBuild");
            
            if (report.summary.platform != BuildTarget.Android)
                return;

            BuildAssets();
        }
    }
}
