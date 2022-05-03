
using SerializedActions.UnitTests;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using SerializedActions.Extensions;
using MonoManager = SerializedActions_MonobehaviourManager;
namespace SerializedActions.Editors {
    [UnityEditor.CustomEditor(typeof(SerializedActions_MethodsRegisters))]
    public class BaseImplementation_UnitTest_Inspector : Editor {
        private string debugMessage = "";
        private bool showImplementations = false;
        private bool[] showImplementation = null;
        private Dictionary<MonoManager, bool[]> implementationsInProject = new Dictionary<MonoManager, bool[]>();
        private const int TOTAL_TIMELINES = 5;
        public override void OnInspectorGUI() {
            SerializedActions_MethodsRegisters targetInstance = (SerializedActions_MethodsRegisters)target;
            SerializedObject objInstance = new SerializedObject(targetInstance);
            if (targetInstance == null || objInstance == null)
                return;
            objInstance.Update();
            base.OnInspectorGUI();
            showImplementations = EditorGUILayout.Foldout(showImplementations, "Show implementations    Total: " + targetInstance.implementationsInProject.Count);
            if (showImplementations && targetInstance.implementationsInProject.Count > 0) {
                if (showImplementation == null) { // Initialise
                    FindImplementations();
                    showImplementation = new bool[implementationsInProject.Keys.Count];
                }
                else {
                    for (int i = 0; i < showImplementation.Length; i++) {
                        ShowImplementationLists(ref showImplementation[i], targetInstance.implementationsInProject[i]);
                        GUILayout.Space(10);
                    }
                }
            }
            if (GUILayout.Button("Check actions"))
                targetInstance.CheckActions();
            if (GUILayout.Button("Clear registed actions"))
                targetInstance.actionsInProject.Clear();
            EditorGUILayout.LabelField("Total registered types: " + targetInstance.monoscripts.Count + ". Total type names: " + targetInstance.classesAndMethods.Count);
            if (GUILayout.Button("Clear registed types")) {
                targetInstance.classesAndMethods.Clear();
                SerializedActions_MethodsRegisters.Instance().monoscripts.Clear();
            }
            if (GUILayout.Button("Clear implementations"))
                targetInstance.implementationsInProject.Clear();
            EditorGUILayout.LabelField("Total registered types by names: " + targetInstance.monoscripts.Count);
            foreach (MonoScript script in targetInstance.monoscripts)
                EditorGUILayout.LabelField(script.GetClass().Name);

        }
        private void ShowImplementationLists(ref bool foldout, MonoManager implementation) {
            if (implementation == null) return;
            GUIStyle s = new GUIStyle(EditorStyles.foldout);
            s.margin.left = 20;
            foldout = EditorGUILayout.Foldout(foldout, implementation.name, s);
            if (foldout) {
                SerializedActions_MonobehaviourManager imple = implementation;
                ShowList(imple.OnAwakeActions, ref implementationsInProject[imple][0], "On Awake");
                ShowList(imple.OnStartActions, ref implementationsInProject[imple][1], "On Start");
                ShowList(imple.OnEnableActions, ref implementationsInProject[imple][2], "On Enable");
                ShowList(imple.OnDisableActions, ref implementationsInProject[imple][3], "On Disable");
                ShowList(imple.OnInteractionActions, ref implementationsInProject[imple][4], "On Select");
            }
        }
        private void ShowList(List<SerializedAction_Container> actions, ref bool foldout, string listName) {
            if (actions.Count > 0) {
                GUILayout.Space(2.5f);
                GUIStyle s = new GUIStyle(EditorStyles.foldout);
                GUIStyle t = new GUIStyle(EditorStyles.label);
                s.margin.left = 30;
                t.contentOffset = new Vector2(30, 0);
                foldout = EditorGUILayout.Foldout(foldout, listName, s);
                if (foldout) {
                    for (int j = 0; j < actions.Count; j++) {
                        SerializedAction_Container action = actions[j];
                        EditorGUILayout.LabelField("Script: " + action.ClassName, t);
                        EditorGUILayout.LabelField("Method: " + action.MethodName, t);
                        GUILayout.Space(5);
                    }
                }
            }
        }
        private void FindImplementations() {
            implementationsInProject.Clear();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", nameof(GameObject)));
            List<MonoManager> implementations = new List<MonoManager>();
            // Prefab Search
            debugMessage += "\nPrefab implementations: ";
            for (int i = 0; i < guids.Length; i++) {
                string Path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
                if (gameObject != null) {
                    MonoManager Implementation = gameObject.GetComponent<MonoManager>();
                    if (Implementation != null && implementationsInProject.Keys.Contains(Implementation) == false) {
                        implementationsInProject.Add(Implementation, new bool[TOTAL_TIMELINES]);
                        debugMessage += Implementation.name + " | ";
                    }
                }
            }
            int prefablImplementations = implementationsInProject.Count;
            debugMessage += "\n\nTotal implementations found in prefabs: " + implementationsInProject.Count.NewLine(2);
            // GameObjects in Scenes, Search
            debugMessage += "Gameobjects in scenes: ";
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                Scene scene = SceneManager.GetSceneByBuildIndex(i);
                if (scene.IsValid() == false) {
                    Debug.LogError("Serialized Action ERROR: ".Colored(Color.red).Bold() + "Could retrieve scene with build-index: " + i);
                    continue;
                }
                GameObject[] rootGameobjects = scene.GetRootGameObjects();
                foreach (GameObject root in rootGameobjects) {
                    MonoManager rootImple = root.GetComponent<MonoManager>();
                    if (rootImple != null && implementationsInProject.Keys.Contains(rootImple) == false) {
                        implementationsInProject.Add(rootImple, new bool[TOTAL_TIMELINES]);
                        debugMessage += rootImple.name + " | ";
                    }
                    MonoManager[] children = root.GetComponentsInChildren<MonoManager>();
                    if (children.Length > 0) {
                        foreach (MonoManager child in children) {
                            if (implementationsInProject.Keys.Contains(child) == false)
                                implementationsInProject.Add(child, new bool[TOTAL_TIMELINES]);
                            debugMessage += child.name + " | ";
                        }
                    }
                }
            }
            debugMessage += "\n\nImplementations in scenes found: " + (implementationsInProject.Count - prefablImplementations);
        }
    }
}