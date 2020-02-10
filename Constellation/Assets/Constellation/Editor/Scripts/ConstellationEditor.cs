using System.IO;
using UnityEngine;

namespace ConstellationEditor {
    public static class ConstellationEditor {
        /// <summary>
        /// Local Editor Data path for Constellation.
        /// </summary>
        private const string EditorData = "Editor/EditorData/";

        /// <summary>
        /// Local Editor Assets path for Constellation.
        /// </summary>
        private const string EditorAssets = "Editor/EditorAssets/";

        /// <summary>
        /// Constellation folder.
        /// </summary>
        private static string constellationFolder;

        /// <summary>
        /// Get Constellation Editor Data folder.
        /// </summary>
        /// <returns>Assets/.../Constellation/Editor/EditorData/</returns>
        public static string GetEditorPath() {
            return string.Format("{0}{1}", ConstellationFolder, EditorData);
        }

        /// <summary>
        /// Get Constellation Editor Assets folder.
        /// </summary>
        /// <returns>Assets/.../Constellation/Editor/EditorAssets/</returns>
        public static string GetEditorAssetPath() {
            return string.Format("{0}{1}", ConstellationFolder, EditorAssets);
        }

        /// <summary>
        /// Get Constellation folder.
        /// </summary>
        /// <returns>Asssets/.../Constellation/</returns>
        public static string GetProjectPath() {
            return ConstellationFolder;
        }

        /// <summary>
        /// Returns a local path to the firs folder named "Constellation" in the current project.
        /// </summary>
        /// <returns>Local path to Constellation folder.</returns>
        private static string FindConstellationFolder() {
            var directories = Directory.GetDirectories(Application.dataPath, "Constellation", SearchOption.AllDirectories);
            if(directories.Length > 0) {
                if(directories.Length > 1) {
                    Debug.LogWarning("Found multiple folders named \"Constellation\". Using the first found folder: " + directories[0]);
                }
                return directories[0].Replace(Application.dataPath, "Assets") + "/";
            }
            Debug.LogError("Unable to find \"Constellation\" folder");
            return null;
        }

        /// <summary>
        /// Constellation folder.
        /// </summary>
        private static string ConstellationFolder {
            get {
                return constellationFolder ?? (constellationFolder = FindConstellationFolder());
            }
        }
    }
}