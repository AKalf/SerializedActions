#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using SerializedActions.Extensions;
using ActionContainer = SerializedAction_Container;
using MonoManager = SerializedActions_MonobehaviourManager;
using TypeUnitTest = SerializedActions.UnitTests.SerializedActions_UnitTestForType;
namespace SerializedActions.UnitTests {
    [Serializable]
    [CreateAssetMenu]
    [InitializeOnLoad]
    public class SerializedActions_MethodsRegisters : ScriptableObject {

        [NonSerialized] private static SerializedActions_MethodsRegisters instance = null;
        [SerializeField] public bool ShouldDebug = true;

        [SerializeField][HideInInspector] public List<MonoManager> implementationsInProject = new List<MonoManager>();
        [SerializeField][HideInInspector] public List<ActionContainer> actionsInProject = new List<ActionContainer>();
        [SerializeField] public List<MethodsOfType> classesAndMethods = new List<MethodsOfType>();
        [SerializeField] public List<MonoScript> monoscripts = new List<MonoScript>();

        private static EditorApplication.CallbackFunction editorCallback = () => Instance().CheckActions();
        private static string debugMessage = "";
        private const string NEW_SCRIPTABLE_NAME = "SerializedActions_RegisteredMethods_ScriptableObject";
        private const string SCRIPTABLE_PATH = "Assets/Scirpts/SerializedActions_Scripts/SerializedActions_UnitTests_Scripts/SerializedActions_ScriptableObjects/";

        static SerializedActions_MethodsRegisters() {
            EditorApplication.update += editorCallback;
        }

        public static SerializedActions_MethodsRegisters Instance() {
            if (instance == null) {
                EditorApplication.update -= editorCallback;
                string[] guids = AssetDatabase.FindAssets("t:" + typeof(SerializedActions_MethodsRegisters).Name);
                SerializedActions_MethodsRegisters foundContainer = null;
                if (guids.Length > 0) {
                    for (int i = 1; i < guids.Length; i++)
                        AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guids[i]));
                    foundContainer = AssetDatabase.LoadAssetAtPath(
                       AssetDatabase.GUIDToAssetPath(guids[0]), typeof(SerializedActions_MethodsRegisters))
                       as SerializedActions_MethodsRegisters;
                }
                if (foundContainer != null) {
                    instance = foundContainer;
                    return instance;
                }
                ScriptableObject inst = CreateInstance(typeof(SerializedActions_MethodsRegisters));
                inst.name = NEW_SCRIPTABLE_NAME;
                AssetDatabase.CreateAsset(inst, SCRIPTABLE_PATH + inst.name + ".asset");
                instance = (SerializedActions_MethodsRegisters)inst;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return instance;
        }

