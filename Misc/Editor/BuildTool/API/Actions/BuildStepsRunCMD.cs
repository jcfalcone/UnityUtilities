using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Falcone.BuildTool
{
    public class BuildStepsRunCMD : TemplateBuildAction
    {
        public enum Type
        {
            Bash,
            Cmd
        }

        [SerializeField, Tooltip("Type of terminal used")]
        Type type;

        [SerializeField, Tooltip("Path where the files will be copied")]
        string Command;

        public override string GetName()
        {
            return "CMD";
        }

        public override bool Exec(BuildEditorSettings _settings,
                                  BuildEditorSettings.Step _step,
                                  BuildScript.ExtraSettings _extra,
                                  string _path,
                                  string _file)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            this.Command = this.Command.Replace("\"", "\\\"");

            if (this.type == Type.Cmd)
            {
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C";
            }
            else
            {
                startInfo.FileName = "/bin/bash";
                startInfo.Arguments = "-c";
            }

            startInfo.Arguments += " "+ this.Command;

            process.StartInfo = startInfo;
            process.Start();

            return true;
        }
    }
}