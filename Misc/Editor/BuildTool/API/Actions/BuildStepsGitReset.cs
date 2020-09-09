using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Falcone.BuildTool
{
    public class BuildStepsGitReset : TemplateBuildAction
    {
        public enum Type
        {
            Soft,
            Hard
        }

        [SerializeField, Tooltip("Type of git Reset")]
        Type type;

        public override string GetName()
        {
            return "Git Reset";
        }

        public override bool Exec(BuildEditorSettings _settings,
                                  BuildEditorSettings.Step _step,
                                  BuildScript.ExtraSettings _extra,
                                  string _path,
                                  string _file)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "git reset";
                startInfo.Arguments = (this.type == Type.Hard)? "--hard HEAD^" : "--soft";
                process.StartInfo = startInfo;
                process.Start();
            }
            catch (System.Exception e)
            {
                this.lastError = "ERROR: Build Steps couldn't execute 'git reset', please check if your system has git installed - " + e.Message;
                return false;
            }

            return true;
        }
    }
}