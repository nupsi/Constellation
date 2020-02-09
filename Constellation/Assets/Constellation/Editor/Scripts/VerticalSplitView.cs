using System;
using UnityEditor;
using UnityEngine;

namespace ConstellationEditor {
    public class VerticalSplitView {
        public delegate void DrawView(Rect rect);

        private DrawView leftView;
        private DrawView rightView;
        private Action repaintWindow;

        private GUIStyle splitStyle;
        private Vector2 limits = new Vector2(0.1f, 0.9f);
        private float split;
        private bool dragging;
        private int thickness = 5;

        public VerticalSplitView(DrawView left, DrawView right, Action repaint, float split) {
            Split = split;
            leftView = left;
            rightView = right;
            repaintWindow = repaint;
        }

        public void Draw(Rect view) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            leftView.Invoke(GetLeftRect(view));
            EditorGUILayout.EndVertical();
            DrawSplit(view);
            EditorGUILayout.BeginVertical();
            rightView.Invoke(GetRightRect(view));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSplit(Rect rect) {
            var splitRect = GetSplitRect(rect);
            EditorGUILayout.BeginVertical(GUILayout.Width(splitRect.width));
            EditorGUILayout.BeginHorizontal();
            GUI.Box(splitRect, string.Empty, SplitStyle);
            Drag(rect, splitRect);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void Drag(Rect parent, Rect rect) {
            var current = Event.current;
            DrawCustomCursor(dragging ? new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height)) : rect);

            if(current.button == 0) {
                if(current.type == EventType.MouseDown && rect.Contains(current.mousePosition)) {
                    dragging = true;
                }

                if(dragging) {
                    if(current.rawType == EventType.MouseUp) {
                        dragging = false;
                        repaintWindow.Invoke();
                    }

                    if(current.rawType == EventType.MouseDrag) {
                        Split = (current.mousePosition.x - parent.x) / parent.width;
                        repaintWindow.Invoke();
                    }
                }
            }
        }

        private void DrawCustomCursor(Rect rect) {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
        }

        private Rect GetLeftRect(Rect parent) {
            return new Rect(parent) {
                width = parent.width * Split
            };
        }

        private Rect GetSplitRect(Rect parent) {
            return new Rect(parent) {
                x = parent.width * Split,
                y = parent.y - 3,
                width = thickness
            };
        }

        private Rect GetRightRect(Rect parent) {
            return new Rect(parent) {
                x = (parent.width * Split) + thickness,
                width = parent.width * (1 - Split)
            };
        }

        private Texture2D GetTexture() {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0.635f, 0.635f, 0.635f));
            texture.Apply();
            return texture;
        }

        private GUIStyle SplitStyle {
            get {
                if(splitStyle == null) {
                    splitStyle = new GUIStyle();
                    splitStyle.normal.background =
                    splitStyle.active.background =
                    splitStyle.hover.background = GetTexture();
                }
                return splitStyle;
            }
        }

        private float Split {
            get {
                return split;
            }

            set {
                split = Mathf.Clamp(value, limits.x, limits.y);
            }
        }
    }
}