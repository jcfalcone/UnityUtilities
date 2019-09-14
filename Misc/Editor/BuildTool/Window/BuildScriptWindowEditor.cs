using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Falcone.BuildTool
{
    public class BuildScriptWindowEditor : EditorWindow
    {

        static BuildEditorSettings currSettings;
        static BuildScriptWindowSettings settings;

        static BuildScriptWindowState stateCtrl;
        static BuildScriptWindowState.States currState;
        static BuildScriptWindowState.States nextState;

        static bool skipWelcome = false;

        [MenuItem("Build/Editor")]
        static void ShowWindow()
        {
            BuildScriptWindowEditor editor = EditorWindow.GetWindow<BuildScriptWindowEditor>();

            editor.minSize = new Vector2(800, 600);
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
                currSettings = Selection.activeObject as BuildEditorSettings;
                //ShowEditor();

                return true;
            }

            return false;
        }

        void OnGUI()
        {
            CheckState();
            stateCtrl.Tick(currState);
        }
        #endregion

        #region State
        static void CheckState()
        {
            if(currState != nextState)
            {
                stateCtrl.InitState(nextState);
                currState = nextState;
            }
        }

        #endregion

        #region Support
        static void Init()
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
        #endregion
    }
}