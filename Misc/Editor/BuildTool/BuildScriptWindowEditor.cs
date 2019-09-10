using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Falcone.BuildTool
{
    public class BuildScriptWindowEdior : EditorWindow
    {
        static BuildEditorSettings settings;

        [MenuItem("Window/BehaviourEditor/Editor")]
        void ShowWindow()
        {

        }

        void OnEnable()
        {
            settings = Resources.Load("EditorSettings") as BuildEditorSettings;
        }
    }
}