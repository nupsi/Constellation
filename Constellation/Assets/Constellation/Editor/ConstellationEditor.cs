using System.IO;
using UnityEngine;

namespace ConstellationEditor {
    public static class ConstellationEditor {
        private const string DefaultLocation = "Assets/Constellation/Editor/";
        private const string FolderToFind = "Constellation/Editor";
        private const string FileToFind = "ConstellationEditor";
        private const string FileExtension = ".cs";
        private static string editorPath = "";

        public static string GetEditorPath () {
            return string.IsNullOrEmpty(editorPath) ? InitializeEditorPath() : editorPath;
        }

        public static string GetEditorAssetPath () {
            return GetEditorPath() + "EditorAssets/";
        }

        private static string InitializeEditorPath() {
            foreach(var directory in Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories)) {
                var currentDirectory = directory.Replace('\\', '/');
                if(currentDirectory.EndsWith(FolderToFind)) {
                    foreach(var file in Directory.GetFiles(currentDirectory, SearchPattern)) {
                        var currentFile = file.Replace('\\', '/');
                        if(currentFile.EndsWith(FileName)) {
                            return editorPath = string.Format("{0}/", currentDirectory.Replace(Application.dataPath, "Assets"));
                        }
                    }
                }
            }
            Debug.LogError("Error finding Constellation Editor folder! Using default location: " + DefaultLocation);
            return DefaultLocation;
        }

        private static string SearchPattern {
            get {
                return string.Format("*{0}", FileExtension);
            }
        }

        private static string FileName {
            get {
                return string.Format("{0}{1}", FileToFind, FileExtension);
            }
        }
    }
}