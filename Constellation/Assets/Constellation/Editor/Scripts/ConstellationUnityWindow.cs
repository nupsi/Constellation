﻿using System;
using Constellation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ConstellationEditor
{
    [InitializeOnLoadAttribute]
    public class ConstellationUnityWindow : ConstellationBaseWindow, IUndoable, ICopyable, ICompilable
    {
        protected NodeEditorPanel nodeEditorPanel;
        protected ConstellationsTabPanel nodeTabPanel;
        private NodeSelectorPanel nodeSelector;
        private string currentPath;
        public static ConstellationUnityWindow WindowInstance;
        Constellation.Constellation constellation;
        protected bool requestSetup;
        protected bool requestCompilation;
        private int splitThickness = 5;
        private VerticalSplitView splitView;

        [MenuItem("Window/Constellation Editor")]
        public static void ShowWindow () {
        	CopyScriptIcons.Copy();
            WindowInstance = EditorWindow.GetWindow(typeof(ConstellationUnityWindow), false, "Constellation") as ConstellationUnityWindow;
            WindowInstance.scriptDataService = new ConstellationEditorDataService();
            WindowInstance.RequestCompilation();
        }

        protected override void ShowEditorWindow () {
            ShowWindow();
        }

        void OnSceneLoaded (Scene scene, LoadSceneMode mode) {
            RefreshNodeEditor();
        }

        [MenuItem("File/Constellation/New %&n")]
        static void NewConstellation () {
            if (WindowInstance != null)
                WindowInstance.New();
            else
            {
                ShowWindow();
                NewConstellation();
            }
        }

        public void CompileScripts () {
            if (WindowInstance.ConstellationCompiler == null)
                WindowInstance.ConstellationCompiler = new ConstellationCompiler();

            if(WindowInstance.scriptDataService == null)
            {
                WindowInstance.scriptDataService = new ConstellationEditorDataService();
            }
            //ResetWindow();
            WindowInstance.ConstellationCompiler.UpdateScriptsNodes(WindowInstance.scriptDataService.GetAllScriptsInProject(), WindowInstance.scriptDataService.GetAllNestableScriptsInProject());
        }

        public void ResetWindow()
        {
            RequestSetup();
        }

        public void RequestCompilation()
        {
            requestCompilation = true;
        }

        [MenuItem("File/Constellation/Save %&s")]
        static void SaveConstellation () {
            if (WindowInstance != null)
                WindowInstance.Save();
            else
                ShowWindow();
        }

        static void SaveConstellationInstance () {
            if (WindowInstance != null)
                WindowInstance.SaveInstance();
            else
                ShowWindow();
        }

        [MenuItem("Edit/Constellation/Copy %&c")]
        static void CopyConstellation () {
            if (WindowInstance != null)
                WindowInstance.Copy();
            else
                ShowWindow();
        }

        [MenuItem("Edit/Constellation/Paste %&v")]
        static void PasteConstellation () {
            if (WindowInstance != null)
                WindowInstance.Paste();
            else
                ShowWindow();
        }

        [MenuItem("File/Constellation/Load %&l")]
        static void LoadConstellation () {
            if (WindowInstance != null)
                WindowInstance.Open();
            else {
                ShowWindow();
                WindowInstance.Open();
            }
        }

        [MenuItem("Edit/Constellation/Undo %&z")]
        static void UndoConstellation () {
            if (WindowInstance != null)
                WindowInstance.Undo();
            else
                ShowWindow();
        }

        [MenuItem("Edit/Constellation/Redo %&y")]
        static void RedoConstellation () {
            if (WindowInstance != null)
                WindowInstance.Redo();
            else
                ShowWindow();
        }

        public void Undo () {
            scriptDataService.Undo();
            RefreshNodeEditor();
        }

        public void Redo () {
            scriptDataService.Redo();
            RefreshNodeEditor();
        }

        public void Copy () {
            scriptDataService.GetEditorData().clipBoard.AddSelection(nodeEditorPanel.GetNodeSelection().SelectedNodes.ToArray(), nodeEditorPanel.GetLinks());
        }

        public void Paste () {
            var pastedNodes = scriptDataService.GetEditorData().clipBoard.PasteClipBoard(scriptDataService.GetCurrentScript());
            RefreshNodeEditor();
            nodeEditorPanel.SelectNodes(pastedNodes);
        }

        public void Cut()
        {

        }

        public void AddAction () {
            scriptDataService.AddAction();
        }

        void OnDestroy () {
            WindowInstance = null;
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected void RefreshNodeEditor () {
            canDrawUI = false;
            if (scriptDataService != null) {
                previousSelectedGameObject = null;
                nodeEditorPanel = new NodeEditorPanel(this,
                    this,
                    scriptDataService.GetCurrentScript(),
                    this,
                    scriptDataService.GetEditorData().clipBoard,
                    scriptDataService.GetLastEditorScrollPositionX(), scriptDataService.GetLastEditorScrollPositionY(), // Editor Position
                    OnLinkAdded, OnLinkRemoved, OnNodeAdded, OnNodeRemoved, OnHelpRequested, SaveConstellationInstance,
                    scriptDataService.GetAllNestableScriptsInProject()); // CallBacks 
                nodeTabPanel = new ConstellationsTabPanel(this);
            }
        }

        private void OnHelpRequested (string nodeName) {
            if (Application.isPlaying) {
                if (EditorUtility.DisplayDialog("Exit play mode", "You need to exit play mode in order to open a Constellation help.", "Continue", "Stop Playing"))
                    return;

                EditorApplication.isPlaying = false;
                return;
            }
            scriptDataService.GetEditorData().ExampleData.openExampleConstellation = true;
            scriptDataService.GetEditorData().ExampleData.constellationName = nodeName;
            EditorApplication.isPlaying = true;
        }

        
        public void RequestSetup()
        {
            requestSetup = true;
        }

        protected override void Setup () {

            wantsMouseMove = true;
            canDrawUI = false;
            WindowInstance = this as ConstellationUnityWindow;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EditorApplication.playModeStateChanged += OnPlayStateChanged;
            scriptDataService = new ConstellationEditorDataService();
            ConstellationCompiler = new ConstellationCompiler();
            scriptDataService.RefreshConstellationEditorDataList();

            if (scriptDataService.OpenEditorData().LastOpenedConstellationPath == null)
                return;

            if (scriptDataService.OpenEditorData().LastOpenedConstellationPath.Count != 0)
            {
                var scriptData = scriptDataService.Recover(scriptDataService.OpenEditorData().LastOpenedConstellationPath[0]);
            }
            RequestCompilation();
            if (scriptDataService != null) {
                nodeEditorPanel = new NodeEditorPanel(this,
                    this,
                    scriptDataService.GetCurrentScript(),
                    this,
                    scriptDataService.GetEditorData().clipBoard,
                    scriptDataService.GetLastEditorScrollPositionX(), scriptDataService.GetLastEditorScrollPositionY(), // Saved editor position
                    OnLinkAdded, OnLinkRemoved, OnNodeAdded, OnNodeRemoved, OnHelpRequested, // callBacks
                    SaveConstellationInstance,
                    scriptDataService.GetAllNestableScriptsInProject());
                nodeTabPanel = new ConstellationsTabPanel(this);
                if (scriptDataService.GetCurrentScript() != null)
                    WindowInstance.titleContent.text = scriptDataService.GetCurrentScript().name;
                else
                    WindowInstance.titleContent.text = "Constellation";
                scriptDataService.ClearActions();
            }
            nodeSelector = new NodeSelectorPanel(OnNodeAddRequested, scriptDataService.GetAllCustomNodesNames());

            splitView = new VerticalSplitView(this,
                nodeEditorPanel.DrawNodeEditor,
                nodeSelector.Draw,
                0.7f);
        }

        void OnGUI () {
            try {

                if (Event.current.type == EventType.Layout) {
                    canDrawUI = true;
                }

                if (Event.current.type == EventType.MouseMove) {
                    RequestRepaint();
                }

                if (canDrawUI) {
                    if (IsConstellationSelected()) {
                        DrawGUI();
                    }
                    else if (!IsConstellationSelected()) {
                        //nodeEditorPanel.DrawBackground(Color.white, position.width, position.height);
                        DrawStartGUI();
                    }
                } else {
                    Repaint();
                    GUIStyle loadingStyle = new GUIStyle();
                    loadingStyle.normal.textColor = Color.white;
                    loadingStyle.fontSize = 50;
                    loadingStyle.alignment = TextAnchor.MiddleCenter;
                    nodeEditorPanel.DrawBackground(Color.white, position.width, position.height);
                    GUI.Label(new Rect(0, 0, position.width, position.height), "Loading...", loadingStyle);
                }
            }
            catch (ConstellationError e)
            {
                ShowError(e);
            }
            catch (Exception e)
            {
                var formatedError = new UnknowError(this.GetType().Name);
                ShowError(formatedError, e);
            }
        }

        protected virtual void DrawStartGUI () {
            StartPanel.Draw(this);
            Recover();
        }

        protected void OnNodeAddRequested (string nodeName, string _namespace) {
            nodeEditorPanel.AddNode(nodeName, _namespace);
        }

        protected virtual void DrawGUI () {
            TopBarPanel.Draw(this, this, this, this);
            var constellationName = nodeTabPanel.Draw(scriptDataService.currentPath.ToArray(), CurrentEditedInstancesName);
            if (constellationName != null)
                Open(constellationName);

            var constellationToRemove = nodeTabPanel.ConstellationToRemove();
            scriptDataService.CloseOpenedConstellation(constellationToRemove);
            if (constellationToRemove != "" && constellationToRemove != null) {
                RequestSetup();
            }

            splitView.Draw(new Rect(0, 35, position.width - 10, position.height - 32));
            RepaintIfRequested();
        }

        static void OnPlayStateChanged (PlayModeStateChange state) {
            WindowInstance.RequestSetup();
            WindowInstance.previousSelectedGameObject = null;
            WindowInstance.ResetInstances();
            if (WindowInstance.scriptDataService.GetEditorData().ExampleData.openExampleConstellation && state == PlayModeStateChange.EnteredPlayMode) {
                var nodeExampleLoader = new ExampleSceneLoader();
                nodeExampleLoader.RunExample(WindowInstance.scriptDataService.GetEditorData().ExampleData.constellationName, WindowInstance.scriptDataService);
                WindowInstance.scriptDataService.GetEditorData().ExampleData.openExampleConstellation = false;
            }

            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
            WindowInstance.RequestRepaint();
        }

        void Update () {
            try {
                if (Application.isPlaying && IsConstellationSelected()) {
                    RequestRepaint();
                    if (nodeEditorPanel != null && previousSelectedGameObject != null && scriptDataService.GetCurrentScript().IsInstance) {
                        nodeEditorPanel.Update(currentEditableConstellation.GetConstellation());
                    }

                    var selectedGameObjects = Selection.gameObjects;
                    if (selectedGameObjects.Length == 0 || selectedGameObjects[0] == previousSelectedGameObject || selectedGameObjects[0].activeInHierarchy == false)
                        return;
                    else if (scriptDataService.GetCurrentScript().IsInstance) {
                        scriptDataService.CloseCurrentConstellationInstance();
                        previousSelectedGameObject = selectedGameObjects[0];
                        RequestSetup();
                    }

                    var selectedConstellation = selectedGameObjects[0].GetComponent<ConstellationEditable>() as ConstellationEditable;
                    if (selectedConstellation != null) {
                        currentEditableConstellation = selectedConstellation;
                        previousSelectedGameObject = selectedGameObjects[0];
                        OpenConstellationInstance(selectedConstellation.GetConstellation(), AssetDatabase.GetAssetPath(selectedConstellation.GetConstellationData()));
                        if (selectedConstellation.GetConstellation() == null)
                        {
                            return;
                        }
                        selectedConstellation.Initialize();
                    }

                    if(requestSetup == true)
                    {
                        Setup();
                        requestSetup = false;
                    }

                    if(requestCompilation == true)
                    {
                        CompileScripts();
                        requestCompilation = false;
                    }
                }
            } catch (ConstellationError e) {
                ShowError(e);
            } catch (Exception e) {
                var unknowError = new UnknowError(this.GetType().Name);
                ShowError(unknowError, e);
            }
        }

        protected virtual void OnLostFocus () {
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
            EditorApplication.playModeStateChanged += OnPlayStateChanged;
        }

        protected override void ShowError(ConstellationError constellationError, Exception exception = null)
        {
            base.ShowError(constellationError, exception);
            if(constellationError != null){
                if(constellationError.GetError().IsCloseEditor())
                    WindowInstance.Close();
            }

        }
    }
}