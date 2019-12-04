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
            if (!Directory.Exists(this.TargetPath))
            {
                this.lastError = "Invalid Target Folder: " + this.TargetPath;
                return false;
            }

            if (_step == null)
            {
                for(int count = 0; count < _settings.Steps.Count; count++)
                {
                    this.MoveBuild(_settings, _settings.Steps[count], _path, _file);
                }
            }
            else
            {
                this.MoveBuild(_settings, _step, _path, _file);
            }

            return true;
        }

        void MoveBuild(BuildEditorSettings _settings,
                       BuildEditorSettings.Step _step,
                       string _path,
                       string _file)
        {
            string buildPath = _path;
            string filePath = string.Empty;

            if (string.IsNullOrEmpty(_path))
            {
                buildPath = BuildScript.ParseString(_settings.Path);
                filePath = BuildScript.ParseString(_settings.File);

                //Replace step if necessary
                if (_step.overwritePath)
                {
                    buildPath = BuildScript.ParseString(_step.path);
                }
            }

            if(_step.zipBuild)
            {
                Directory.SetCurrentDirectory(_path);
                string parentFolder = Directory.GetCurrentDirectory();
                filePath = parentFolder + ".zip";
            }

            if(string.IsNullOrEmpty(filePath))
            {
                DirectoryCopy(filePath,
                              this.TargetPath,
                              true);
            }
            else
            {
                try
                {
                    File.Copy(buildPath + "/" + filePath,
                              this.TargetPath + "/" + filePath, 
                              false);
                }
                catch (IOException iox)
                {
                    this.lastError = "Error on move file ["+ buildPath + "/"+ filePath + "] " + iox.Message;
                }
            }
        }

        private static void DirectoryCopy(string sourceDirName, 
                                          string destDirName, 
                                          bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

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

                // Copy the file.
                file.CopyTo(temppath, false);
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
        }
    }
}