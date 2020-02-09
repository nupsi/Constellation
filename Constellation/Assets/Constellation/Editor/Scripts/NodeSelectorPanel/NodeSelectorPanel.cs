using System.Collections.Generic;
using Constellation;
using UnityEditor;
using UnityEngine;

namespace ConstellationEditor {
    public class NodeSelectorPanel {
        SearchBar searchBar;
        Vector2 scrollPosition;
        public delegate void NodeAdded (string nodeName, string _namespace);
        NodeAdded OnNodeAdded;
        private List<NodeNamespacesData> NodeNamespaceData;
        private string[] namespaces;

        public NodeSelectorPanel (NodeAdded _onNodeAdded, NodeNamespacesData[] customNodes) {
            OnNodeAdded = null;
            OnNodeAdded += _onNodeAdded;
            var nodes = new List<string> (NodesFactory.GetAllNodes ());
            namespaces = NodesFactory.GetAllNamespaces (nodes.ToArray());
            NodeNamespaceData = new List<NodeNamespacesData> ();
            searchBar = new SearchBar(FilterNodes);

            foreach (var _namespace in namespaces)
            {
                var nodeNamespace = new NodeNamespacesData(_namespace, nodes.ToArray());
                NodeNamespaceData.Add(nodeNamespace);
            }

            foreach (var node in customNodes)
            {
                NodeNamespaceData.Add(node);
            }
        }

        private void FilterNodes (string _filer) {
            foreach (var nodeNameData in NodeNamespaceData) {
                nodeNameData.FilterNodes (_filer);
            }
        }

        public void Draw (Rect _rect) {
            GUILayout.BeginVertical (GUILayout.Width(_rect.width + 5));
            searchBar.DrawSearchBar();
            scrollPosition = EditorGUILayout.BeginScrollView (scrollPosition);
            foreach (NodeNamespacesData nodeNamespace in NodeNamespaceData) {
                EditorGUILayout.LabelField(nodeNamespace.namespaceName, GUI.skin.GetStyle("OL Title"));
                GUILayout.Space(4);
                var selGridInt = GUILayout.SelectionGrid (-1, nodeNamespace.GetNiceNames (), 1 + (int)Mathf.Floor(_rect.width / 255));
                if (selGridInt >= 0) {
                    OnNodeAdded (nodeNamespace.GetNames () [selGridInt], nodeNamespace.namespaceName);
                }
            }
            EditorGUILayout.EndScrollView ();
            GUILayout.EndVertical ();
        }
    }
}