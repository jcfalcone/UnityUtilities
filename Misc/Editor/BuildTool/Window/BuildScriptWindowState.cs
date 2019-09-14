using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Falcone.BuildTool
{
    public class BuildScriptWindowState
    {
        public enum States
        {
            Welcome,

            NewPlatform,
            NewSettings,
            NewActions,

            ChangeSettings
        }

        public void Init()
        {

        }

        public void InitState(States _state)
        {
            switch (_state)
            {
                case States.Welcome:
                    this.InitWelcome();
                    break;
            }
        }

        public void Tick(States _state)
        {
            switch (_state)
            {
                case States.Welcome:
                    this.Welcome();
                    break;
            }
        }

        #region Welcome
        public void InitWelcome()
        {

        }

        public void Welcome()
        {
            UIUtility.BeginCenterGroup();

            GUILayout.BeginVertical();
            GUILayout.BeginVertical();
            UIUtility.Space();

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Build Steps", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            GUILayout.Label("Aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            GUILayout.EndVertical();

            UIUtility.Space(2);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Build Steps", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            GUILayout.Label("Aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            GUILayout.Button("Start a new File");
            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            UIUtility.BeginCenterGroup();
            GUILayout.Label("Build Steps", EditorStyles.boldLabel);
            UIUtility.EndCenterGroup();

            GUILayout.Label("Aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            GUILayout.Button("Documentation");
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            UIUtility.EndCenterGroup();
        }
        #endregion
    }
}