using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Falcone.BuildTool
{
    public class BuildStepsDestroyGameObject : TemplateBuildAction
    {
        public enum Condition
        {
            And,
            Or
        }

        [SerializeField]
        Condition condition;

        [SerializeField]
        new string name;

        [SerializeField]
        string tag;

        [Space, SerializeField, Tooltip("Destroy Game Object in this scenes")]
        Object[] customScenes;

        string[] currBakeScenesPath;
        UnityEngine.SceneManagement.Scene[] currBakeSceneFiles;

        public override string GetName()
        {
            return "Create Prefab";
        }

        public override bool Exec(BuildEditorSettings _settings,
                                  BuildEditorSettings.Step _step,
                                  BuildScript.ExtraSettings _extra,
                                  string _path,
                                  string _file)
        {
            if (string.IsNullOrEmpty(this.name) && string.IsNullOrEmpty(this.tag))
            {
                this.lastError = "No name or tag set in this action";
                return false;
            }

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

            return this.DeleteGameObject();
        }

        bool DeleteGameObject()
        {
            if (this.condition == Condition.Or)
            {
                this.DeleteByName();
                this.DeleteByTag();
                
            }
            else
            {
                if (string.IsNullOrEmpty(this.name) || string.IsNullOrEmpty(this.tag))
                {
                    this.lastError = "No name or tag set in this action";
                    return false;
                }

                this.DeleteByTagAndName();
            }

            return true;
        }

        void DeleteByTag()
        {
            if (!string.IsNullOrEmpty(this.tag))
            {
                GameObject[] objects = GameObject.FindGameObjectsWithTag(this.tag);

                for (int count = 0; count < objects.Length; count++)
                {
                    GameObject.DestroyImmediate(objects[count]);
                }
            }
        }

        void DeleteByName()
        {
            if (!string.IsNullOrEmpty(this.name))
            {
                bool keepSearching = false;

                do
                {
                    GameObject gameObject = GameObject.Find(this.name);
                    keepSearching = gameObject != null;

                    if(keepSearching)
                    {
                        GameObject.DestroyImmediate(gameObject);
                    }

                } while (keepSearching);
            }
        }

        void DeleteByTagAndName()
        {
            if (!string.IsNullOrEmpty(this.tag))
            {
                GameObject[] objects = GameObject.FindGameObjectsWithTag(this.tag);

                for (int count = 0; count < objects.Length; count++)
                {
                    if(objects[count].name != this.name)
                    {
                        continue;
                    }

                    GameObject.DestroyImmediate(objects[count]);
                }
            }
        }
    }
}
