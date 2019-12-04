using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Falcone.BuildTool
{
    public class BuildStepsGitPull : TemplateBuildAction
    {

        public override string GetName()
        {
            return "Git Pull";
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
            startInfo.FileName = "git pull";
            process.StartInfo = startInfo;
            process.Start();

            return true;
        }
    }
}