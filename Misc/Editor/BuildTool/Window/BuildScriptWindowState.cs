﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

namespace Falcone.BuildTool
{
    [System.Serializable]
    public class BuildScriptWindowState
    {
        [System.Serializable]
        public class MenuOption
        {
            public string Folder;
            public BuildEditorSettings BuildSetting;
            public TemplateBuildAction Script;
            public List<TemplateBuildAction> List;

            public MenuOption(string _Folder, BuildEditorSettings _build, TemplateBuildAction _Script, List<TemplateBuildAction> _list)
            {
                Folder = _Folder;
                BuildSetting = _build;
                Script = _Script;
                List = _list;
            }
        }

        public enum States
        {
            Welcome,

            NewPlatform,
            NewSettings,
            NewActions,

            BuildStep,

            ChangeSettings
        }

        Vector2 scrollPosition;

        ReorderableList actionsPreBuild;
        ReorderableList actionsPostBuild;

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
            }
        }

        public void Tick(States _state, BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings)
        {
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
            }
        }

        #region Welcome
        public void InitWelcome(BuildEditorSettings _build, BuildScriptWindowEditor _editor)
        {

        }

        public void Welcome(BuildEditorSettings _build, BuildScriptWindowEditor _editor, BuildScriptWindowSettings _settings)
        {
            EditorStyles.label.wordWrap = true;

            _settings.style.WelcomeGroup = new GUIStyle(EditorStyles.helpBox);
            _settings.style.WelcomeBuildButton = new GUIStyle(GUI.skin.button);
            _settings.style.WelcomeButtons = new GUIStyle(GUI.skin.button);

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

            GUILayoutOption[] options = { GUILayout.MinHeight(100), GUILayout.MinWidth(minWidth) };

            GUILayout.BeginVertical(_settings.style.WelcomeGroup, options);

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Wizard", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            EditorGUILayout.LabelField("Start a new Build file using our wizard!");
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create a new File", _settings.style.WelcomeButtons))
            {

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

            GUILayout.BeginVertical();

            UIUtility.Space();

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Edit Settings", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            UIUtility.Space();

            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, _settings.style.WelcomeGroup);

            UIUtility.SubTitle("Version");

            _build.controlVersion = EditorGUILayout.Toggle("Control Project Version", _build.controlVersion);
            UIUtility.Space();

            EditorGUI.BeginDisabledGroup(!_build.controlVersion);

            GUILayout.BeginHorizontal();
            _build.Version = EditorGUILayout.TextField("Version Template", _build.Version);
            GUILayout.Button("Add...", GUILayout.MaxWidth(50));
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            EditorGUILayout.TextField("                ", "Ex.: "+BuildScript.ParseString(_build.Version), GUI.skin.label);
            GUILayout.EndHorizontal();

            EditorGUILayout.IntField("Build Sequence", _build.sequence);

            EditorGUI.EndDisabledGroup();

            UIUtility.Space();

            UIUtility.SubTitle("Build");


            GUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Destine Path", _build.Path);
            GUILayout.Button("...", GUILayout.MaxWidth(50));
            GUILayout.Button("Add...", GUILayout.MaxWidth(50));
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            EditorGUILayout.TextField("File Name", _build.File);
            GUILayout.Button("Add...", GUILayout.MaxWidth(50));
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

                GUILayout.BeginVertical();

                UIUtility.BeginCenterGroup();
                GUILayout.Label(_build.Steps[count].Name, EditorStyles.boldLabel);
                UIUtility.EndCenterGroup();

                _build.Steps[count].Name = EditorGUILayout.TextField("Name", _build.Steps[count].Name);
                _build.Steps[count].Labels = EditorGUILayout.TextField("Label", _build.Steps[count].Labels);

                GUILayout.BeginHorizontal();
                _build.Steps[count].overwriteStep = (TemplateBuildAction)EditorGUILayout.ObjectField("Overwrite Step", _build.Steps[count].overwriteStep, typeof(TemplateBuildAction), false);
                GUILayout.Button("Overwrite...", GUILayout.MaxWidth(80));
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
                    GUILayout.Button("...", GUILayout.MaxWidth(50));
                    GUILayout.Button("Add...", GUILayout.MaxWidth(50));
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
                _editor.nextState = States.Welcome;
            }

            UIUtility.Space();

            if (GUILayout.Button("Build All", _settings.style.WelcomeButtons))
            {
                _editor.nextState = States.Welcome;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
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
                var classes = UIUtility.GetEnumerableOfType<TemplateBuildAction>();

                GenericMenu menu = new GenericMenu();

                foreach (var temp in classes)
                {
                    menu.AddItem(new GUIContent(temp.GetName()), true, this.OnAddAction, new MenuOption(_folder, _build, temp, _actions));
                }

                menu.ShowAsContext();
                //EditorGUILayout.Pop
                //_actions.Add(null);
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
            string folder = Path.GetDirectoryName(path) + "/Actions/"+option.Folder+"/";

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

            option.List.Add(script as TemplateBuildAction);
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
    }
}