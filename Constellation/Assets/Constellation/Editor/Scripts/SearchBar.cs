using UnityEditor;
using UnityEngine;

namespace ConstellationEditor {
    public class SearchBar {
        public delegate void OnSearchChange(string text);

        private OnSearchChange OnChange;
        private string text;

        public SearchBar(OnSearchChange onChange) {
            OnChange = onChange;
            text = string.Empty;
        }

        public void DrawSearchBar() {
            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;
            GUILayout.BeginHorizontal(ToolBarStyle);
            DrawSearchField();
            DrawClearSearch();
            GUILayout.EndHorizontal();
        }

        private void DrawSearchField() {
            var newText = GUILayout.TextField(text, TextFieldStyle);
            if(newText != text) {
                SetText(newText);
            }
        }

        private void DrawClearSearch() {
            if(GUILayout.Button(string.Empty, CancelButtonStyle)) {
                SetText(string.Empty);
                GUI.FocusControl(null);
            }
        }

        private void SetText(string newText) {
            OnChange.Invoke(text = newText);
        }

        private GUIStyle ToolBarStyle {
            get {
                return GUI.skin.FindStyle("Toolbar");
            }
        }

        private GUIStyle TextFieldStyle {
            get {
                return GUI.skin.FindStyle("ToolbarSeachTextField");
            }
        }

        private GUIStyle CancelButtonStyle {
            get {
                return string.IsNullOrEmpty(text)
                    ? GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty")
                    : GUI.skin.FindStyle("ToolbarSeachCancelButton");
            }
        }
    }
}