using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace SerializedActions.UnitTests {
    [CreateAssetMenu]
    [Serializable]
    public class UnitTestsDataContainer : ScriptableObject {

        private const string pathToScriptable = "Assets/Scripts/SerializedActions_Scripts";

        private static UnitTestsDataContainer instance = null;

        [HideInInspector]
        public List<SerializedActionsManager> implementationsInProject = new List<SerializedActionsManager>();
        [SerializeField]
        [HideInInspector]
        public List<SerializedAction_Instance> actionsInProject = new List<SerializedAction_Instance>();
        [SerializeField]
        public List<MethodsOfType> classesAndMethods = new List<MethodsOfType>();
        [SerializeField]
        public List<MonoScript> monoscripts = new List<MonoScript>();

        private static string debugMessage = "";

        public static UnitTestsDataContainer Instance() {
            if (instance == null) {
                string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", nameof(UnitTestsDataContainer)));
                if (guids.Length == 0)
                    return null;
                guids = AssetDatabase.FindAssets(string.Format("t:{0}", nameof(UnitTestsDataContainer)));
                instance = AssetDatabase.LoadAssetAtPath<UnitTestsDataContainer>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            return instance;
        }



        /// <summary>Check all SerializedActions in project for errors</summary>
        public void CheckActions() {
            debugMessage = "----|Starting checking actions...".Bold();
            FindImplementations();
            Debug.Log(debugMessage.NewLine(2));
            if (implementationsInProject.Count > 0) {
                foreach (SerializedActionsManager imple in implementationsInProject) {
                    debugMessage = ("Initialising tests for implementation: " + imple.name.Bold());
                    CheckImplementation(imple);
                }
                AssetDatabase.SaveAssets();
            }
            else {
                debugMessage += "\nNo implementations found!".Colored(Color.red);
                Debug.Log(debugMessage.NewLine(2));
                debugMessage = "";
            }
        }
        public void CheckSingleAction(SerializedAction_Instance serializedAction, SerializedActionsManager implementation) {
            Type resolvedType = SerializedTypeTest.CheckType(serializedAction, monoscripts, implementation, classesAndMethods);
            if (resolvedType != null) {
                if (SerializedMethods.CheckMethods(serializedAction, SerializedAction_Instance.GetType(serializedAction.ClassName), implementation, classesAndMethods, ref debugMessage)) {
                    SerializedParametersTest.CheckMethodParameters(
                        // Method info
                        SerializedAction_Instance.GetType(serializedAction.ClassName).GetMethod(serializedAction.MethodName),
                        // Action
                        serializedAction,
                        // Implementation
                        implementation);
                }
                else
                    Debug.Log(debugMessage.NewLine(2));
            }
            else
                Debug.Log(debugMessage.NewLine(2));
        }
        /// <summary> Add an action to the database to check for errors </summary>
        /// <param name="action">The action to add</param>
        /// <param name="script">The script holding the class of the action</param>
        /// <param name="method">The method of the action</param>
        public void AddAction(SerializedAction_Instance action, MonoScript script, MethodInfo method) {
            if (monoscripts.Contains(script) == false) { // If monoscript has not been used before
                BaseImplementationMethodAttribute attr = (BaseImplementationMethodAttribute)method.GetCustomAttribute(typeof(BaseImplementationMethodAttribute));
                if (attr != null) { // if method has the attribute
                    MethodsOfType cm = new MethodsOfType(script.GetClass().Name, method.Name, attr.MethodID);
                    classesAndMethods.Add(cm);
                    monoscripts.Add(script);
                }
            }
            else {
                BaseImplementationMethodAttribute attr = (BaseImplementationMethodAttribute)method.GetCustomAttribute(typeof(BaseImplementationMethodAttribute));
                MethodsOfType cm = GetStructByType(script.GetClass().Name, classesAndMethods);
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
        private void CheckImplementation(SerializedActionsManager imple) {
            debugMessage.NewLine(2);
            if (imple.OnAwakeActions.Count > 0) {
                debugMessage += "Checking " + "On Awake".Bold() + " list";
                CheckList(imple.OnAwakeActions, imple);
                Debug.Log(debugMessage.NewLine(2));
                debugMessage = "";
            }
            if (imple.OnStartActions.Count > 0) {
                debugMessage += "Checking " + "On Start".Bold() + " list";
                CheckList(imple.OnStartActions, imple);
                Debug.Log(debugMessage.NewLine(2));
                debugMessage = "";
            }
            if (imple.OnEnableActions.Count > 0) {
                debugMessage += "Checking " + "On Enable".Bold() + " list";
                CheckList(imple.OnEnableActions, imple);
                Debug.Log(debugMessage.NewLine(2));
                debugMessage = "";
            }
            if (imple.OnDisableActions.Count > 0) {
                debugMessage += "Checking " + "On Disalbe".Bold() + " list";
                CheckList(imple.OnDisableActions, imple);
                Debug.Log(debugMessage.NewLine(2));
                debugMessage = "";
            }
            if (imple.OnPointEnterActions.Count > 0) {
                debugMessage += "Checking " + "On Select".Bold() + " list";
                CheckList(imple.OnPointEnterActions, imple);
                Debug.Log(debugMessage.NewLine(2));
                debugMessage = "";
            }

            EditorUtility.SetDirty(imple.gameObject);

        }

        private void CheckList(List<SerializedAction_Instance> list, SerializedActionsManager implementation) {
            foreach (SerializedAction_Instance serializedAction in list) {
                Type resolvedType = SerializedTypeTest.CheckType(serializedAction, monoscripts, implementation, classesAndMethods);
                if (resolvedType != null) {
                    if (SerializedMethods.CheckMethods(serializedAction, SerializedAction_Instance.GetType(serializedAction.ClassName), implementation, classesAndMethods, ref debugMessage)) {
                        SerializedParametersTest.CheckMethodParameters(
                            // Method info
                            SerializedAction_Instance.GetType(serializedAction.ClassName).GetMethod(serializedAction.MethodName),
                            // Action
                            serializedAction,
                            // Implementation
                            implementation);
                    }
                    else
                        Debug.Log(debugMessage.NewLine(2));
                }
                else
                    Debug.Log(debugMessage.NewLine(2));
            }
        }

        protected static MethodsOfType GetStructByType(string name, List<MethodsOfType> methodsOfType) {
            foreach (MethodsOfType cm in methodsOfType) {
                if (cm.TypeName == name)
                    return cm;
            }
            return null;
        }
        /// <summary>Searches all scenes and prefabs for objects with the "SerializedAction_MonoBehaviour" component </summary>
        private void FindImplementations() {
            implementationsInProject.Clear();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", nameof(GameObject)));
            List<SerializedActionsManager> implementations = new List<SerializedActionsManager>();
            // Prefab Search
            debugMessage += "\nPrefab implementations: ";
            for (int i = 0; i < guids.Length; i++) {
                string Path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
                if (gameObject != null) {
                    SerializedActionsManager Implementation = gameObject.GetComponent<SerializedActionsManager>();
                    if (Implementation != null && implementationsInProject.Contains(Implementation) == false) {
                        implementationsInProject.Add(Implementation);
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
                    SerializedActionsManager rootImple = root.GetComponent<SerializedActionsManager>();
                    if (rootImple != null && implementationsInProject.Contains(rootImple) == false) {
                        implementationsInProject.Add(rootImple);
                        debugMessage += rootImple.name + " | ";
                    }
                    SerializedActionsManager[] children = root.GetComponentsInChildren<SerializedActionsManager>();
                    if (children.Length > 0) {
                        foreach (SerializedActionsManager child in children) {
                            if (implementationsInProject.Contains(child) == false)
                                implementationsInProject.Add(child);
                            debugMessage += child.name + " | ";
                        }
                    }
                }
            }
            debugMessage += "\n\nImplementations in scenes found: " + (implementationsInProject.Count - prefablImplementations);
        }

        /// <summary>Represents a class type and the methods it has that have the "BaseImplementationMethodAttribute" attribute</summary>
        [Serializable]
        public class MethodsOfType {
            [SerializeField]
            private string typeName;
            /// <summary>The type name of the class </summary>
            public string TypeName { get => typeName; set => typeName = value; }
            [SerializeField]
            private List<int> methodsIDs;
            /// <summary>Registered methods IDs </summary>
            public List<int> MethodsIDs => methodsIDs;
            [SerializeField]
            private List<string> methodsNames;
            /// <summary> The methods of this class that have the "BaseImplementationMethodAttribute" attribute </summary>
            public List<string> MethodsNames => methodsNames;

            /// <summary>Adds a new method to the container if not registered already</summary>
            /// <param name="methodName">The name of the method to add</param>
            /// <param name="methodID">The ID of the method to add sa registered from "BaseImplementationMethodAttribute" attribute</param>
            public void AddMethod(string methodName, int methodID) {
                if (methodsIDs.Contains(methodID) == false) {
                    methodsNames.Add(methodName);
                    methodsIDs.Add(methodID);
                }
            }
            /// <summary>Finds all methods defined in ths.type and registers any with the "BaseImplementationMethodAttribute" attribute</summary>
            /// <returns>Returns a list with methods that have the "BaseImplementationMethodAttribute" attribute</returns>
            public List<MethodInfo> RegisterMethods() {
                List<MethodInfo> results = new List<MethodInfo>();
                MethodInfo[] classMethods = SerializedAction_Instance.GetType(typeName).GetMethods(BindingFlags.Public | BindingFlags.Static);
                debugMessage += "\n\nPublic static methods found: " + classMethods.Length.NewLine();
                foreach (MethodInfo info in classMethods) {
                    debugMessage += "Checking method: " + info.Name + " for attribute".NewLine();
                    BaseImplementationMethodAttribute attr = info.GetCustomAttribute<BaseImplementationMethodAttribute>();
                    if (attr != null)
                        debugMessage += "Method: " + info.Name + " has id: " + attr.MethodID.NewLine();
                    if (attr != null && methodsIDs.Contains(attr.MethodID)) {
                        debugMessage += "\nMethod: " + info.Name + " has registered ID";
                        results.Add(info);
                    }
                }
                return results;
            }
            /// <summary>Returns the method by the ID registered from the "BaseImplementationMethodAttribute" attribute </summary>
            /// <param name="id">The ID registered from "BaseImplementationMethodAttribute" attribute</param>
            /// <returns>Returns the method whose "BaseImplementationMethodAttribute" attribute has the provided ID</returns>
            public MethodInfo GetMethodById(int id) {
                debugMessage += "\n\nSearching for method with ID: " + id;
                foreach (MethodInfo info in RegisterMethods()) {
                    BaseImplementationMethodAttribute attr = info.GetCustomAttribute<BaseImplementationMethodAttribute>();
                    if (attr.MethodID == id) {
                        debugMessage += "\nMethod found: " + info.Name;
                        return info;
                    }
                }
                Debug.LogError("UIScreen_Base_Implementation_UnitTests ERROR! Could not find method with id: " + id + " for type: " + typeName.Bold());
                debugMessage += "\nUIScreen_Base_Implementation_UnitTests ERROR! Could not find method with id: " + id + " for type: " + typeName.Bold();
                Debug.Log(debugMessage);
                return null;
            }
            public MethodsOfType(string type, Dictionary<string, int> namesAndIDs) {
                typeName = type;
                methodsIDs = new List<int>();
                methodsNames = new List<string>();
                foreach (string name in namesAndIDs.Keys) {
                    AddMethod(name, namesAndIDs[name]);
                }
            }
            public MethodsOfType(string type, string name, int id) {
                typeName = type;
                methodsIDs = new List<int>();
                methodsNames = new List<string>();
                AddMethod(name, id);
            }
        }
    }
}
