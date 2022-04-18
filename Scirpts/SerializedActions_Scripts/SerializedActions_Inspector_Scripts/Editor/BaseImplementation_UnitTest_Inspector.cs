
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
[UnityEditor.CustomEditor(typeof(SerializedActions_UnitTests))]

public class BaseImplementation_UnitTest_Inspector : Editor {

    bool showImplementations = false;
    bool[] showImplementation = null;
    Dictionary<SerializedActions_MonoBehaviourHolder, bool[]> actions = new Dictionary<SerializedActions_MonoBehaviourHolder, bool[]>();
    public override void OnInspectorGUI() {
        SerializedActions_UnitTests targetInstance = (SerializedActions_UnitTests)target;
        SerializedObject objInstance = new SerializedObject(targetInstance);
        objInstance.Update();
        base.OnInspectorGUI();
        showImplementations = EditorGUILayout.Foldout(showImplementations, "Show implementations");
        if (showImplementations) {
            if (showImplementation == null) {
                showImplementation = new bool[targetInstance.implementationsInProject.Count];
                foreach (SerializedActions_MonoBehaviourHolder imple in targetInstance.implementationsInProject)
                    actions.Add(imple, new bool[5]);
            }
            for (int i = 0; i < showImplementation.Length; i++) {
                ShowImplementationLists(ref showImplementation[i], targetInstance.implementationsInProject[i]);
                GUILayout.Space(10);
            }

        }
        if (GUILayout.Button("Check actions")) {
            targetInstance.CheckActions();
        }
        if (GUILayout.Button("Clear registed actions")) {
            targetInstance.actionsInProject.Clear();
        }
        EditorGUILayout.LabelField("Total registered types: " + targetInstance.monoscripts.Count + ". Total type names: " + targetInstance.classesAndMethods.Count);
        if (GUILayout.Button("Clear registed types")) {
            targetInstance.classesAndMethods.Clear();
            SerializedActions_UnitTests.Instance().monoscripts.Clear();
        }
        EditorGUILayout.LabelField("Total registered types by names: " + SerializedActions_UnitTests.Instance().monoscripts.Count);
        foreach (MonoScript script in SerializedActions_UnitTests.Instance().monoscripts) {
            EditorGUILayout.LabelField(script.GetClass().Name);
        }

    }
    private void ShowImplementationLists(ref bool foldout, SerializedActions_MonoBehaviourHolder implementation) {
        GUIStyle s = new GUIStyle(EditorStyles.foldout);
        s.margin.left = 20;
        foldout = EditorGUILayout.Foldout(foldout, implementation.name, s);
        if (foldout) {
            SerializedActions_MonoBehaviourHolder imple = implementation;
            ShowList(imple.OnAwakeActions, ref actions[imple][0], "On Awake");
            ShowList(imple.OnStartActions, ref actions[imple][1], "On Start");
            ShowList(imple.OnEnableActions, ref actions[imple][2], "On Enable");
            ShowList(imple.OnDisableActions, ref actions[imple][3], "On Disable");
            ShowList(imple.OnPointEnterActions, ref actions[imple][4], "On Select");
        }
    }
    private void ShowList(List<SerializedAction> actions, ref bool foldout, string listName) {
        if (actions.Count > 0) {
            GUILayout.Space(2.5f);
            GUIStyle s = new GUIStyle(EditorStyles.foldout);
            GUIStyle t = new GUIStyle(EditorStyles.label);
            s.margin.left = 30;
            t.contentOffset = new Vector2(30, 0);
            foldout = EditorGUILayout.Foldout(foldout, listName, s);
            if (foldout) {
                for (int j = 0; j < actions.Count; j++) {
                    SerializedAction action = actions[j];
                    EditorGUILayout.LabelField("Script: " + action.ClassName, t);
                    EditorGUILayout.LabelField("Method: " + action.methodName, t);
                    GUILayout.Space(5);
                }
            }
        }
    }
}
