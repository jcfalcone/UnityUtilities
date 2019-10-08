using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Falcone.BuildTool
{
    public class BuildStepsBakeLightAction : TemplateBuildAction
    {
        [SerializeField, Tooltip("Bake Scenes in this list")]
        Object[] customScenes;

        string[] currBakeScenesPath;
        UnityEngine.SceneManagement.Scene[] currBakeSceneFiles;
        int scenesIndex;

        System.DateTime timeStamp;

        public override string GetName()
        {
            return "Bake Light";
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

                    this.InitializeBake();
                }

                if (_step != null)
                {
                    this.currBakeScenesPath = new string[_step.scenes.Count];
                    this.currBakeSceneFiles = new UnityEngine.SceneManagement.Scene[_step.scenes.Count];

                    for (int count = 0; count < EditorSceneManager.sceneCount; count++)
                    {
                        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetSceneByPath(AssetDatabase.GetAssetPath(_step.scenes[count]));

                        this.currBakeScenesPath[count] = scene.path;
                        this.currBakeSceneFiles[count] = scene;
                    }

                    this.InitializeBake();
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

                this.InitializeBake();
            }

            return true;
        }

        void InitializeBake()
        {
            if (!Lightmapping.isRunning)
            {
                Lightmapping.bakeCompleted += this.SaveScene;
                Lightmapping.bakeCompleted += this.BakeNewScene;
                BakeNewScene();
            }
            else
            {
                Lightmapping.Cancel();
            }
        }

        // Loop through scenes to bake and update on progress
        private void BakeNewScene()
        {
            if (this.scenesIndex < this.currBakeScenesPath.Length)
            {
                EditorSceneManager.OpenScene(this.currBakeScenesPath[this.scenesIndex]);
                this.timeStamp = System.DateTime.Now;
                Lightmapping.Bake();
            }
            else
            {
                this.OnBakeCompleted();
            }
        }

        // Saves the scene at the end of each bake before starting new bake
        private void SaveScene()
        {
            System.TimeSpan bakeSpan = System.DateTime.Now - timeStamp;
            string bakeTime = string.Format("{0:D2}:{1:D2}:{2:D2}", bakeSpan.Hours, bakeSpan.Minutes, bakeSpan.Seconds);
            Debug.Log("(" + this.currBakeSceneFiles[this.scenesIndex].name + ") " + " Baked in " + bakeTime);

            EditorSceneManager.SaveScene(this.currBakeSceneFiles[this.scenesIndex]);
            this.scenesIndex++;
        }

        // When done baking, update the editor text
        private void OnBakeCompleted()
        {
            Lightmapping.bakeCompleted -= this.SaveScene;
            Lightmapping.bakeCompleted -= this.BakeNewScene;
        }
    }
}