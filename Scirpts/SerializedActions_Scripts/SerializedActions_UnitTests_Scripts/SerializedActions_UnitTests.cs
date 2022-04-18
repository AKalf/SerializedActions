using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
[Serializable]
public class SerializedActions_UnitTests : ScriptableObject {

    private const string pathToScriptable = "Assets/Scripts/Utilities/SerializedActions_Scripts/UIScreen_Base_Implementation_UnitTests.asset";

    private static SerializedActions_UnitTests instance = null;

    [HideInInspector]
    public List<SerializedAction_MonoBehaviour> implementationsInProject = new List<SerializedAction_MonoBehaviour>();
    [SerializeField]
    [HideInInspector]
    public List<SerializedAction_Instance> actionsInProject = new List<SerializedAction_Instance>();
    [SerializeField]
    public List<ClassAndMethods> classesAndMethods = new List<ClassAndMethods>();
    [SerializeField]
    public List<MonoScript> monoscripts = new List<MonoScript>();

    private static string debugMessage = "";

    public static SerializedActions_UnitTests Instance() {
        if (instance == null) {
            instance = AssetDatabase.LoadAssetAtPath<SerializedActions_UnitTests>(pathToScriptable);
            if (instance == null) {
                string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", nameof(SerializedActions_UnitTests)));
                if (guids.Length == 0) {
                    Debug.LogError("<b>Serialized Action <color=Red>ERROR:</color></b> Could not find <b>SerializedActions_UnitTests</b> scriptable object");
                }
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                instance = AssetDatabase.LoadAssetAtPath<SerializedActions_UnitTests>(path);
            }
        }
        return instance;
    }

    // Unity Callbacks
    #region Unity Callbacks
    private void Awake() {
        Instance();
    }
    void OnEnable() {
        Instance();
    }
    #endregion

    /// <summary>Check all SerializedActions in project for errors</summary>
    public void CheckActions() {
        debugMessage = "<b>----|Starting checking actions...</b> ";
        FindImplementations();
        Debug.Log(debugMessage + "\n\n");
        if (implementationsInProject.Count > 0) {
            foreach (SerializedAction_MonoBehaviour imple in implementationsInProject) {
                debugMessage = ("Initialising tests for implementation: " + imple.name);
                CheckImplementation(imple);
            }
            AssetDatabase.SaveAssets();
        }
        else {
            debugMessage += "\n <color=red> No implementations found!</color>";
            Debug.Log(debugMessage + "\n\n");
            debugMessage = "";
        }
    }
    public void CheckSingleAction(SerializedAction_Instance serializedAction, SerializedAction_MonoBehaviour implementation) {
        Type resolvedType = SerializedAction_Type_UnitTest.CheckType(serializedAction, monoscripts, implementation, classesAndMethods);
        if (resolvedType != null) {
            if (SerializedAction_Method_UnitTest.CheckMethods(serializedAction, SerializedAction_Instance.GetType(serializedAction.ClassName), implementation, classesAndMethods, ref debugMessage)) {
                SerializedAction_Parameters_UnitTest.CheckMethodParameters(
                    // Method info
                    SerializedAction_Instance.GetType(serializedAction.ClassName).GetMethod(serializedAction.MethodName),
                    // Action
                    serializedAction,
                    // Implementation
                    implementation);
            }
            else
                Debug.Log(debugMessage + "\n\n");
        }
        else
            Debug.Log(debugMessage + "\n\n");
        //EditorUtility.SetDirty(serializedAction);
    }
    /// <summary> Add an action to the database to check for errors </summary>
    /// <param name="action">The action to add</param>
    /// <param name="script">The script holding the class of the action</param>
    /// <param name="method">The method of the action</param>
    public void AddAction(SerializedAction_Instance action, MonoScript script, MethodInfo method) {
        if (monoscripts.Contains(script) == false) {
            BaseImplementationMethodAttribute attr = (BaseImplementationMethodAttribute)method.GetCustomAttribute(typeof(BaseImplementationMethodAttribute));
            if (attr != null) {
                ClassAndMethods cm = new ClassAndMethods(script.GetClass().Name, method.Name, attr.MethodID);
                classesAndMethods.Add(cm);
                monoscripts.Add(script);
            }
        }
        else {
            BaseImplementationMethodAttribute attr = (BaseImplementationMethodAttribute)method.GetCustomAttribute(typeof(BaseImplementationMethodAttribute));
            ClassAndMethods cm = GetStructByType(script.GetClass().Name, classesAndMethods);
            if (cm != null && attr != null)
                cm.AddMethod(method.Name, attr.MethodID);
        }
        if (actionsInProject.Contains(action) == false) {
            actionsInProject.Add(action);
        }
        EditorUtility.SetDirty(instance);
        AssetDatabase.SaveAssets();
    }

