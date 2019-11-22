using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Falcone.BuildTool
{
    [CreateAssetMenu(menuName = "Tools/Build/Settings")]
    public class BuildEditorSettings : ScriptableObject
    {
        [System.Serializable]
        public class Step
        {
            public string Name;
            public string Labels;
            public bool defaultSettings;

            [Space]
            public bool zipBuild;
            public bool mainBuild;

            [Space]
            public TemplateBuildAction overwriteStep;

            [Space]
            [Header("Build")]
            public BuildTarget Target;
            public BuildOptions Option;

            [Space]
            [Header("Path")]
            public bool overwritePath;

            [ConditionalHideAttribute("overwritePath")]
            public string path;

            [Space]
            [Header("Scene")]
            public bool overwriteScenes;

            [ConditionalHideAttribute("overwritePath")]
            public List<Object> scenes;

            [Space]
            [Header("Actions")]
            public List<TemplateBuildAction> preBuildActions;
            public List<TemplateBuildAction> postBuildActions;

            public Step()
            {
                this.Labels = "All";

                //this.Target = EditorUserBuildSettings.activeBuildTarget;
                this.Option = GetBuildOptions().options;

                this.overwriteScenes = false;
                this.scenes = new List<Object>();
                this.preBuildActions = new List<TemplateBuildAction>();
                this.postBuildActions = new List<TemplateBuildAction>();
        }

            public static BuildPlayerOptions GetBuildOptions()
            {
                // Get static internal "GetBuildPlayerOptionsInternal" method
                MethodInfo method = typeof(BuildPlayerWindow).GetMethod( "GetBuildPlayerOptionsInternal",
                                                                         BindingFlags.NonPublic | 
                                                                         BindingFlags.Static);

                if(method == null)
                {
                    return new BuildPlayerOptions();
                }

                // invoke internal method
                return (BuildPlayerOptions)method.Invoke( null, new object[] { false, new BuildPlayerOptions() });
            }
        }

        [Header("Project")]
        public bool controlVersion;

        [ConditionalHideAttribute("controlVersion")]
        public int Sequence;

        [ConditionalHideAttribute("controlVersion")]
        public string Version;

        [Header("File")]
        public string Path;
        public string File;

        [Space]
        [Header("Actions")]
        public List<TemplateBuildAction> preBuildActions = new List<TemplateBuildAction>();
        public List<TemplateBuildAction> postBuildActions = new List<TemplateBuildAction>();

        [Space]
        [Header("Build")]

        public List<Step> Steps = new List<Step>();

        public void UpdateStepTags()
        {
            string[] targets = BuildScriptUtilities.GetAllShortTargetName();

            for(int count = 0; count < this.Steps.Count; count++)
            {
                for (int countT = 0; countT < targets.Length; countT++)
                {
                    this.Steps[count].Labels = this.Steps[count].Labels.Replace(targets[countT], "");
                }

                this.Steps[count].Labels = this.Steps[count].Labels.Replace("  ", "");
                this.Steps[count].Labels = this.Steps[count].Labels.Trim();

                this.Steps[count].Labels += ((this.Steps[count].Labels.Length > 0)? " " : "") + BuildScriptUtilities.GetShortTargetName(this.Steps[count].Target);
            }
        }

        public void Clear()
        {
            this.controlVersion = false;
            this.Sequence = 0;
            this.Version = string.Empty;
            this.Path = string.Empty;
            this.File = string.Empty;

            this.preBuildActions.Clear();
            this.postBuildActions.Clear();
            this.Steps.Clear();
        }
    }
}