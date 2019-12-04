using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Falcone.BuildTool
{
    public class BuildStepsBakeNavmesh : TemplateBuildAction
    {
        [SerializeField, Tooltip("Bake Scenes in this list")]
        Object[] customScenes;

        string[] currBakeScenesPath;
        UnityEngine.SceneManagement.Scene[] currBakeSceneFiles;

        System.DateTime timeStamp;

        public override string GetName()
        {
            return "Bake Navmesh";
        }


        public override bool Exec(BuildEditorSettings _settings,
                                  BuildEditorSettings.Step _step,
                                  BuildScript.ExtraSettings _extra,
                                  string _path,
                                  string _file)
        {
            if (this.customScenes.Length == 0)
            {
                if (_step == null)
                {
                    this.currBakeScenesPath = new string[EditorSceneManager.sceneCount];
                    this.currBakeSceneFiles = new UnityEngine.SceneManagement.Scene[EditorSceneManager.sceneCount];

                    for (int count = 0; count < EditorSceneManager.sceneCount; count++)
                    {
                        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetSceneAt(count);

                        this.currBakeScenesPath[count] = scene.path;
                        this.currBakeSceneFiles[count] = scene;
                    }
                }
                else
                {
                    this.currBakeScenesPath = new string[_step.scenes.Count];
                    this.currBakeSceneFiles = new UnityEngine.SceneManagement.Scene[_step.scenes.Count];

                    for (int count = 0; count < EditorSceneManager.sceneCount; count++)
                    {
                        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetSceneByPath(AssetDatabase.GetAssetPath(_step.scenes[count]));

                        this.currBakeScenesPath[count] = scene.path;
                        this.currBakeSceneFiles[count] = scene;
                    }
                }
            }
            else
            {
                this.currBakeScenesPath = new string[this.customScenes.Length];
                this.currBakeSceneFiles = new UnityEngine.SceneManagement.Scene[this.customScenes.Length];

                for (int count = 0; count < EditorSceneManager.sceneCount; count++)
                {
                    UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetSceneByPath(AssetDatabase.GetAssetPath(this.customScenes[count]));

                    this.currBakeScenesPath[count] = scene.path;
                    this.currBakeSceneFiles[count] = scene;
                }
            }

            this.InitializeBake();

            return true;
        }


        void InitializeBake()
        {
            if(NavMeshBuilder.isRunning)
            {
                NavMeshBuilder.Cancel();
            }

            this.BakeScenes();
        }

        void BakeScenes()
        {
            for(int count = 0; count < this.currBakeScenesPath.Length; count++)
            {
                EditorSceneManager.OpenScene(this.currBakeScenesPath[count]);
                this.timeStamp = System.DateTime.Now;

                NavMeshBuilder.BuildNavMesh();

                this.SaveScene(this.currBakeSceneFiles[count]);
            }
        }

        // Saves the scene at the end of each bake before starting new bake
        private void SaveScene(UnityEngine.SceneManagement.Scene _scene)
        {
            System.TimeSpan bakeSpan = System.DateTime.Now - timeStamp;
            string bakeTime = string.Format("{0:D2}:{1:D2}:{2:D2}", bakeSpan.Hours, bakeSpan.Minutes, bakeSpan.Seconds);
            Debug.Log("(" + _scene.name + ") " + " Navmesh Baked in " + bakeTime);

            EditorSceneManager.SaveScene(_scene);
        }
    }
}