    /// <summary>Check implementation for errors </summary>
    /// <param name="imple">Implementation to check</param>
    private void CheckImplementation(SerializedAction_MonoBehaviour imple) {
        if (imple.OnAwakeActions.Count > 0) {
            debugMessage += "\n\nChecking <b>On Awake</b> list";
            CheckList(imple.OnAwakeActions, imple);
            Debug.Log(debugMessage + "\n\n");
            debugMessage = "";
        }
        if (imple.OnStartActions.Count > 0) {
            debugMessage += "\n\nChecking <b>On Start</b> list";
            CheckList(imple.OnStartActions, imple);
            Debug.Log(debugMessage + "\n\n");
            debugMessage = "";
        }
        if (imple.OnEnableActions.Count > 0) {
            debugMessage += "\ns\nChecking <b>On Enable</b> list";
            CheckList(imple.OnEnableActions, imple);
            Debug.Log(debugMessage + "\n\n");
            debugMessage = "";
        }
        if (imple.OnDisableActions.Count > 0) {
            debugMessage += "\n\nChecking <b>On Disalbe</b> list";
            CheckList(imple.OnDisableActions, imple);
            Debug.Log(debugMessage + "\n\n");
            debugMessage = "";
        }
        if (imple.OnPointEnterActions.Count > 0) {
            debugMessage += "\n\nChecking <b>On Select</b> list";
            CheckList(imple.OnPointEnterActions, imple);
            Debug.Log(debugMessage + "\n\n");
            debugMessage = "";
        }
        EditorUtility.SetDirty(imple.gameObject);

    }

    private void CheckList(List<SerializedAction_Instance> list, SerializedAction_MonoBehaviour implementation) {
        foreach (SerializedAction_Instance serializedAction in list) {
            Type resolvedType = SerializedAction_Type_UnitTest.CheckType(serializedAction, monoscripts, implementation, classesAndMethods);
            if (resolvedType != null) {
                if (SerializedAction_Method_UnitTest.CheckMethods(serializedAction, SerializedAction_Instance.GetType(serializedAction.ClassName), implementation, classesAndMethods, ref debugMessage)) {
                    SerializedAction_Parameters_UnitTest.CheckMethodParameters(
                        // Method info
                        SerializedAction_Instance.GetType(serializedAction.ClassName).GetMethod(serializedAction.MethodName),
                        // Action
                        serializedAction,
                        // Implementation
                        implementation);
                }
                else
                    Debug.Log(debugMessage + "\n\n");
            }
            else
                Debug.Log(debugMessage + "\n\n");
            //EditorUtility.SetDirty(serializedAction);

        }
    }




    protected static ClassAndMethods GetStructByType(string name, List<ClassAndMethods> classesAndMethods) {
        foreach (ClassAndMethods cm in classesAndMethods) {
            if (cm.TypeName == name)
                return cm;
        }
        return null;
    }

