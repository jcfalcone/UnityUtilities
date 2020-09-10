using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text;
using UnityEditor.PackageManager;

namespace Falcone.BuildTool
{
    public class BuildScript
    {
        #region Declares
        public class ExtraSettings
        {
            public List<string> targets = new List<string>();
            public List<string> tags = new List<string>();
            public bool forceDev;
            public bool noAlerts;
            public bool mainBuild = true;

            public int forceSequence = -1;

            public static ExtraSettings Dev()
            {
                ExtraSettings extra = new ExtraSettings();
                extra.forceDev = true;

                return extra;
            }

            public static ExtraSettings Default()
            {
                return new ExtraSettings();
            }
        }

        public delegate void OnStep();
        #endregion

        #region Events
        public static OnStep onStep;
        public static OnStep onComplete;
        #endregion

        #region Progress
        public static int totalParts = 0;
        public static int currParts = 0;

        public static float Progress
        {
            get
            {
                if(totalParts == 0)
                {
                    return 0f;
                }

                return Mathf.Clamp01(currParts / totalParts);
            }
        }

        public static bool isBuilding = false;
        public static string currStep = "";
        #endregion

        #region Settings
        static Dictionary<string, string> dictionary = new Dictionary<string, string>();

        static int tempSequence = -1;
        #endregion

        #region Premade Builds
        #region IOS Build
        [MenuItem("Build/IOs/Development", false, 1)]
        static void BuildiOSDev()
        {
            ExtraSettings extra = ExtraSettings.Dev();
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.iOS));

