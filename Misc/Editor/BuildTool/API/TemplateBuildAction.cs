using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Falcone.BuildTool
{
    public abstract class TemplateBuildAction : ScriptableObject
    {
        protected string lastError;

        public virtual string GetName()
        {
            return "Unknow";
        }

        public virtual string GetError()
        {
            return this.lastError;
        }

        public abstract bool Exec(BuildEditorSettings _settings, 
                                  BuildEditorSettings.Step _step, 
                                  BuildScript.ExtraSettings _extra, 
                                  string _path, 
                                  string _file);
    }
}