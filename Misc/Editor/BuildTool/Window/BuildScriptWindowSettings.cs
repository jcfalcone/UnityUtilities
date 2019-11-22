using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Falcone.BuildTool
{
    [CreateAssetMenu(menuName = "Tools/Build/EditorSettings")]
    public class BuildScriptWindowSettings : ScriptableObject
    {
        public Texture icon;
        public BuildEditorSettings tempSettings;
        public BuildScriptWindowStyle style;
    }
}
