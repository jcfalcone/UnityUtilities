using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Falcone.BuildTool
{
    public class BuildStepsExecuteProgram : TemplateBuildAction
    {
        [SerializeField, Tooltip("Path to the other program")]
        string ProgramPath;

        [SerializeField, Tooltip("Path to the other program")]
        string Arguments;

        public override string GetName()
        {
            return "Execute Program";
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

            startInfo.FileName = this.ProgramPath;
            startInfo.Arguments = this.Arguments;

            process.StartInfo = startInfo;
            process.Start();

            return true;
        }
    }
}