    private void FindImplementations() {
        implementationsInProject.Clear();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", nameof(GameObject)));
        List<SerializedAction_MonoBehaviour> implementations = new List<SerializedAction_MonoBehaviour>();
        debugMessage += "\nPrefab implementations: ";
        for (int i = 0; i < guids.Length; i++) {
            string Path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (gameObject != null) {
                SerializedAction_MonoBehaviour Implementation = gameObject.GetComponent<SerializedAction_MonoBehaviour>();
                if (Implementation != null && implementationsInProject.Contains(Implementation) == false) {
                    implementationsInProject.Add(Implementation);
                    debugMessage += Implementation.name + " | ";
                }
            }
        }
        int prefablImplementations = implementationsInProject.Count;
        debugMessage += "\n\nTotal implementations found in prefabs: " + implementationsInProject.Count;
        debugMessage += "\n\nGameobjects in scenes: ";
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
            Scene scene = SceneManager.GetSceneByBuildIndex(i);
            if (scene.IsValid() == false) {
                Debug.LogError("<b>Serialized Action <color=Red>ERROR:</color></b> Could retrieve scene with build-index: " + i);
                continue;
            }
            GameObject[] rootGameobjects = scene.GetRootGameObjects();
            foreach (GameObject root in rootGameobjects) {
                SerializedAction_MonoBehaviour rootImple = root.GetComponent<SerializedAction_MonoBehaviour>();
                if (rootImple != null && implementationsInProject.Contains(rootImple) == false) {
                    implementationsInProject.Add(rootImple);
                    debugMessage += rootImple.name + " | ";
                }
                SerializedAction_MonoBehaviour[] children = root.GetComponentsInChildren<SerializedAction_MonoBehaviour>();
                if (children.Length > 0) {
                    foreach (SerializedAction_MonoBehaviour child in children) {
                        if (implementationsInProject.Contains(child) == false)
                            implementationsInProject.Add(child);
                        debugMessage += child.name + " | ";
                    }
                }
            }
        }
        debugMessage += "\n\nImplementations in scenes found: " + (implementationsInProject.Count - prefablImplementations);
    }

    [Serializable]
    public class ClassAndMethods {
        [SerializeField]
        private string typeName;
        public string TypeName { get => typeName; set => typeName = value; }
        [SerializeField]
        private List<int> methodsIDs;
        public List<int> MethodsIDs => methodsIDs;
        [SerializeField]
        private List<string> methodsNames;
        public List<string> MethodsNames => methodsNames;

        public void AddMethod(string methodName, int methodID) {
            if (methodsIDs.Contains(methodID) == false) {
                methodsNames.Add(methodName);
                methodsIDs.Add(methodID);
            }
        }
        public List<MethodInfo> GetMethods() {
            List<MethodInfo> results = new List<MethodInfo>();
            MethodInfo[] classMethods = SerializedAction_Instance.GetType(typeName).GetMethods(BindingFlags.Public | BindingFlags.Static);
            debugMessage += "\n\nPublic static methods found: " + classMethods.Length;
            foreach (MethodInfo info in classMethods) {
                debugMessage += "\nChecking method: " + info.Name + " for attribute";
                BaseImplementationMethodAttribute attr = info.GetCustomAttribute<BaseImplementationMethodAttribute>();
                if (attr != null)
                    debugMessage += "\nMethod: " + info.Name + " has id: " + attr.MethodID;
                if (attr != null && methodsIDs.Contains(attr.MethodID)) {
                    debugMessage += "\nMethod: " + info.Name + " has registered ID";
                    results.Add(info);
                }
            }
            return results;
        }
        public MethodInfo GetMethodById(int id) {
            debugMessage += "\n\nSearching for method with ID: " + id;
            foreach (MethodInfo info in GetMethods()) {
                BaseImplementationMethodAttribute attr = info.GetCustomAttribute<BaseImplementationMethodAttribute>();
                if (attr.MethodID == id) {
                    debugMessage += "\nMethod found: " + info.Name;
                    return info;
                }
            }
            Debug.LogError("UIScreen_Base_Implementation_UnitTests ERROR! Could not find method with id: " + id + " for type: <b>" + typeName + "</b>");
            debugMessage += "\nUIScreen_Base_Implementation_UnitTests ERROR! Could not find method with id: " + id + " for type: <b>" + typeName + "</b>";
            Debug.Log(debugMessage);
            return null;
        }
        public ClassAndMethods(string type, Dictionary<string, int> namesAndIDs) {
            typeName = type;
            methodsIDs = new List<int>();
            methodsNames = new List<string>();
            foreach (string name in namesAndIDs.Keys) {
                AddMethod(name, namesAndIDs[name]);
            }
        }
        public ClassAndMethods(string type, string name, int id) {
            typeName = type;
            methodsIDs = new List<int>();
            methodsNames = new List<string>();
            AddMethod(name, id);
        }
    }


}