        /// <summary>Check all SerializedActions in project for errors</summary>
        public void CheckActions() {
            debugMessage = "----|Scanning for corrupted SerializedActions...".Bold();
            FindImplementations();
            if (ShouldDebug) Debug.Log(debugMessage.NewLine(2));
            if (implementationsInProject.Count > 0) {
                foreach (MonoManager imple in implementationsInProject) {
                    debugMessage = "----|Starting tests for implementation: " + imple.name.Bold();
                    CheckImplementation(imple);
                }

            }
            else {
                debugMessage += "\nNo implementations found!".Colored("red");
                Debug.Log(debugMessage.NewLine(2));
                debugMessage = "";
            }
        }
        /// <summary>Checks a single action for conflicts</summary>
        /// <param name="serializedAction">The action to check</param>
        /// <param name="implementation">The SerializedActions_MonobehaviourManager component that it is stored</param>
        public void CheckSingleAction(ActionContainer serializedAction, MonoManager implementation) {
            Type resolvedType = TypeUnitTest.CheckType(serializedAction, monoscripts, implementation, classesAndMethods);
            if (resolvedType != null) {
                if (SerializedActions_UnitTestForMethod.CheckMethods(serializedAction, resolvedType.GetTypeFromName(serializedAction.ClassName), implementation, classesAndMethods, ref debugMessage)) {
                    SerializedActions_UnitTestForParameters.CheckMethodParameters(
                        // Method info
                        resolvedType.GetTypeFromName(serializedAction.ClassName).GetMethod(serializedAction.MethodName),
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
        public void AddAction(MonoManager managerInstance, ActionContainer action, MonoScript script, MethodInfo method) {
            if (monoscripts.Contains(script) == false) { // If monoscript has not been used before
                BaseImplementationMethodAttribute attr = (BaseImplementationMethodAttribute)method.GetCustomAttribute(typeof(BaseImplementationMethodAttribute));
                if (attr != null) { // if method has the attribute
                    MethodsOfType methodOfType = new MethodsOfType(script.GetClass().Name, method.Name, attr.MethodID);
                    classesAndMethods.Add(methodOfType);
                    monoscripts.Add(script);
                    implementationsInProject.Add(managerInstance);
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
                }
            }
            else {
                BaseImplementationMethodAttribute attr = (BaseImplementationMethodAttribute)method.GetCustomAttribute(typeof(BaseImplementationMethodAttribute));
                MethodsOfType methodOfType = GetStructByType(script.GetClass().Name, classesAndMethods);
                if (methodOfType != null && attr != null)
                    methodOfType.AddMethod(method.Name, attr.MethodID);
            }
            if (actionsInProject.Contains(action) == false) {
                actionsInProject.Add(action);
            }
            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssets();
        }
        /// <summary>Check implementation for errors </summary>
        /// <param name="imple">Implementation to check</param>
        private void CheckImplementation(MonoManager imple) {
            debugMessage.NewLine(2);
            if (imple.OnAwakeActions.Count > 0) {
                debugMessage += "Checking " + "On Awake".Bold() + " list";
                CheckList(imple.OnAwakeActions, imple);
                if (ShouldDebug) Debug.Log(debugMessage.NewLine(1));
                debugMessage = "";
            }
            if (imple.OnStartActions.Count > 0) {
                debugMessage += "Checking " + "On Start".Bold() + " list";
                CheckList(imple.OnStartActions, imple);
                if (ShouldDebug) Debug.Log(debugMessage.NewLine(1));
                debugMessage = "";
            }
            if (imple.OnEnableActions.Count > 0) {
                debugMessage += "Checking " + "On Enable".Bold() + " list";
                CheckList(imple.OnEnableActions, imple);
                if (ShouldDebug) Debug.Log(debugMessage.NewLine(1));
                debugMessage = "";
            }
            if (imple.OnDisableActions.Count > 0) {
                debugMessage += "Checking " + "On Disalbe".Bold() + " list";
                CheckList(imple.OnDisableActions, imple);
                if (ShouldDebug) Debug.Log(debugMessage.NewLine(1));
                debugMessage = "";
            }
            if (imple.OnInteractionActions.Count > 0) {
                debugMessage += "Checking " + "On Interaction".Bold() + " list";
                CheckList(imple.OnInteractionActions, imple);
                if (ShouldDebug) Debug.Log(debugMessage.NewLine(1));
                debugMessage = "";
            }

        }

        private void CheckList(List<ActionContainer> list, MonoManager implementation) {
            foreach (ActionContainer serializedAction in list) {
                Type resolvedType = TypeUnitTest.CheckType(serializedAction, monoscripts, implementation, classesAndMethods);
                if (resolvedType != null) {
                    if (SerializedActions_UnitTestForMethod.CheckMethods(serializedAction, resolvedType.GetTypeFromName(serializedAction.ClassName), implementation, classesAndMethods, ref debugMessage)) {
                        SerializedActions_UnitTestForParameters.CheckMethodParameters(
                            // Method info
                            resolvedType.GetTypeFromName(serializedAction.ClassName).GetMethod(serializedAction.MethodName),
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
            foreach (MethodsOfType method in methodsOfType) {
                if (method.TypeName == name)
                    return method;
            }
            return null;
        }

        /// <summary>Searches all scenes and prefabs for objects with the "SerializedAction_MonoBehaviour" component </summary>
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
                    Debug.LogError("Serialized Action ERROR: ".Colored("red").Bold() + "Could retrieve scene with build-index: " + i);
                    continue;
                }
                GameObject[] rootGameobjects = scene.GetRootGameObjects();
                foreach (GameObject root in rootGameobjects) {
                    MonoManager rootImple = root.GetComponent<MonoManager>();
                    if (rootImple != null && implementationsInProject.Contains(rootImple) == false) {
                        implementationsInProject.Add(rootImple);
                        debugMessage += rootImple.name + " | ";
                    }
                    MonoManager[] children = root.GetComponentsInChildren<MonoManager>();
                    if (children.Length > 0) {
                        foreach (MonoManager child in children) {
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
                Type type = null;
                type = type.GetTypeFromName(typeName);
                MethodInfo[] classMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                debugMessage += "\nPublic static methods found: " + classMethods.Length.NewLine();
                foreach (MethodInfo info in classMethods) {
                    debugMessage += "Checking method: " + info.Name + " for attribute...  ";
                    BaseImplementationMethodAttribute attr = info.GetCustomAttribute<BaseImplementationMethodAttribute>();
                    if (attr != null)
                        debugMessage += "Method: " + info.Name.Bold() + " has id: " + attr.MethodID.NewLine();
                    if (attr != null && methodsIDs.Contains(attr.MethodID)) {
                        debugMessage += "Method: " + info.Name.Bold() + "'s ID is already registered";
                        results.Add(info);
                    }
                }
                return results;
            }
            /// <summary>Returns the method by the ID registered from the "BaseImplementationMethodAttribute" attribute </summary>
            /// <param name="id">The ID registered from "BaseImplementationMethodAttribute" attribute</param>
            /// <returns>Returns the method whose "BaseImplementationMethodAttribute" attribute has the provided ID</returns>
            public MethodInfo GetMethodById(int id) {
                debugMessage += "\nSearching for method with ID: " + id + "...  ";
                foreach (MethodInfo info in RegisterMethods()) {
                    BaseImplementationMethodAttribute attr = info.GetCustomAttribute<BaseImplementationMethodAttribute>();
                    if (attr.MethodID == id) {
                        debugMessage += "Method found: " + info.Name.Bold();
                        return info;
                    }
                }
                Debug.LogError("SerializedActions ERROR!".Colored("red") + " Could not find method with id: " + id + " for type: " + typeName.Bold());
                debugMessage += "SerializedActions ERROR!".Colored("red") + " Could not find method with id: " + id + " for type: " + typeName.Bold();
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
#endif