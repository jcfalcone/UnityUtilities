using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Falcone.BuildTool
{
    public class BuildStepsMoveBuild : TemplateBuildAction
    {
        [SerializeField, Tooltip("Path where the files will be copied")]
        string TargetPath;

        string ParsedPath;

        public override string GetName()
        {
            return "Move Build";
        }

        public override bool Exec(BuildEditorSettings _settings,
                                  BuildEditorSettings.Step _step,
                                  BuildScript.ExtraSettings _extra,
                                  string _path,
                                  string _file)
        {
            this.ParsedPath = BuildScript.ParseString(this.TargetPath);

            if (!Directory.Exists(this.ParsedPath))
            {
                Directory.CreateDirectory(this.ParsedPath);
                /*this.lastError = "Invalid Target Folder: " + this.TargetPath;
                return false;*/
            }

            bool finalResult = true;

            if (_step == null)
            {
                for(int count = 0; count < _settings.Steps.Count; count++)
                {
                    BuildScript.SetDictionaryToStep(_settings.Steps[count]);
                    finalResult = finalResult && this.MoveBuild(_settings, _settings.Steps[count], _path, _file);
                }
            }
            else
            {
                BuildScript.SetDictionaryToStep(_step);
                finalResult = finalResult && this.MoveBuild(_settings, _step, _path, _file);
            }

            return finalResult;
        }

        bool MoveBuild(BuildEditorSettings _settings,
                       BuildEditorSettings.Step _step,
                       string _path,
                       string _file)
        {

            if (!_step.wasBuild)
            {
                return true;
            }
            else
            {
                BuildScriptUtilities.Log("BUILD WAS MADE " + _step.Name);
            }

            string buildPath = _path;
            string filePath = string.Empty;

            if (string.IsNullOrEmpty(_path))
            {
                buildPath = BuildScript.ParseString(_settings.Path);
                //filePath = BuildScript.ParseString(_settings.File);

                //Replace step if necessary
                if (_step.overwritePath)
                {
                    buildPath = BuildScript.ParseString(_step.path);
                }
            }

            if(_step.zipBuild)
            {

                // If the source directory does not exist, throw an exception.
                if (!Directory.Exists(buildPath))
                {
                    this.lastError = "Source directory does not exist or could not be found: " + buildPath;
                    return false;
                }

                //Directory.SetCurrentDirectory(buildPath);
                string parentFolder = System.IO.Directory.GetParent(buildPath).ToString();
                filePath = parentFolder + ".zip";

                BuildScriptUtilities.Log("Moving ZIP file "+ filePath);
            }

            if(string.IsNullOrEmpty(filePath))
            {
                BuildScriptUtilities.Log("Moving folder [" + buildPath + "] to [" + this.ParsedPath + "]");
                return DirectoryCopy(buildPath,
                                     this.ParsedPath,
                                     true);
            }
            else
            {
                try
                {
                    if(!File.Exists(filePath))
                    {
                        this.lastError = "Error on move file [" + buildPath + "/" + filePath + "] - File Not Found!";
                        return false;
                    }

                    BuildScriptUtilities.Log("Moving file [" + buildPath + "/" + filePath +"] to ["+ this.ParsedPath + "/" + filePath+"]");

                    File.Copy(filePath,
                              this.ParsedPath + "/" + Path.GetFileName(filePath),
                              false); ;
                }
                catch (IOException iox)
                {
                    this.lastError = "Error on move file ["+ buildPath + "/"+ filePath + "] - " + iox.Message;
                    return false;
                }
            }

            return true;
        }

        private bool DirectoryCopy(string sourceDirName, 
                                   string destDirName, 
                                   bool copySubDirs)
        {

            // If the source directory does not exist, throw an exception.
            if (!Directory.Exists(sourceDirName))
            {
                this.lastError = "Source directory does not exist or could not be found: " + sourceDirName;
                return false;
            }

            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                if (!File.Exists(temppath))
                {
                    // Copy the file.
                    file.CopyTo(temppath, false);
                }
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }

            return true;
        }
    }
}