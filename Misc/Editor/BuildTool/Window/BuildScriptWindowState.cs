﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

namespace Falcone.BuildTool
{
    public class StringComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return (new CaseInsensitiveComparer()).Compare(x.ToString(), y.ToString());
        }
    }

    [System.Serializable]
    public class BuildScriptWindowState
    {
        [System.Serializable]
        public class MenuOption
        {
            public string Folder;
            public BuildEditorSettings BuildSetting;
            public TemplateBuildAction Script;
            public BuildEditorSettings.Step Step;
            public List<TemplateBuildAction> List;

            public MenuOption(string _Folder, BuildEditorSettings _build, TemplateBuildAction _Script, List<TemplateBuildAction> _list)
            {
                Folder = _Folder;
                BuildSetting = _build;
                Script = _Script;
                List = _list;
            }

            public MenuOption(string _Folder, BuildEditorSettings _build, TemplateBuildAction _Script, BuildEditorSettings.Step _step)
            {
                Folder = _Folder;
                BuildSetting = _build;
                Script = _Script;
                Step = _step;
            }
        }

        public enum States
        {
            Welcome,

            NewPath,
            NewProject,
            NewPlatform,
            NewActions,
            NewStep,
            NewEnd,

            BuildStep,

            ChangeSettings
        }

        public enum SmartAreaType
        {
            None = -1,

            File,
            Path,
            Version
        }

        Vector2 scrollPosition;
        Vector2 scrollPosition2;

        Vector2 smartAreaScrollPos;

        string lastSmartType;

        ReorderableList actionsPreBuild;
        ReorderableList actionsPostBuild;

        ReorderableList WZactionsPreBuild;
        ReorderableList WZactionsPostBuild;

        ReorderableList WZactionsStepPreBuild;
        ReorderableList WZactionsStepPostBuild;
        ReorderableList WZScene;

        string saveTempFilePath = "";

        int newCurrStep = 0;

        List<Dictionary<string, ReorderableList>> StepsLists;

        public void Init()
        {

        }

        public void InitState(States _state, BuildEditorSettings _build, BuildScriptWindowEditor _editor)
        {
            switch (_state)
            {
                case States.Welcome:
                    this.InitWelcome(_build, _editor);
                    break;
                case States.ChangeSettings:
                    this.InitChangeSettings(_build, _editor);
                    break;
                case States.NewPath:
                    this.InitNewPath(_build, _editor);
                    break;
                case States.NewProject:
                    this.InitNewProject(_build, _editor);
                    break;
                case States.NewActions:
                    this.InitNewActions(_build, _editor);
                    break;
                case States.NewPlatform:
                    this.InitNewPlatform(_build, _editor);
                    break;
                case States.NewStep:
                    this.InitNewStep(_build, _editor);
                    break;
            }
        }

        public void Tick(States _state, BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings)
        {
            Debug.Log("Tick " + _state);
            switch (_state)
            {
                case States.Welcome:
                    this.Welcome(_build, _editor, _settings);
                    break;
                case States.BuildStep:
                    this.BuildStep(_build, _editor, _settings);
                    break;
                case States.ChangeSettings:
                    this.ChangeSettings(_build, _editor, _settings);
                    break;
                case States.NewPath:
                    this.NewPath(_build, _editor, _settings);
                    break;
                case States.NewProject:
                    this.NewProject(_build, _editor, _settings);
                    break;
                case States.NewActions:
                    this.NewActions(_build, _editor, _settings);
                    break;
                case States.NewPlatform:
                    this.NewPlatform(_build, _editor, _settings);
                    break;
                case States.NewStep:
                    this.NewStep(_build, _editor, _settings);
                    break;
                case States.NewEnd:
                    this.NewEnd(_build, _editor, _settings);
                    break;
            }
        }

        #region Welcome
        public void InitWelcome(BuildEditorSettings _build, BuildScriptWindowEditor _editor)
        {

        }

        public void Welcome(BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings)
        {
            EditorStyles.label.wordWrap = true;

            /*_settings.style.WelcomeGroup = new GUIStyle(EditorStyles.helpBox);
            _settings.style.WelcomeBuildButton = new GUIStyle(GUI.skin.button);
            _settings.style.WelcomeButtons = new GUIStyle(GUI.skin.button);
            _settings.style.StepGroup = EditorStyles.helpBox;
            EditorUtility.SetDirty(_settings);*/

            GUILayout.BeginVertical();

            UIUtility.Space();

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Build Steps", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            GUILayout.BeginVertical(_settings.style.WelcomeGroup);

            GUILayout.Label("Build Steps is a tool to help you manage the many different builds your project require, automatize pre and post build actions, build using command line and more");

            EditorGUI.BeginDisabledGroup(_build == null);

            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Build Project", _settings.style.WelcomeBuildButton))
            {
                BuildScript.BuildAll(_build);
            }

            if (GUILayout.Button("Build a Step...", _settings.style.WelcomeBuildButton))
            {
                _editor.nextState = States.BuildStep;
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Edit Settings", _settings.style.WelcomeBuildButton))
            {
                _editor.nextState = States.ChangeSettings;
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.EndVertical();

            UIUtility.Space();

            if (_editor.position.width > 300)
            {
                EditorGUILayout.BeginHorizontal();
            }

            float minWidth = _editor.position.width ;

            if (_editor.position.width > 300)
            {
                minWidth = _editor.position.width / 2f;
            }

            minWidth -= _settings.style.WelcomeGroup.margin.left;
            minWidth -= _settings.style.WelcomeGroup.margin.right;

            GUILayoutOption[] options = { GUILayout.MinHeight(Mathf.Clamp(200f * Mathf.Clamp01(1f - (_editor.position.width / 900f)), 90f, 200f)), GUILayout.MinWidth(minWidth) };

            if (_editor.position.width < 300)
            {
                options[0] = GUILayout.MinHeight(100);
            }

            GUILayout.BeginVertical(_settings.style.WelcomeGroup, options);

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Wizard", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            EditorGUILayout.LabelField("Start a new Build file using our wizard!");
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create a new File", _settings.style.WelcomeButtons))
            {
                _editor.nextState = States.NewPath;
                this.lastSmartType = "NONE";
                _settings.tempSettings.Clear();
            }

            GUILayout.EndVertical();

            if (_editor.position.width <= 300)
            {
                UIUtility.Space();
            }

            GUILayout.BeginVertical(_settings.style.WelcomeGroup, options);

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Online Documentation", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            EditorGUILayout.LabelField("Having any problem? Looking for more in details? Take a look at our online documentation!");
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Documentation", _settings.style.WelcomeButtons))
            {
                Help.BrowseURL("http://falcone.ai");
            }

            GUILayout.EndVertical();

            if (_editor.position.width > 300)
            {
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
        #endregion

        #region BuildStep
        public void BuildStep(BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings)
        {
            GUILayout.BeginVertical();

            UIUtility.Space();

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Build Steps", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, _settings.style.WelcomeGroup);

            if (_build != null)
            {
                for (int count = 0; count < _build.Steps.Count; count++)
                {
                    if (GUILayout.Button("Build " + _build.Steps[count].Name, _settings.style.WelcomeButtons))
                    {
                        BuildScript.BuildAll(_build, BuildScript.ExtraSettings.Default(), count);
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Back", _settings.style.WelcomeButtons))
            {
                _editor.nextState = States.Welcome;
            }

            UIUtility.Space(4);

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
        #endregion

        #region Edit Settings
        public void InitChangeSettings(BuildEditorSettings _build, BuildScriptWindowEditor _editor)
        {
            this.CreateEditLists(_build);

            BuildScript.CreateSampleDictionary();
        }

        public void ChangeSettings(BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {

                GUILayout.BeginVertical();

                UIUtility.Space();

                UIUtility.BeginCenterGroup();
                GUILayout.Label("Edit Settings: "+ _build.name, EditorStyles.boldLabel);
                UIUtility.EndCenterGroup();

                UIUtility.Space();

                this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, _settings.style.WelcomeGroup);

                UIUtility.SubTitle("Version");

                _build.controlVersion = EditorGUILayout.Toggle("Control Project Version", _build.controlVersion);
                UIUtility.Space();

                EditorGUI.BeginDisabledGroup(!_build.controlVersion);

                GUILayout.BeginHorizontal();
                _build.Version = EditorGUILayout.TextField("Version Template", _build.Version);

                if(GUILayout.Button("Add...", GUILayout.MaxWidth(50)))
                {
                    GUI.FocusControl(null);

                    GenericMenu menu = this.GetTagsMenu((_object) =>
                                                        {
                                                            string tag = _object as string;

                                                            _build.Version += "{" + tag + "}";
                                                        });

                    menu.ShowAsContext();
                }

                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField("                ", "Ex.: "+BuildScript.ParseString(_build.Version), GUI.skin.label);
                GUILayout.EndHorizontal();

                EditorGUILayout.IntField("Build Sequence", _build.Sequence);

                EditorGUI.EndDisabledGroup();

                UIUtility.Space();

                UIUtility.SubTitle("Build");


                GUILayout.BeginHorizontal();
                _build.Path = EditorGUILayout.TextField("Destine Path", _build.Path);

                if(GUILayout.Button("...", GUILayout.MaxWidth(50)))
                {
                    _build.Path = EditorUtility.SaveFolderPanel("Build End Path", _build.Path, "Build");
                }

                if (GUILayout.Button("Add...", GUILayout.MaxWidth(50)))
                {
                    GUI.FocusControl(null);

                    GenericMenu menu = this.GetTagsMenu((_object) =>
                                                        {
                                                            string tag = _object as string;

                                                            _build.Path += "{" + tag + "}";
                                                        });

                    menu.ShowAsContext();
                }

                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                _build.File = EditorGUILayout.TextField("File Name", _build.File);

                if (GUILayout.Button("Add...", GUILayout.MaxWidth(50)))
                {
                    GUI.FocusControl(null);

                    GenericMenu menu = this.GetTagsMenu((_object) =>
                                                        {
                                                            string tag = _object as string;

                                                            _build.File += "{" + tag + "}";
                                                        });

                    menu.ShowAsContext();
                }

                GUILayout.EndHorizontal();

                UIUtility.Space();
                UIUtility.SubTitle("Actions");

                this.CreateEditLists(_build);

                this.actionsPreBuild.DoLayoutList();
                UIUtility.Space();

                this.actionsPostBuild.DoLayoutList();
                UIUtility.Space();

                UIUtility.SubTitle("Steps");

                if (this.StepsLists == null || this.StepsLists.Count != _build.Steps.Count)
                {
                    this.StepsLists = new List<Dictionary<string, ReorderableList>>(_build.Steps.Count);

                    for (int count = 0; count < _build.Steps.Count; count++)
                    {
                        this.StepsLists.Add(new Dictionary<string, ReorderableList>());
                    }
                }

                ReorderableList tempList = null;

                for (int count = 0; count < _build.Steps.Count; count++)
                {

                    GUILayout.BeginVertical(_settings.style.WelcomeGroup);

                    UIUtility.BeginCenterGroup();
                    GUILayout.Label(_build.Steps[count].Name, EditorStyles.boldLabel);
                    UIUtility.EndCenterGroup();

                    UIUtility.Space(1, GUILayout.MaxHeight(25));

                    _build.Steps[count].Name = EditorGUILayout.TextField("Name", _build.Steps[count].Name);
                    _build.Steps[count].Labels = EditorGUILayout.TextField("Label", _build.Steps[count].Labels);

                    GUILayout.BeginHorizontal();
                    _build.Steps[count].overwriteStep = (TemplateBuildAction)EditorGUILayout.ObjectField("Overwrite Step", _build.Steps[count].overwriteStep, typeof(TemplateBuildAction), false);

                    if (GUILayout.Button("Overwrite...", GUILayout.MaxWidth(80)))
                    {
                        GenericMenu menu = this.GetActionsMenu("Overwrite", _build, _build.Steps[count]);
                        menu.ShowAsContext();
                    }

                    GUILayout.EndHorizontal();

                    if (_build.Steps[count].overwriteStep == null)
                    {
                        UIUtility.Space();
                        _build.Steps[count].Target = (BuildTarget)EditorGUILayout.EnumPopup("Target Platform", _build.Steps[count].Target);
                        _build.Steps[count].Option = (BuildOptions)EditorGUILayout.EnumFlagsField("Options", _build.Steps[count].Option);

                        UIUtility.Space();
                        _build.Steps[count].mainBuild = EditorGUILayout.Toggle("Main Build?", _build.Steps[count].mainBuild);
                        _build.Steps[count].zipBuild = EditorGUILayout.Toggle("Zip Build?", _build.Steps[count].zipBuild);
                        _build.Steps[count].overwritePath = EditorGUILayout.Toggle("Custom Path?", _build.Steps[count].overwritePath);

                        EditorGUI.BeginDisabledGroup(!_build.Steps[count].overwritePath);

                        GUILayout.BeginHorizontal();
                        _build.Steps[count].path = EditorGUILayout.TextField("Path", _build.Steps[count].path);

                        if (GUILayout.Button("...", GUILayout.MaxWidth(50)))
                        {
                            _build.Steps[count].path = EditorUtility.SaveFolderPanel("Overwrite End Path", _build.Steps[count].path, "Build");
                        }

                        if (GUILayout.Button("Add...", GUILayout.MaxWidth(50)))
                        {
                            GUI.FocusControl(null);

                            BuildEditorSettings.Step _step = _build.Steps[count];

                            GenericMenu menu = this.GetTagsMenu((_object) =>
                                                                {
                                                                    string tag = _object as string;

                                                                    _step.path += "{" + tag + "}";
                                                                });

                            menu.ShowAsContext();
                        }

                        GUILayout.EndHorizontal();

                        EditorGUI.EndDisabledGroup();

                        UIUtility.Space();
                        _build.Steps[count].overwriteScenes = EditorGUILayout.Toggle("Custom Scenes?", _build.Steps[count].overwriteScenes);

                        EditorGUI.BeginDisabledGroup(!_build.Steps[count].overwriteScenes);

                        if (!this.StepsLists[count].ContainsKey("SCENE"))
                        {
                            //tempList = this.StepsLists[count]["SCENE"];
                            this.SetupSceneList("Scenes", _build, _build.Steps[count].scenes, ref tempList);

                            this.StepsLists[count]["SCENE"] = tempList;
                        }

                        if (!this.StepsLists[count].ContainsKey("PRE_BUILD"))
                        {
                            //tempList = this.StepsLists[count]["SCENE"];
                            this.SetupActionList("Pre Step Build", "PreBuild/Step/", _build, _build.Steps[count].preBuildActions, ref tempList);

                            this.StepsLists[count]["PRE_BUILD"] = tempList;
                        }

                        if (!this.StepsLists[count].ContainsKey("POST_BUILD"))
                        {
                            //tempList = this.StepsLists[count]["SCENE"];
                            this.SetupActionList("Post Step Build", "PostBuild/Step/", _build, _build.Steps[count].postBuildActions, ref tempList);

                            this.StepsLists[count]["POST_BUILD"] = tempList;
                        }

                        this.StepsLists[count]["SCENE"].DoLayoutList();
                        UIUtility.Space();

                        EditorGUI.EndDisabledGroup();

                        this.StepsLists[count]["PRE_BUILD"].DoLayoutList();
                        UIUtility.Space();

                        this.StepsLists[count]["POST_BUILD"].DoLayoutList();
                        UIUtility.Space();
                    }

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Delete", _settings.style.WelcomeButtons))
                    {
                        if (!EditorUtility.DisplayDialog("Delete Step?",
                                                        "Do you want to delete the step " + _build.Steps[count].Name + "?",
                                                        "Cancel",
                                                        "Delete"))
                        {
                            _build.Steps.RemoveAt(count);
                        }
                    }

                    if (GUILayout.Button("Build", _settings.style.WelcomeButtons))
                    {
                        _editor.nextState = States.Welcome;
                    }

                    GUILayout.EndHorizontal();

                    UIUtility.HR();

                    GUILayout.EndVertical();

                    UIUtility.Space();
                }

                EditorGUILayout.EndScrollView();

                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Back", _settings.style.WelcomeButtons))
                {
                    _editor.nextState = States.Welcome;
                }

                UIUtility.Space();

                if (GUILayout.Button("Add Step", _settings.style.WelcomeButtons))
                {
                    BuildEditorSettings.Step step = new BuildEditorSettings.Step();
                    step.Name = "New Step";

                    int count = _build.Steps.FindAll(x => x.Name.StartsWith("New Step")).Count;

                    if(count > 0)
                    {
                        step.Name += " " + count;
                    }

                    _build.Steps.Add(step);
                }

                UIUtility.Space();

                if (GUILayout.Button("Build All", _settings.style.WelcomeButtons))
                {
                    BuildScript.BuildAll(_build);
                }

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                if (check.changed)
                {
                    _build.UpdateStepTags();
                }
            }
        }

        void CreateEditLists(BuildEditorSettings _build)
        {
            if (this.actionsPreBuild == null)
            {
                this.SetupActionList("Pre Build Actions", "PreBuild", _build, _build.preBuildActions, ref this.actionsPreBuild);
            }

            if (this.actionsPostBuild == null)
            {
                this.SetupActionList("Post Build Actions", "PostBuild", _build, _build.postBuildActions, ref this.actionsPostBuild);
            }
        }
        #endregion

        #region WizardPath
        public void InitNewPath(BuildEditorSettings _build, BuildScriptWindowEditor _editor)
        {
            this.newCurrStep = 0;
        }

        public void NewPath(BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings)
        {
            GUILayout.BeginVertical();

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Wizard: Path", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            UIUtility.Space();

            EditorGUILayout.BeginVertical(_settings.style.WelcomeGroup);

            GUILayout.Label("Choose the final folder for the build");

            EditorGUILayout.BeginHorizontal();

            GUI.SetNextControlName("PATH_FilePath");
            _settings.tempSettings.Path = EditorGUILayout.TextField(_settings.tempSettings.Path);

            if(GUILayout.Button("...", GUILayout.MaxWidth(30)))
            {
                _settings.tempSettings.Path = EditorUtility.SaveFolderPanel("Build End Path", _settings.tempSettings.Path, "Build");
            }

            EditorGUILayout.EndHorizontal();

            UIUtility.HR();

            GUILayout.Label("Choose the project name");

            GUI.SetNextControlName("FILE_FileName");
            _settings.tempSettings.File = EditorGUILayout.TextField(_settings.tempSettings.File);

            EditorGUILayout.EndVertical();

            this.DrawSmartArea(_editor, GUI.GetNameOfFocusedControl(), _settings);

            GUILayout.FlexibleSpace();

            this.DrawWizardButtons(_build, _editor, _settings, true, !string.IsNullOrEmpty(_settings.tempSettings.Path) &&  
                                                                     !string.IsNullOrEmpty(_settings.tempSettings.File));

            GUILayout.EndVertical();
        }
        #endregion

        #region WizardProject
        public void InitNewProject(BuildEditorSettings _build, BuildScriptWindowEditor _editor)
        {
        }


        public void NewProject(BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings)
        {
            GUILayout.BeginVertical();

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Wizard: Project", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            UIUtility.Space();

            EditorGUILayout.BeginVertical(_settings.style.WelcomeGroup);

            _settings.tempSettings.controlVersion = EditorGUILayout.Toggle("Control Project Version", _settings.tempSettings.controlVersion);

            EditorGUI.BeginDisabledGroup(!_settings.tempSettings.controlVersion);
            UIUtility.Space();

            GUI.SetNextControlName("VERSION_Version Pattern");
            _settings.tempSettings.Version = EditorGUILayout.TextField("Version Pattern", _settings.tempSettings.Version);

            GUI.SetNextControlName("NONE_Version Sequence");
            _settings.tempSettings.Sequence = EditorGUILayout.IntField("Version Sequence", _settings.tempSettings.Sequence);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();

            this.DrawSmartArea(_editor, GUI.GetNameOfFocusedControl(), _settings);

            GUILayout.FlexibleSpace();

            this.DrawWizardButtons(_build, _editor, _settings);

            GUILayout.EndVertical();
        }
        #endregion

        #region WizardProjectActions
        public void InitNewActions(BuildEditorSettings _build, BuildScriptWindowEditor _editor)
        {
        }


        public void NewActions(BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings)
        {
            GUILayout.BeginVertical();

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Wizard: Extra Actions", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            UIUtility.Space();

            EditorGUILayout.BeginVertical(_settings.style.WelcomeGroup);

            this.CreateWizardLists("Pre Build Actions", "Post Build Actions", _settings.tempSettings, ref this.WZactionsPreBuild, ref this.WZactionsPostBuild);

            this.WZactionsPreBuild.DoLayoutList();
            UIUtility.Space();
            this.WZactionsPostBuild.DoLayoutList();

            EditorGUILayout.EndVertical();

            this.DrawSmartArea(_editor, GUI.GetNameOfFocusedControl(), _settings);

            GUILayout.FlexibleSpace();

            this.DrawWizardButtons(_build, _editor, _settings);

            GUILayout.EndVertical();
        }
        #endregion

        #region WizardPlatform
        public void InitNewPlatform(BuildEditorSettings _build, BuildScriptWindowEditor _editor)
        {

        }

        public void NewPlatform(BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings)
        {
            GUILayout.BeginVertical();

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Wizard: Choose your Target Platform", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            UIUtility.Space();

            var values = System.Enum.GetValues(typeof(BuildTarget));
            System.Array.Sort(values, new StringComparer());

            int numRow = Mathf.RoundToInt(_editor.position.width / 150f);

            bool endGroup = true;
            bool startGroup = false;

            int count = 1;

            this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, _settings.style.WelcomeGroup, GUILayout.MaxHeight(_editor.position.height * 0.4f));

            GUILayout.Label("Platforms, click to add:");
            foreach (var value in values)
            {

                switch (value.ToString())
                {
                    case "iPhone":
                    case "StandaloneOSXIntel":
                    case "WebPlayer":
                    case "StandaloneOSXIntel64":
                    case "NoTarget":
                    case "BB10":
                    case "MetroPlayer":
                        continue;
                }

                if (endGroup)
                {
                    GUILayout.BeginHorizontal();
                    startGroup = true;
                    endGroup = false;
                }

                string name = value.ToString().Replace("Standalone", "");

                if (GUILayout.Button(name, GUILayout.MaxWidth(150f)))
                {
                    BuildEditorSettings.Step step = new BuildEditorSettings.Step();
                    System.Enum.TryParse<BuildTarget>(value.ToString(), out step.Target);

                    step.Name = step.Target.ToString();

                    _settings.tempSettings.Steps.Add(step);
                    _settings.tempSettings.UpdateStepTags();
                    //GUI.FocusControl("SMART_TagBtn_" + count);
                }

                if (count > 0 && count % numRow == 0)
                {
                    GUILayout.EndHorizontal();
                    endGroup = true;
                    startGroup = false;
                }

                count++;
                //Debug.Log(value.ToString());
            }

            if (startGroup && !endGroup)
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            UIUtility.HR();

            GUILayout.Label("In Project, click to remove:");

            numRow = Mathf.RoundToInt(_editor.position.width / 150f);

            endGroup = true;
            startGroup = false;

            count = 1;

            this.scrollPosition2 = GUILayout.BeginScrollView(this.scrollPosition2, _settings.style.WelcomeGroup, GUILayout.MaxHeight(_editor.position.height * 0.4f));

            for (int countS = 0; countS < _settings.tempSettings.Steps.Count; countS++)
            {
                if (endGroup)
                {
                    GUILayout.BeginHorizontal();
                    startGroup = true;
                    endGroup = false;
                }

                if (GUILayout.Button(_settings.tempSettings.Steps[countS].Target.ToString(), GUILayout.MaxWidth(150f)))
                {
                    _settings.tempSettings.Steps.RemoveAt(countS);
                    //GUI.FocusControl("SMART_TagBtn_" + count);
                }

                if (count > 0 && count % numRow == 0)
                {
                    GUILayout.EndHorizontal();
                    endGroup = true;
                    startGroup = false;
                }

                count++;
            }

            if (startGroup && !endGroup)
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            //BuildScript.

            GUILayout.FlexibleSpace();

            this.DrawWizardButtons(_build, _editor, _settings, true, _settings.tempSettings.Steps.Count > 0);

            GUILayout.EndVertical();
        }
        #endregion

        #region WizardNewStep
        public void InitNewStep(BuildEditorSettings _build, BuildScriptWindowEditor _editor)
        {

        }

        public void NewStep(BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                GUILayout.BeginVertical();

                UIUtility.BeginCenterGroup();

                string nameSettings = _settings.tempSettings.Steps[this.newCurrStep].Name;

                if(string.IsNullOrEmpty(nameSettings))
                {
                    nameSettings = _settings.tempSettings.Steps[this.newCurrStep].Target.ToString();
                }

                GUILayout.Label("Wizard: Platform Settings - "+ nameSettings, EditorStyles.boldLabel);
                UIUtility.EndCenterGroup();

                UIUtility.Space();

                if (GUI.GetNameOfFocusedControl().Contains("PATH"))
                {
                    this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, _settings.style.WelcomeGroup, GUILayout.MaxHeight(_editor.position.height * 0.4f));
                }
                else
                {
                    this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, _settings.style.WelcomeGroup);
                }

                _settings.tempSettings.Steps[this.newCurrStep].Name = EditorGUILayout.TextField("Name", _settings.tempSettings.Steps[this.newCurrStep].Name);
                _settings.tempSettings.Steps[this.newCurrStep].Labels = EditorGUILayout.TextField("Label", _settings.tempSettings.Steps[this.newCurrStep].Labels);

                GUILayout.BeginHorizontal();
                _settings.tempSettings.Steps[this.newCurrStep].overwriteStep = (TemplateBuildAction)EditorGUILayout.ObjectField("Overwrite Step", _settings.tempSettings.Steps[this.newCurrStep].overwriteStep, typeof(TemplateBuildAction), false);

                if (GUILayout.Button("Overwrite...", GUILayout.MaxWidth(80)))
                {
                    GenericMenu menu = this.GetActionsMenu("Overwrite", _build, _settings.tempSettings.Steps[this.newCurrStep]);
                    menu.ShowAsContext();
                }

                GUILayout.EndHorizontal();

                if (_settings.tempSettings.Steps[this.newCurrStep].overwriteStep == null)
                {
                    UIUtility.Space();
                    _settings.tempSettings.Steps[this.newCurrStep].Target = (BuildTarget)EditorGUILayout.EnumPopup("Target Platform", _settings.tempSettings.Steps[this.newCurrStep].Target);
                    _settings.tempSettings.Steps[this.newCurrStep].Option = (BuildOptions)EditorGUILayout.EnumFlagsField("Options", _settings.tempSettings.Steps[this.newCurrStep].Option);

                    UIUtility.Space();
                    _settings.tempSettings.Steps[this.newCurrStep].mainBuild = EditorGUILayout.Toggle("Main Build?", _settings.tempSettings.Steps[this.newCurrStep].mainBuild);
                    _settings.tempSettings.Steps[this.newCurrStep].zipBuild = EditorGUILayout.Toggle("Zip Build?", _settings.tempSettings.Steps[this.newCurrStep].zipBuild);
                    _settings.tempSettings.Steps[this.newCurrStep].overwritePath = EditorGUILayout.Toggle("Custom Path?", _settings.tempSettings.Steps[this.newCurrStep].overwritePath);

                    EditorGUI.BeginDisabledGroup(!_settings.tempSettings.Steps[this.newCurrStep].overwritePath);

                    GUILayout.BeginHorizontal();

                    GUI.SetNextControlName("PATH_StepFilePath");
                    _settings.tempSettings.Steps[this.newCurrStep].path = EditorGUILayout.TextField("Path", _settings.tempSettings.Steps[this.newCurrStep].path);

                    if (GUILayout.Button("...", GUILayout.MaxWidth(50)))
                    {
                        _settings.tempSettings.Steps[this.newCurrStep].path = EditorUtility.SaveFolderPanel("Overwrite End Path", _settings.tempSettings.Steps[this.newCurrStep].path, "Build");
                    }

                    if (GUILayout.Button("Add...", GUILayout.MaxWidth(50)))
                    {
                        GUI.FocusControl(null);

                        BuildEditorSettings.Step _step = _settings.tempSettings.Steps[this.newCurrStep];

                        GenericMenu menu = this.GetTagsMenu((_object) =>
                        {
                            string tag = _object as string;

                            _step.path += "{" + tag + "}";
                        });

                        menu.ShowAsContext();
                    }

                    GUILayout.EndHorizontal();

                    EditorGUI.EndDisabledGroup();

                    UIUtility.Space();
                    _settings.tempSettings.Steps[this.newCurrStep].overwriteScenes = EditorGUILayout.Toggle("Custom Scenes?", _settings.tempSettings.Steps[this.newCurrStep].overwriteScenes);

                    EditorGUI.BeginDisabledGroup(!_settings.tempSettings.Steps[this.newCurrStep].overwriteScenes);

                    if (this.WZScene == null)
                    {
                        this.SetupSceneList("Scenes", _settings.tempSettings, _settings.tempSettings.Steps[this.newCurrStep].scenes, ref this.WZScene);
                    }

                    this.WZScene.DoLayoutList();
                    UIUtility.Space();

                    EditorGUI.EndDisabledGroup();

                    if (this.WZactionsStepPreBuild == null)
                    {
                        this.SetupActionList("Pre Step Build", "PreBuild/Step/", _settings.tempSettings, _settings.tempSettings.Steps[this.newCurrStep].preBuildActions, ref this.WZactionsStepPreBuild);
                    }

                    if (this.WZactionsStepPostBuild == null)
                    {
                        this.SetupActionList("Post Step Build", "PostBuild/Step/", _settings.tempSettings, _settings.tempSettings.Steps[this.newCurrStep].postBuildActions, ref this.WZactionsStepPostBuild);
                    }

                    this.WZactionsStepPreBuild.DoLayoutList();
                    UIUtility.Space();

                    this.WZactionsStepPostBuild.DoLayoutList();
                    UIUtility.Space();
                }

                GUILayout.EndScrollView();

                if (GUI.GetNameOfFocusedControl().Contains("PATH"))
                {
                    this.DrawSmartArea(_editor, GUI.GetNameOfFocusedControl(), _settings);
                }

                GUILayout.FlexibleSpace();

                this.DrawWizardButtons(_build, _editor, _settings);

                GUILayout.EndVertical();

                if (check.changed)
                {
                    _settings.tempSettings.UpdateStepTags();
                }
            }
        }
        #endregion

        #region WizardNewEnd
        public void InitNewEnd(BuildEditorSettings _build, BuildScriptWindowEditor _editor)
        {

        }

        public void NewEnd(BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings)
        {
            GUILayout.BeginVertical();

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Wizard: Final File", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            UIUtility.Space();

            EditorGUILayout.BeginVertical(_settings.style.WelcomeGroup);

            GUILayout.Label("Now, choose where this seetings will be saved");

            EditorGUILayout.BeginHorizontal();

            this.saveTempFilePath = EditorGUILayout.TextField(this.saveTempFilePath);

            if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
            {
                this.saveTempFilePath = EditorUtility.SaveFilePanelInProject("Settings File", "BuildSettings", "asset", "Final Build Settings", this.saveTempFilePath);
                GUI.FocusControl("NONE_BackBtn");
            }

            EditorGUILayout.EndHorizontal();

            UIUtility.Space();

            EditorGUILayout.Toggle("Default Build Settings?", false);

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if(this.DrawWizardButtons(_build, _editor, _settings, true, !string.IsNullOrEmpty(this.saveTempFilePath)))
            {
                if (System.IO.File.Exists(this.saveTempFilePath))
                {
                    AssetDatabase.DeleteAsset(this.saveTempFilePath);
                }

                BuildEditorSettings script = Object.Instantiate(_settings.tempSettings);
                AssetDatabase.CreateAsset(script, this.saveTempFilePath);
                AssetDatabase.SaveAssets();

                _editor.currSettings = script;

                var directory = new System.IO.DirectoryInfo(this.saveTempFilePath).Parent;

                string[] newPath = directory.FullName.Split(new string[] { "Assets" }, System.StringSplitOptions.None);

                if (_settings.tempSettings.preBuildActions.Count > 0)
                {
                    Debug.Log("<color=red>Path == </color>"+newPath[1] + "/" + script.name + "/Actions/PreBuild/");
                    this.NewMoveAssets("Assets/"+newPath[1] + "/" + script.name + "/Actions/PreBuild/", _settings.tempSettings.preBuildActions);
                }

                if (_settings.tempSettings.postBuildActions.Count > 0)
                {
                    this.NewMoveAssets("Assets/" + newPath[1] + "/" + script.name + "/Actions/PostBuild/", _settings.tempSettings.postBuildActions);
                }

                if (_settings.tempSettings.Steps.Count > 0)
                {
                    for(int count = 0; count < _settings.tempSettings.Steps.Count; count++)
                    {
                        this.NewMoveAssets("Assets/" + newPath[1] + "/" + script.name + "/Actions/PreBuild/Steps/"+ _settings.tempSettings.Steps[count].Name+"/",
                                           _settings.tempSettings.Steps[count].preBuildActions);

                        this.NewMoveAssets("Assets/" + newPath[1] + "/" + script.name + "/Actions/PostBuild/Steps/" + _settings.tempSettings.Steps[count].Name + "/",
                                           _settings.tempSettings.Steps[count].postBuildActions);
                    }
                }

                //EditorUtility.FocusProjectWindow();
                Selection.activeObject = script;

                UIUtility.Alert("File Created!", "Your build settings was created!");

                _editor.nextState = States.ChangeSettings;
            }

            GUILayout.EndVertical();
        }

        public void NewMoveAssets(string _newPath, List<TemplateBuildAction> _actions)
        {
            if(_actions.Count <= 0)
            {
                return;
            }

            Directory.CreateDirectory(_newPath);

            AssetDatabase.Refresh();

            for (int count = 0; count < _actions.Count; count++)
            {
                string[] oldPath = AssetDatabase.GetAssetPath(_actions[count]).Split(new string[] { "Assets" }, System.StringSplitOptions.None);

                string error = AssetDatabase.MoveAsset("Assets" + oldPath[1],
                                                       _newPath + _actions[count].name + ".asset");

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.Log("Error: " + error);
                }
            }
        }
        #endregion

        #region WizardMisc
        public bool DrawWizardButtons(BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings, bool _enableBack = true, bool _enableNext = true)
        {
            GUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(!_enableBack);

            GUI.SetNextControlName("NONE_BackBtn");
            if (_editor.currState == States.NewPath)
            {
                if (GUILayout.Button("Cancel", _settings.style.WelcomeButtons))
                {
                    GUI.FocusControl("NONE_BackBtn");
                    _editor.nextState = States.Welcome;
                    _settings.tempSettings.Clear();
                }
            }
            else
            {
                if (GUILayout.Button("Back", _settings.style.WelcomeButtons))
                {
                    this.lastSmartType = "NONE";
                    GUI.FocusControl("NONE_BackBtn");

                    switch (_editor.currState)
                    {
                        case States.NewProject:
                            _editor.nextState = States.NewPath;
                            break;
                        case States.NewActions:
                            _editor.nextState = States.NewProject;
                            break;
                        case States.NewPlatform:
                            _editor.nextState = States.NewActions;
                            break;
                        case States.NewStep:
                            if (this.newCurrStep - 1 >= 0)
                            {
                                this.newCurrStep--;
                                this.WZScene = null;
                                this.WZactionsStepPreBuild = null;
                                this.WZactionsStepPreBuild = null;
                            }
                            else
                            {
                                _editor.nextState = States.NewPlatform;
                            }

                            break;
                        case States.NewEnd:
                            _editor.nextState = States.NewStep;
                            this.newCurrStep = _settings.tempSettings.Steps.Count - 1;
                            break;
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            UIUtility.Space(3);

            EditorGUI.BeginDisabledGroup(!_enableNext);

            GUI.SetNextControlName("NONE_NextBtn");
            if (_editor.currState == States.NewEnd)
            {
                if (GUILayout.Button("Finish", _settings.style.WelcomeButtons))
                {
                    return true;
                }
            }
            else
            {
                if (GUILayout.Button("Next", _settings.style.WelcomeButtons))
                {
                    GUI.FocusControl("NONE_NextBtn");
                    this.lastSmartType = "NONE";

                    switch (_editor.currState)
                    {
                        case States.NewPath:
                            _editor.nextState = States.NewProject;
                            break;
                        case States.NewProject:
                            _editor.nextState = States.NewActions;
                            break;
                        case States.NewActions:
                            _editor.nextState = States.NewPlatform;
                            break;
                        case States.NewPlatform:
                            _editor.nextState = States.NewStep;
                            this.newCurrStep = 0;
                            break;
                        case States.NewStep:
                            if (_settings.tempSettings.Steps.Count > this.newCurrStep + 1)
                            {
                                this.newCurrStep++;
                                this.WZScene = null;
                                this.WZactionsStepPreBuild = null;
                                this.WZactionsStepPreBuild = null;
                            }
                            else
                            {
                                _editor.nextState = States.NewEnd;
                            }

                            break;
                        case States.NewEnd:
                            return true;
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

            return false;
        }

        public void DrawSmartArea(BuildScriptWindowEditor _editor, string _target, BuildScriptWindowSettings _settings)
        {
            if(string.IsNullOrWhiteSpace(_target))
            {
                _target = string.Empty;
            }

            string type = _target.Split('_')[0];

            switch (type)
            {
                case "FILE":

                    UIUtility.Space();
                    this.smartAreaScrollPos = GUILayout.BeginScrollView(this.smartAreaScrollPos);

                    GUILayout.Label("Tags", EditorStyles.boldLabel);
                    this.DrawSATags(_editor, SmartAreaType.File, _target, ref _settings.tempSettings.File);
                    this.lastSmartType = _target;

                    GUILayout.EndScrollView();
                    break;
                case "PATH":

                    UIUtility.Space();
                    this.smartAreaScrollPos = GUILayout.BeginScrollView(this.smartAreaScrollPos);

                    GUILayout.Label("Tags", EditorStyles.boldLabel);

                    if (_target.Contains("Step"))
                    {
                        this.DrawSATags(_editor, SmartAreaType.Path, _target, ref _settings.tempSettings.Steps[this.newCurrStep].path);
                    }
                    else
                    {
                        this.DrawSATags(_editor, SmartAreaType.Path, _target, ref _settings.tempSettings.Path);
                    }

                    this.lastSmartType = _target;

                    GUILayout.EndScrollView();
                    break;
                case "VERSION":
                    this.smartAreaScrollPos = GUILayout.BeginScrollView(this.smartAreaScrollPos);

                    GUILayout.Label("Tags", EditorStyles.boldLabel);
                    this.DrawSATags(_editor, SmartAreaType.Version, _target, ref _settings.tempSettings.Version);
                    this.lastSmartType = _target;

                    GUILayout.EndScrollView();
                    break;
                case "NONE":
                    break;
                case "SMART":
                default:
					if(!string.IsNullOrEmpty(this.lastSmartType))
					{
						this.DrawSmartArea(_editor, this.lastSmartType, _settings);
					}
                    break;
            }
        }

        public void DrawSATags(BuildScriptWindowEditor _editor, SmartAreaType _type, string _target, ref string _field)
        {

            bool startGroup = false;
            bool endGroup = true;

            Dictionary<string, string> dicionary = BuildScript.GetDictionary();
            int count = 1;

            int numRow = Mathf.RoundToInt(_editor.position.width / 150f);


            foreach (var tags in dicionary)
            {

                if (endGroup)
                {
                    GUILayout.BeginHorizontal();
                    startGroup = true;
                    endGroup = false;
                }

                string[] tag = tags.Value.Split('/');

                switch (_type)
                {
                    case SmartAreaType.File:
                        if(tag[0].Contains("Folder"))
                        {
                            continue;
                        }

                        break;
                    case SmartAreaType.Path:
                        if (tag[1].Contains("Extension"))
                        {
                            continue;
                        }
                        break;
                    case SmartAreaType.Version:
                        if (tag[0].Contains("Folder") || tag[0].Contains("Platform"))
                        {
                            continue;
                        }
                        break;
                }


                GUI.SetNextControlName("SMART_TagBtn_" + count);
                if (GUILayout.Button(tag[1], GUILayout.MaxWidth(150f)))
                {
                    GUI.FocusControl("SMART_TagBtn_" + count);
                    _field += "{"+tags.Key+"}";
                }

                if (count > 0 && count % numRow == 0)
                {
                    GUILayout.EndHorizontal();
                    endGroup = true;
                    startGroup = false;
                }

                count++;
            }

            /*for (int count = 1; count < 20; count++)
            {

                if (endGroup)
                {
                    GUILayout.BeginHorizontal();
                    startGroup = true;
                    endGroup = false;
                }

                GUI.SetNextControlName("SMART_TagBtn_"+count);
                GUILayout.Button("Path");

                if (count > 0 && count % 3 == 0)
                {
                    GUILayout.EndHorizontal();
                    endGroup = true;
                    startGroup = false;
                }
            }*/

            if (startGroup && !endGroup)
            {
                GUILayout.EndHorizontal();
            }
        }

        void CreateWizardLists(string _preTitle, string _postTitle, BuildEditorSettings _build, ref ReorderableList _preBuild, ref ReorderableList _postBuild)
        {
            if (_preBuild == null)
            {
                this.SetupActionList(_preTitle, "PreBuild", _build, _build.preBuildActions, ref _preBuild);
            }

            if (_postBuild == null)
            {
                this.SetupActionList(_postTitle, "PostBuild", _build, _build.postBuildActions, ref _postBuild);
            }
        }
        #endregion

        #region List
        public void SetupActionList(string _title, string _folder, BuildEditorSettings _build, List<TemplateBuildAction> _actions, ref ReorderableList _list)
        {
            _list = new ReorderableList(_actions, typeof(TemplateBuildAction), true, true, true, true);

            _list.drawHeaderCallback = (Rect _rect) =>
            {
                EditorGUI.LabelField(_rect, _title);
            };

            _list.drawElementCallback = (Rect _rect, int _index, bool _isActive, bool _isFocused) =>
            {
                _actions[_index] = (TemplateBuildAction)EditorGUI.ObjectField(new Rect(_rect.x + (_rect.width * .01f), _rect.y, _rect.width * .98f, EditorGUIUtility.singleLineHeight), _actions[_index], typeof(TemplateBuildAction), false);
            };

            _list.onAddCallback = (ReorderableList list) =>
            {
                GenericMenu menu = this.GetActionsMenu(_folder, _build, _actions);
                menu.ShowAsContext();
            };

            _list.onRemoveCallback = (ReorderableList list) =>
            {
                this.DeleteAction(list.index, _actions);
            };

            _list.onSelectCallback = (ReorderableList list) =>
            {
                UIUtility.SetInspectorFile(_actions[list.index]);
            };
        }

        public void OnAddAction(object _action)
        {
            MenuOption option = _action as MenuOption;

            string path = AssetDatabase.GetAssetPath(option.BuildSetting);
            string folder = Path.GetDirectoryName(path) + "/"+ option.BuildSetting.name + "/Actions/"+option.Folder+"/";

            Debug.Log(folder);

            Directory.CreateDirectory(folder);

            bool fileCreated = false;

            string baseFilePath = folder + option.Script.GetName();
            string currfilePath = baseFilePath + ".asset";

            int count = 1;

            ScriptableObject script = null;

            do
            {
                if (!File.Exists(currfilePath))
                {
                    script = ScriptableObject.CreateInstance(option.Script.GetType());
                    AssetDatabase.CreateAsset(script, currfilePath);
                    AssetDatabase.SaveAssets();
                    //EditorUtility.FocusProjectWindow();
                    Selection.activeObject = script;

                    fileCreated = true;
                }
                else
                {
                    currfilePath = baseFilePath+" ("+count+").asset";
                    count++;
                }

            } while (!fileCreated);

            if (option.Step != null)
            {
                option.Step.overwriteStep = script as TemplateBuildAction;
            }
            else
            {
                option.List.Add(script as TemplateBuildAction);
            }
        }

        public void DeleteAction(int _Index, List<TemplateBuildAction> _actions)
        {
            if(_actions[_Index] != null && !AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_actions[_Index])))
            {
                Debug.LogError("Error on Deleting "+ _actions[_Index]);
                return;
            }

            _actions.RemoveAt(_Index);
            AssetDatabase.SaveAssets();
        }

        public void SetupSceneList(string _title, BuildEditorSettings _build, List<Object> _scene, ref ReorderableList _list)
        {
            _list = new ReorderableList(_scene, typeof(Object), true, true, true, true);

            _list.drawHeaderCallback = (Rect _rect) =>
            {
                EditorGUI.LabelField(_rect, _title);
            };

            _list.drawElementCallback = (Rect _rect, int _index, bool _isActive, bool _isFocused) =>
            {
                _scene[_index] = (Object)EditorGUI.ObjectField(new Rect(_rect.x + (_rect.width * .01f), _rect.y, _rect.width * .98f, EditorGUIUtility.singleLineHeight), _scene[_index], typeof(Object), false);
            };

            _list.onAddCallback = (ReorderableList list) =>
            {
                _scene.Add(null);
            };

            _list.onRemoveCallback = (ReorderableList list) =>
            {
                _scene.RemoveAt(list.index);
            };
        }
        #endregion

        #region Misc
        public GenericMenu GetActionsMenu(string _folder, BuildEditorSettings _build, List<TemplateBuildAction> _targetList)
        {
            var classes = UIUtility.GetEnumerableOfType<TemplateBuildAction>();

            GenericMenu menu = new GenericMenu();

            foreach (var temp in classes)
            {
                menu.AddItem(new GUIContent(temp.GetName()), true, this.OnAddAction, new MenuOption(_folder, _build, temp, _targetList));
            }

            return menu;
        }

        public GenericMenu GetActionsMenu(string _folder, BuildEditorSettings _build, BuildEditorSettings.Step _step)
        {
            var classes = UIUtility.GetEnumerableOfType<TemplateBuildAction>();

            GenericMenu menu = new GenericMenu();

            foreach (var temp in classes)
            {
                menu.AddItem(new GUIContent(temp.GetName()), true, this.OnAddAction, new MenuOption(_folder, _build, temp, _step));
            }

            return menu;
        }

        public GenericMenu GetTagsMenu(GenericMenu.MenuFunction2 _action)
        {
            var classes = UIUtility.GetEnumerableOfType<TemplateBuildAction>();

            GenericMenu menu = new GenericMenu();

            Dictionary<string, string> tags = BuildScript.GetDictionary();

            foreach (var tag in tags)
            {
                menu.AddItem(new GUIContent(tag.Value), false, _action, tag.Key);
            }

            return menu;
        }
        #endregion
    }
}