            BuildAll(null, extra);
        }

        [MenuItem("Build/IOs/Release", false, 2)]
        static void BuildiOSRelease()
        {
            ExtraSettings extra = new ExtraSettings();
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.iOS));

            BuildAll(null, extra);
        }

        [MenuItem("Build/IOs/All", false, 14)]
        static void BuildiOSAll()
        {
            BuildiOSDev();
            BuildiOSRelease();
        }
        #endregion

        #region Android Build
        [MenuItem("Build/Android/Development", false, 1)]
        static void BuildAndroidDev()
        {
            ExtraSettings extra = ExtraSettings.Dev();
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.Android));

            BuildAll(null, extra);
        }

        [MenuItem("Build/Android/Release", false, 2)]
        static void BuildAndroidRelease()
        {
            ExtraSettings extra = new ExtraSettings();
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.Android));

            BuildAll(null, extra);
        }

        [MenuItem("Build/Android/All", false, 14)]
        static void BuildAndroidAll()
        {
            BuildAndroidDev();
            BuildAndroidRelease();
        }
        #endregion

        #region WebGL Build
        [MenuItem("Build/WebGL/Development", false, 1)]
        static void BuildWebDev()
        {
            ExtraSettings extra = ExtraSettings.Dev();
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.WebGL));

            BuildAll(null, extra);
        }

        [MenuItem("Build/WebGL/Release", false, 2)]
        static void BuildWebRelease()
        {
            ExtraSettings extra = new ExtraSettings();
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.WebGL));

            BuildAll(null, extra);
        }

        [MenuItem("Build/WebGL/All", false, 14)]
        static void BuildWebAll()
        {
            BuildWebDev();
            BuildWebRelease();
        }
        #endregion

        #region PC Build
        [MenuItem("Build/PC/Development", false, 1)]
        static void BuildPCDev()
        {
            ExtraSettings extra = ExtraSettings.Dev();

            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneOSX));
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneLinux64));
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneWindows));
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneWindows64));

            BuildAll(null, extra);
        }

        [MenuItem("Build/PC/Release", false, 2)]
        static void BuildPCRelease()
        {
            ExtraSettings extra = new ExtraSettings();

            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneOSX));
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneLinux64));
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneWindows));
            extra.targets.Add(BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneWindows64));

            BuildAll(null, extra);
        }

        [MenuItem("Build/PC/All", false, 14)]
        static void BuildPCAll()
        {
            BuildPCDev();
            BuildPCRelease();
        }
        #endregion

        #region All
        [MenuItem("Build/Build All", false, 14)]
        static void BuildAll()
        {
            BuildAll();
        }

        public static void BuildAll(BuildEditorSettings _setting = null, ExtraSettings _extra = null, int _index = -1)
        {
            if (_setting == null)
            {
                _setting = BuildScriptUtilities.GetBuildSetting();

                if (_setting == null)
                {
                    BuildScriptUtilities.LogError("No build setting found!");
                    return;
                }

                BuildScriptUtilities.Log("Loading "+_setting.name+" file...");

                //_setting = settings[0];
            }

            BuildSettings(_setting, _extra, _index);
        }
        #endregion

        #region CLI
        //[RuntimeInitializeOnLoadMethod]
        static void BuildCLI()
        {
            BuildScriptUtilities.terminal = false;

            ExtraSettings extra = new ExtraSettings();
            extra.noAlerts = true;
            extra.mainBuild = false;

            string[] args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-targets")
                {
                    i++;

                    while (i < args.Length && !args[i].Contains("-"))
                    {
                        extra.targets.Add(args[i++]);
                    }

                    if (extra.targets.Count == 0)
                    {
                        BuildScriptUtilities.LogError("ERROR: no valid targets found!");
                        return;
                    }
                }
                else if (args[i] == "-dev")
                {
                    extra.forceDev = true;
                }
                else if (args[i] == "-main")
                {
                    extra.mainBuild = true;
                }
                else if (args[i] == "-sequence")
                {
                    int newSequence = -1;

                    if (int.TryParse(args[++i], out newSequence))
                    {
                        extra.forceSequence = newSequence;
                    }
                }
                else if (args[i] == "-terminal")
                {
                    BuildScriptUtilities.terminal = true;
                }
                else if (args[i] == "-tag")
                {
                    i++;

                    while (i < args.Length && !args[i].Contains("-"))
                    {
                        extra.tags.Add(args[i++]);
                    }

                    if (extra.tags.Count == 0)
                    {
                        BuildScriptUtilities.LogError("ERROR: no valid tags found!");
                        return;
                    }
                }
            }

            BuildAll(null, extra);
        }
        #endregion
        #endregion

        #region Functions
        public static string ParseString(string _base)
        {
            if(string.IsNullOrEmpty(_base))
            {
                return _base;
            }

            foreach (var key in dictionary)
            {
                _base = _base.Replace(key.Key, key.Value);
            }

            return _base;
        }

        static void CreateDictionary()
        {
            AddToDictionary("Product_Name", PlayerSettings.productName);
            AddToDictionary("Company_Name", PlayerSettings.companyName);
            AddToDictionary("Asset_Folder", Application.dataPath+"/");
            AddToDictionary("Root_Folder",  Application.dataPath.Replace("/Assets", "/"));
            AddToDictionary("Build_Folder", Application.dataPath.Replace("/Assets", "/") + "Build/");

            DateTime saveUtcNow = DateTime.UtcNow;

            AddToDictionary("Date",           saveUtcNow.ToShortDateString()+" "+ saveUtcNow.ToShortTimeString());

            AddToDictionary("Date.ShortFull", saveUtcNow.ToShortDateString() + " " + saveUtcNow.ToShortTimeString());
            AddToDictionary("Date.LongFull",  saveUtcNow.ToLongDateString() + " " + saveUtcNow.ToLongTimeString());

            AddToDictionary("Date.Path",      DateTime.Now.ToString("yyyyMMddHHmmss"));
            AddToDictionary("Date.Path.Date", DateTime.Now.ToString("yyyyMMdd"));
            AddToDictionary("Date.Path.Time", DateTime.Now.ToString("HHmmss"));

            AddToDictionary("Date.ShortDate", saveUtcNow.ToShortDateString());
            AddToDictionary("Date.LongDate",  saveUtcNow.ToLongDateString());
            AddToDictionary("Date.ShortTime", saveUtcNow.ToShortTimeString());
            AddToDictionary("Date.LongTime",  saveUtcNow.ToLongTimeString());

            AddToDictionary("Date.Day",       saveUtcNow.Day.ToString());
            AddToDictionary("Date.Month",     saveUtcNow.Month.ToString());
            AddToDictionary("Date.Year",      saveUtcNow.Year.ToString());
        }

        static void AddToDictionary(string _key, string _value)
        {
            dictionary["{" + _key + "}"] = _value;
        }

        public static Dictionary<string, string> GetDictionary()
        {
            Dictionary<string, string> tags = new Dictionary<string, string>();

            tags["Short_Target"] = "Platform/Short Target";
            tags["Long_Target"] = "Platform/Target";
            tags["Extension"] = "Platform/Extension";

            tags["Product_Name"] = "Project/Product Name";
            tags["Company_Name"] = "Project/Company Name";
            tags["Sequence"] = "Project/Sequence";

            tags["Asset_Folder"] = "Folder/Asset Folder";
            tags["Build_Folder"] = "Folder/Build Folder";
            tags["Root_Folder"] = "Folder/Project Folder";

            tags["Date"] = "Date/Date";

            tags["Date.Day"] = "Date/Day";
            tags["Date.Month"] = "Date/Month";
            tags["Date.Year"] = "Date/Year";

            tags["Date.ShortFull"] = "Date/Short Date Time";
            tags["Date.LongFull"] = "Date/Long Date Time";

            tags["Date.Path"] = "Date/Path Date Time";
            tags["Date.Path.Date"] = "Date/Path Date";
            tags["Date.Path.Time"] = "Date/Path Time";

            tags["Date.ShortDate"] = "Date/Short Date";
            tags["Date.LongDate"] = "Date/Long Date";
            tags["Date.ShortTime"] = "Date/Short Time";
            tags["Date.LongTime"] = "Date/Long Time";

            return tags;
        }

        public static void CreateSampleDictionary()
        {
            CreateDictionary();

            AddToDictionary("Short_Target", BuildScriptUtilities.GetShortTargetName(BuildTarget.StandaloneWindows));
            AddToDictionary("Long_Target", BuildTarget.StandaloneWindows.ToString());
            AddToDictionary("Extension", BuildScriptUtilities.GetTargetExtension(BuildTarget.StandaloneWindows));
            AddToDictionary("Sequence", "99");
        }

        public static void ZipBuild(string _build, string _path, string _file)
        {
            #if _BUILDSTEPS_ZIP_FILE_
                if(File.Exists(_path + _file))
                {
                    BuildScriptUtilities.LogError("Zip found at destine, overwriting...");
                    File.Delete(_path + _file);
                }

                string pathToZip = _build;

                if(!File.GetAttributes(_build).HasFlag(FileAttributes.Directory))
                {
                    pathToZip = new FileInfo(_build).Directory.FullName;
                }

                BuildScriptUtilities.Log("Building Step: Zipping path " + pathToZip + " to "+ _path + _file+" - "+ File.GetAttributes(_build).HasFlag(FileAttributes.Directory));
                System.IO.Compression.ZipFile.CreateFromDirectory(pathToZip, _path + _file);
            #else
                BuildScriptUtilities.LogError("Zip settings not enabled!");
            #endif
        }
		
        public static bool HasToGenerateSettings()
        {
            #if _BUILDSTEPS_ZIP_FILE_
            return false;
            #else
            return true;
            #endif
        }
        public static void GenerateFileForZip()
        {
            string path = Application.dataPath;
            string file = "/csc.rsp";

            string fullPath = path + file;

            Debug.Log("Checking prerequisites files...");

            if (File.Exists(fullPath))
            {
                Debug.Log("File found, checking content for prerequisites...");
                StreamReader theReader = new StreamReader(fullPath, Encoding.Default);
                string checkCondition = theReader.ReadToEnd();
                theReader.Close();

                if(!checkCondition.Contains("-r:System.IO.Compression.FileSystem.dll"))
                {
                    Debug.Log("Writing files conditions...");
                    StreamWriter writer = new StreamWriter(fullPath, true);
                    writer.WriteLine("\n-r:System.IO.Compression.FileSystem.dll\n");
                    writer.Close();
                }
            }
            else
            {
                Debug.Log("File not found, creating new file...");
                FileStream createdFile = File.Create(fullPath);
                createdFile.Close();

                Debug.Log("Writing files conditions...");
                StreamWriter writer = new StreamWriter(fullPath, true);
                writer.WriteLine("\n-r:System.IO.Compression.FileSystem.dll\n");
                writer.Close();
            }


            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (!symbols.Contains("_BUILDSTEPS_ZIP_FILE_"))
            {
                Debug.Log("Creating tags and variables...");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols+ ";_BUILDSTEPS_ZIP_FILE_");
            }


            EditorUtility.DisplayDialog("Settings Created", "Setting files and symbols created", "OK");
        }

        static void CalcBuildProgress(BuildEditorSettings _settings)
        {
            currParts = 0;

            totalParts = _settings.Steps.Count;

            for (int count = 0; count < _settings.Steps.Count; count++)
            {
                totalParts += _settings.Steps[count].preBuildActions.Count;
                totalParts += _settings.Steps[count].postBuildActions.Count;

                if (_settings.Steps[count].zipBuild)
                {
                    totalParts++;
                }

                if (_settings.Steps[count].overwriteScenes)
                {
                    totalParts++;
                }
            }
        }

        static void SetStep(string _step, bool _countStep = true)
        {
            currStep = _step;

            if (_countStep)
            {
                currParts++;
            }

            Debug.Log(_step);

            if(BuildScriptUtilities.terminal)
            {
                //Console.WriteLine(_step);
            }

            if(onStep != null) onStep();
        }
        #endregion

        #region Build
        static bool PreBuild(BuildEditorSettings _settings, ExtraSettings _extra = null, bool _changeVersion = true)
        {
            CreateDictionary();

            if (_settings.controlVersion)
            {
                if (BuildScriptUtilities.IsNewSequence(_settings))
                {
                    _settings.Sequence++;
                }
                else
                {
                    _settings.Sequence = 0;
                }

                tempSequence = _settings.Sequence;

                if (_extra != null)
                {
                    if (_extra.forceSequence != -1)
                    {
                        BuildScriptUtilities.Log("Building Step: Forcing Sequence to " + _extra.forceSequence);
                        tempSequence = _extra.forceSequence;
                    }
                }

                AddToDictionary("Sequence", tempSequence.ToString());

                string version = ParseString(_settings.Version);

                AddToDictionary("Version", version);

                PlayerSettings.bundleVersion = version;
            }
            else
            {

                tempSequence = _settings.Sequence;

                if (_extra != null)
                {
                    if (_extra.forceSequence != -1)
                    {
                        BuildScriptUtilities.Log("Building Step: Forcing Sequence to " + _extra.forceSequence);
                        tempSequence = _extra.forceSequence;
                    }
                }

                AddToDictionary("Sequence", tempSequence.ToString());
            }

            return true;
        }

        static void BuildSettings(BuildEditorSettings _settings, ExtraSettings _extra = null, int _index = -1)
        {
            isBuilding = true;

            BuildTarget projectTarget           = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup projectTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            PreBuild(_settings, _extra);

            //Execute Pre Actions
            for (int count = 0; count < _settings.preBuildActions.Count; count++)
            {
                SetStep("Building Step: Executing pre action " + _settings.preBuildActions[count].GetName());

                if (!_settings.preBuildActions[count].Exec(_settings, null, _extra, null, null) || !string.IsNullOrEmpty(_settings.preBuildActions[count].GetError()))
                {
                    if (!string.IsNullOrEmpty(_settings.preBuildActions[count].GetError()))
                    {
                        BuildScriptUtilities.LogError("ERROR: Pre Step " + _settings.preBuildActions[count].GetName() +
                                                      " returned a error - " + _settings.preBuildActions[count].GetError());
                    }
                }
            }

            if (_index == -1)
            {
                for (int count = 0; count < _settings.Steps.Count; count++)
                {
                    BuildStep(_settings, _settings.Steps[count], _extra);
                }
            }
            else
            {
                if(_index < 0 || _index >= _settings.Steps.Count)
                {
                    BuildScriptUtilities.LogError("ERROR: Invalid Build Index");
                    return;
                }

                BuildStep(_settings, _settings.Steps[_index], _extra);
            }

            //Execute Post Actions
            for (int count = 0; count < _settings.postBuildActions.Count; count++)
            {
                SetStep("Building Step: Executing post action " + _settings.postBuildActions[count].GetName());

                if (!_settings.postBuildActions[count].Exec(_settings, null, _extra, null, null) || !string.IsNullOrEmpty(_settings.postBuildActions[count].GetError()))
                {
                    if (!string.IsNullOrEmpty(_settings.postBuildActions[count].GetError()))
                    {
                        BuildScriptUtilities.LogError("ERROR: Post Step " + _settings.postBuildActions[count].GetName() +
                                                      " returned a error - " + _settings.postBuildActions[count].GetError());
                    }
                }
            }

            //Return original Platform
            bool switchResult = EditorUserBuildSettings.SwitchActiveBuildTarget(projectTargetGroup, projectTarget);

            if (!switchResult)
            {
                BuildScriptUtilities.Log("Unable to change Build Target to: " + projectTarget.ToString());
                return;
            }

            isBuilding = false;
            if (onComplete != null) onComplete();
        }

        static void BuildStep(BuildEditorSettings _settings, BuildEditorSettings.Step _step, ExtraSettings _extra = null)
        {
            _step.wasBuild = false;

            SetDictionaryToStep(_step);

            string buildPath = ParseString(_settings.Path);
            string filePath = ParseString(_settings.File);

            //Replace step if necessary
            if (_step.overwritePath)
            {
                buildPath = ParseString(_step.path);
            }

            if(string.IsNullOrEmpty(buildPath))
            {
                BuildScriptUtilities.LogError("Destination Path can't be null!!");
                return;
            }

            //Check if a custom build step is set
            if (_step.overwriteStep != null)
            {
                _step.overwriteStep.Exec(_settings, _step, _extra, buildPath, filePath);
                return;
            }

            //Filter by tag and platform
            if (_extra != null)
            {
                if(_extra.targets.Count > 0)
                {
                    if(!_extra.targets.Contains(BuildScriptUtilities.GetShortTargetName(_step.Target)))
                    {
                        BuildScriptUtilities.Log("Skipping "+ _step.Name+ " : Missing target "+ String.Join(", ", _extra.targets.ToArray()));
                        return;
                    }
                }

                if (_extra.tags.Count > 0)
                {
                    string[] labels = _step.Labels.Split(' ');
                    bool found = false;

                    for (int count = 0; count < labels.Length; count++)
                    {
                        if (_extra.tags.Contains(labels[count]))
                        {
                            found = true;
                            break;
                        }
                    }

                    if(!found)
                    {
                        BuildScriptUtilities.Log("Skipping " + _step.Name + " : Missing label " + String.Join(", ", _extra.tags.ToArray()));
                        return;
                    }
                }
            }

            if ((_step.Option & BuildOptions.AcceptExternalModificationsToPlayer) != BuildOptions.AcceptExternalModificationsToPlayer &&
                (File.Exists(buildPath + "/" + filePath) || Directory.Exists(buildPath + "/" + filePath)))
            {
                BuildScriptUtilities.LogError("Another Build found at " + buildPath + "/" + filePath);
                return;
            }

            //Check if platform is different
            if (_extra != null && _extra.mainBuild)
            {
                if (!_step.mainBuild)
                {
                    BuildScriptUtilities.Log("Skipping " + _step.Name + " : Not a Main Build");
                    return;
                }
            }


            //Check if platform is different
            if (_extra != null && !_extra.noAlerts)
            {
                if(_step.Target != EditorUserBuildSettings.activeBuildTarget)
                {
                    if (EditorUtility.DisplayDialog("Switch Targets?", "Do you want to switch platforms? It can take several minutes", "Cancel", "Switch"))
                    {
                        BuildScriptUtilities.Log("Stopping Step "+ _step.Name);
                        return;
                    }
                }
            }

            if(!IsPlatformSupported(_step.Target))
            {
                BuildScriptUtilities.Log("Platform "+ _step.Target + " not supported, please download the Unity Module");
                return;
            }

            //Switching platforms
            SetStep("Building Step: " + _step.Name + " - Switching Platform to " + _step.Target);


            bool switchResult = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildScriptUtilities.GetTargetGroup(_step.Target), _step.Target);

            if (!switchResult)
            {
                BuildScriptUtilities.Log("Unable to change Build Target to: " + _step.Target.ToString());
                return;
            }

            //Get Scenes from overwrite or the editor
            string[] scenes;

            SetStep("Building Step: " + _step.Name +" - Getting Scenes");

            if (_step.overwriteScenes)
            {
                scenes = BuildScriptUtilities.GetSceneList(_step);
            }
            else
            {
                scenes = BuildScriptUtilities.GetSceneLevels();
            }

            SetStep("Building Step: " + _step.Name + " - Creating Directory");
            Directory.CreateDirectory(buildPath);

            //Execute Pre Actions
            for (int count = 0; count < _step.preBuildActions.Count; count++)
            {
                SetStep("Building Step: " + _step.Name + " - Executing pre action "+ _step.preBuildActions[count].name);

                if (!_step.preBuildActions[count].Exec(_settings, _step, _extra, buildPath, filePath) || !string.IsNullOrEmpty(_step.preBuildActions[count].GetError()))
                {
                    if (!string.IsNullOrEmpty(_step.postBuildActions[count].GetError()))
                    {
                        BuildScriptUtilities.LogError("ERROR: Pre Step " + _step.postBuildActions[count].GetName() +
                                                      " returned a error - " + _step.postBuildActions[count].GetError());
                    }
                }
            }


            SetStep("Building Step: " + _step.Name + " - Building Project");
            //Create the Build and Zip

            BuildOptions options = _step.Option;

            if (_extra != null && _extra.forceDev)
            {
                BuildScriptUtilities.Log("Forcing Dev Mode");
                options |= BuildOptions.Development;
            }

            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(scenes, buildPath + "/" + filePath, _step.Target, options);
            UnityEditor.Build.Reporting.BuildSummary summary = report.summary;

            if(summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                BuildScriptUtilities.LogError("Building Step: Build Failed - Target: "+ _step.Name + " Time:" + summary.totalTime + " Total Errors: " + summary.totalErrors);

                foreach(var step in report.steps)
                {
                    foreach (var message in step.messages)
                    {
                        if (message.type != LogType.Log)
                        {
                            BuildScriptUtilities.LogError(message.content);
                        }
                    }
                }

                return;
            }
            else
            {
                _step.wasBuild = true;
                BuildScriptUtilities.LogError("Build Completed with Success!");
            }

            if (_step.zipBuild)
            {
                SetStep("Building Step: " + _step.Name + " - Zipping Project");

                string parentFolder = System.IO.Directory.GetParent(buildPath + "/" + filePath).ToString();
                ZipBuild(buildPath + "/" + filePath, parentFolder+".zip", "");
            }

            //Execute Post Actions
            for (int count = 0; count < _step.postBuildActions.Count; count++)
            {
                SetStep("Building Step: " + _step.Name + " - Executing post action " + _step.postBuildActions[count].GetName());

                if (!_step.postBuildActions[count].Exec(_settings, _step, _extra, buildPath, filePath) || !string.IsNullOrEmpty(_step.postBuildActions[count].GetError()))
                {
                    if (!string.IsNullOrEmpty(_step.postBuildActions[count].GetError()))
                    {
                        BuildScriptUtilities.LogError("ERROR: Post Step "+ _step.postBuildActions[count].GetName()+
                                                      " returned a error - "+ _step.postBuildActions[count].GetError());
                    }
                }
            }
        }

        public static void SetDictionaryToStep(BuildEditorSettings.Step _step)
        {
            AddToDictionary("Short_Target", BuildScriptUtilities.GetShortTargetName(_step.Target));
            AddToDictionary("Long_Target", _step.Target.ToString());
            AddToDictionary("Extension", BuildScriptUtilities.GetTargetExtension(_step.Target));
        }

        static bool IsPlatformSupported(BuildTarget _target)
        {
            var moduleManager = System.Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
            var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            return (bool)isPlatformSupportLoaded.Invoke(null, new object[] { (string)getTargetStringFromBuildTarget.Invoke(null, new object[] { _target }) });
        }
        #endregion
    }
}