
using SerializedActions.UnitTests;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SerializedActions.Editors {
    [UnityEditor.CustomEditor(typeof(SerializedActions_MethodsRegisters))]

    public class BaseImplementation_UnitTest_Inspector : Editor {

        private bool showImplementations = false;
        private bool[] showImplementation = null;
        private Dictionary<SerializedActions_MonobehaviourManager, bool[]> actions = new Dictionary<SerializedActions_MonobehaviourManager, bool[]>();
        private const int TIMELINES_COUNT = 5;
        public override void OnInspectorGUI() {
            SerializedActions_MethodsRegisters targetInstance = (SerializedActions_MethodsRegisters)target;
            SerializedObject objInstance = new SerializedObject(targetInstance);
            if (targetInstance == null || objInstance == null)
                return;
            objInstance.Update();
            base.OnInspectorGUI();
            showImplementations = EditorGUILayout.Foldout(showImplementations, "Show implementations");
            if (showImplementations) {
                if (showImplementation == null) { // Initialise
                    showImplementation = new bool[targetInstance.implementationsInProject.Count];
                    foreach (SerializedActions_MonobehaviourManager imple in targetInstance.implementationsInProject)
                        actions.Add(imple, new bool[TIMELINES_COUNT]);
                }
                for (int i = 0; i < showImplementation.Length; i++) {
                    ShowImplementationLists(ref showImplementation[i], targetInstance.implementationsInProject[i]);
                    GUILayout.Space(10);
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
            EditorGUILayout.LabelField("Total registered types by names: " + targetInstance.monoscripts.Count);
            foreach (MonoScript script in targetInstance.monoscripts)
                EditorGUILayout.LabelField(script.GetClass().Name);

        }
        private void ShowImplementationLists(ref bool foldout, SerializedActions_MonobehaviourManager implementation) {
            GUIStyle s = new GUIStyle(EditorStyles.foldout);
            s.margin.left = 20;
            foldout = EditorGUILayout.Foldout(foldout, implementation.name, s);
            if (foldout) {
                SerializedActions_MonobehaviourManager imple = implementation;
                ShowList(imple.OnAwakeActions, ref actions[imple][0], "On Awake");
                ShowList(imple.OnStartActions, ref actions[imple][1], "On Start");
                ShowList(imple.OnEnableActions, ref actions[imple][2], "On Enable");
                ShowList(imple.OnDisableActions, ref actions[imple][3], "On Disable");
                ShowList(imple.OnInteractionActions, ref actions[imple][4], "On Select");
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
    }
}