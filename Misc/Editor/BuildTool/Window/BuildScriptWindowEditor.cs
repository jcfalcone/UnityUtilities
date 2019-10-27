using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Falcone.BuildTool
{
    public class BuildScriptWindowEditor : EditorWindow
    {

        static BuildEditorSettings staticSettings;
        public BuildEditorSettings currSettings;
        public static BuildScriptWindowSettings settings;

        static BuildScriptWindowState stateCtrl;
        public BuildScriptWindowState.States currState;
        public BuildScriptWindowState.States m_nextState;

        public BuildScriptWindowState.States nextState
        {
            get
            {
                return m_nextState;
            }
            set
            {
                fadeStartTime = Time.realtimeSinceStartup;
                Debug.Log("Changed State - "+value);
                m_nextState = value;
            }
        }

        static bool skipWelcome = false;

        public static BuildScriptWindowEditor editor;

        float currFadeVal = 1;
        float fadeStartTime = 0;
        float fadeTime = 0.5f;

        [MenuItem("Build/Editor")]
        static void ShowWindow()
        {
            editor = EditorWindow.GetWindow<BuildScriptWindowEditor>();

            editor.minSize = new Vector2(250, 150);
            editor.titleContent.text = "Build Steps";
            editor.titleContent.image = GetSettings().icon;

            skipWelcome = EditorPrefs.GetBool("SkipWelcomeScreen");

            stateCtrl = new BuildScriptWindowState();
            stateCtrl.Init();
        }

        #region Unity Events
        void OnEnable()
        {
            skipWelcome = EditorPrefs.GetBool("SkipWelcomeScreen");
            currState = BuildScriptWindowState.States.Welcome;

            if(stateCtrl == null)
            {
                stateCtrl = new BuildScriptWindowState();
                stateCtrl.Init();
            }

            //settings = Resources.Load("EditorSettings") as BuildEditorSettings;
        }

        [UnityEditor.Callbacks.OnOpenAsset(1)]
        public static bool OnOpenAsset(int _instanceID, int _line)
        {
            if (Selection.activeObject is BuildEditorSettings)
            {
                staticSettings = Selection.activeObject as BuildEditorSettings;
                ShowWindow();

                return true;
            }

            return false;
        }

        void OnGUI()
        {
            if(staticSettings != null)
            {
                this.SetLocalSetting();
            }

            CheckState(currSettings, this);

            if(Selection.objects.Length > 0 && Selection.objects[0] is BuildEditorSettings)
            {
                BuildEditorSettings selectSetting = Selection.objects[0] as BuildEditorSettings;

                if(selectSetting != currSettings)
                {
                    staticSettings = selectSetting;
                    this.SetLocalSetting();
                }
            }

            EditorGUILayout.BeginFadeGroup(this.currFadeVal);

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                stateCtrl.Tick(currState, currSettings, this, GetSettings());

                if (check.changed)
                {
                    Undo.RegisterCompleteObjectUndo(currSettings, "Build Step Editor Undo");
                    
                    EditorUtility.SetDirty(currSettings);
                }
            }

            EditorGUILayout.EndFadeGroup();
        }
        #endregion

        #region State
        void CheckState(BuildEditorSettings _build, BuildScriptWindowEditor _editor)
        {
            if(currState != nextState)
            {
                stateCtrl.InitState(nextState, _build, _editor);
                currState = nextState;
            }
        }

        #endregion

        #region Support
        void Init()
        {
            if(!EditorPrefs.HasKey("SkipWelcomeScreen"))
            {
                currState = BuildScriptWindowState.States.Welcome;
                EditorPrefs.SetBool("SkipWelcomeScreen", false);
            }
        }

        static BuildScriptWindowSettings GetSettings()
        {
            if (settings == null)
            {
                settings = FileUtility.GetAssetByType<BuildScriptWindowSettings>();
            }

            return settings;
        }

        public void OnUndoRedo()
        {
            this.Repaint();
        }

        public void SetLocalSetting()
        {
            this.currSettings = staticSettings;
            Undo.RegisterCompleteObjectUndo(currSettings, "Build Step Editor Undo");

            Undo.undoRedoPerformed += this.OnUndoRedo;
        }
        #endregion
    }
}