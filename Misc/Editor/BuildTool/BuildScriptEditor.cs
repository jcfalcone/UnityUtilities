using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Falcone.BuildTool
{
    [CustomEditor(typeof(BuildEditorSettings))]
    public class BuildScriptEditor : Editor
    {
        BuildEditorSettings buildSetting;

        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Build Project"))
                {
                    BuildScript.onStep += this.OnStep;
                    BuildScript.onComplete += this.OnComplete;
                    BuildScript.BuildAll(this.buildSetting);
                }

                if (BuildScript.isBuilding)
                {
                    EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 25), BuildScript.Progress, BuildScript.currStep);
                }

                if (check.changed)
                {
                    buildSetting.UpdateStepTags();
                }
            }
        }

        private void OnEnable()
        {
            buildSetting = (BuildEditorSettings)target;
        }

        private void OnStep()
        {
            this.Repaint();
        }

        private void OnComplete()
        {
            BuildScript.onStep -= this.OnStep;
            BuildScript.onComplete -= this.OnComplete;
        }
    }
}
