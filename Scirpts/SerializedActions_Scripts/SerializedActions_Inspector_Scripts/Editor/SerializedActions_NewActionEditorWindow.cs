using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ActionTimeline = SerializedAction_Instance.ActionTimeline;

namespace SerializedActions.Editors {
    public class SerializedActions_NewActionEditorWindow : EditorWindow {

        /// <summary>The timeline to trigger this action </summary>
        private ActionTimeline selectedTimeline = ActionTimeline.OnPointerEnterInteraction;
        private string debugMessage = "";
        private GUIStyle myStyle = new GUIStyle();
        private SerializedAction_MonoBehaviour targetInstance = null;

        /// <summary>The UI element which will trig the action </summary>
        private UnityEngine.Object triggerObject = null;
        /// <summary>A helping field that holds the object the user selected to retrieve methods from</summary>
        private MonoScript objType = null;
        /// <summary>The type user selected to retreive methods</summary>
        private System.Type type = null;
        /// <summary> /// The index of the selected method in the available methods list /// </summary>
        private int selectedMethodNameIndex = 0;
        /// <summary> /// The name of the selected method in the available methods list /// </summary>
        private string selectedMethodName = "";
        /// <summary>/// Methods reflection representetion for script ///</summary>
        private List<MethodInfo> methodsRetrieved = new List<MethodInfo>();
        /// <summary>/// Names of availble functions to trigger ///</summary>
        private List<string> methodNames = new List<string>();
        /// <summary> /// The parameters of the action to be created /// </summary>
        private List<System.Object> parameters = new List<System.Object>();
        /// <summary> /// The type of parameters of the action to be created /// </summary>
        private List<System.Type> parameterTypes = new List<System.Type>();
        /// <summary> /// The name of the parameters of the action to be created /// </summary>
        private List<string> parametersNames = new List<string>();
        /// <summary> /// The name of the parameters of the action to be created /// </summary>
        private List<string> parametersTypesNames = new List<string>();

        private UnityEngine.EventSystems.EventTriggerType triggerType = UnityEngine.EventSystems.EventTriggerType.PointerEnter;

        private bool hasPressedGetMethods = false;
        private bool hasPressedGetParameters = false;

        [MenuItem("Window/Serialized Actions/New Action")]
        public static void ShowWindow() {
            EditorWindow.GetWindow(typeof(SerializedActions_NewActionEditorWindow));
        }
        public static void ShowWindow(SerializedAction_MonoBehaviour target) {
            SerializedActions_NewActionEditorWindow window = EditorWindow.GetWindow(typeof(SerializedActions_NewActionEditorWindow)) as SerializedActions_NewActionEditorWindow;
            window.targetInstance = target;
        }

        void OnGUI() {
            targetInstance = Drawings.UnityObjectField<SerializedAction_MonoBehaviour>(targetInstance, targetInstance.GetType(), "Target behaviour", true, false);
            if (targetInstance != null) {
                // Get invoke time
                selectedTimeline = (ActionTimeline)EditorGUILayout.EnumPopup(selectedTimeline);
                debugMessage = "Selected timeline for new action: " + selectedTimeline.Bold();
                // Check selected timeline and draw corresponding inspector for list
                switch (selectedTimeline) {
                    // Case is on object click
                    case ActionTimeline.OnPointerEnterInteraction:
                        SelectNewActionTrigger();
                        AddNewActionToListInspector(targetInstance.OnPointEnterActions);
                        break;
                    // Case is OnAwake 
                    case ActionTimeline.OnAwake:
                        AddNewActionToListInspector(targetInstance.OnAwakeActions);
                        break;
                    // Case is OnStart 
                    case ActionTimeline.OnStart:
                        AddNewActionToListInspector(targetInstance.OnStartActions);
                        break;
                    // Case is OnEnable 
                    case ActionTimeline.OnEnable:
                        AddNewActionToListInspector(targetInstance.OnEnableActions);
                        break;
                    // Case is OnDisable
                    case ActionTimeline.OnDisable:
                        AddNewActionToListInspector(targetInstance.OnDisableActions);
                        break;
                }
            }
        }

        /// <summary>Draw the field for the trigger object </summary>
        /// <returns>Returns true if the field value wasnt null</returns>
        private bool SelectNewActionTrigger() {
            myStyle.fontSize = 15;
            myStyle.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("New action", myStyle, GUILayout.Height(20));
            // Set the UI element that will trigger the action
            triggerObject = Drawings.UnityObjectField<UnityEngine.Object>(triggerObject, typeof(UnityEngine.Object), "New Input: ");
            if (triggerObject != null) {
                Selectable selectable = null;
                if (triggerObject.GetType().Equals(typeof(GameObject))) {
                    selectable = ((GameObject)triggerObject).GetComponent<Selectable>();
                    if (selectable != null)
                        triggerObject = selectable;
                }
                debugMessage += "\nTrigger object selected: " + triggerObject.name.Bold();
                if (triggerObject.GetType().IsSubclassOf(typeof(UnityEngine.UI.Selectable)) == false) {
                    triggerType = (UnityEngine.EventSystems.EventTriggerType)EditorGUILayout.EnumPopup(triggerType);
                }
                GUILayout.Space(10);
                return true;
            }
            else
                return false;

        }

        /// <summary>Draw the MonoScript field where the class is defined</summary>
        /// <returns>Returns true if the field value wasnt null</returns>
        private bool SelectNewActionMonoScript() {
            // Set an object to retrieve methods from
            objType = Drawings.UnityObjectField<MonoScript>(objType, typeof(MonoScript), "Script with class: ", true, false);
            if (objType != null) { // if object has been assigned
                type = objType.GetClass(); // Get the class of the monoscript assigned
                debugMessage += "Type selected: " + type?.Bold().NewLine();
            }
            if (type != null) {
                GUILayout.TextArea("Type selected: " + type?.Bold().NewLine());
                GUILayout.Space(25);
                return true;
            }
            return false;
        }

        /// <summary>Find the public static methods of the class given</summary>
        /// <returns>Returns true if method selected</returns>
        private bool SelectNewActionMethod() {
            methodNames.Clear();
            // Get all methods of the script
            List<MethodInfo> methodInfo = type.GetMethods().ToList();
            // Get each name

            debugMessage += "\nGetting methods in class... Methods found: ";
            foreach (MethodInfo method in methodInfo) {
                // if it is not a derived class (?! that is what i noticed)
                if (method.Equals(method.GetBaseDefinition()) && method.IsStatic && method.IsPublic && method.GetCustomAttributes(typeof(BaseImplementationMethodAttribute)).FirstOrDefault() != null) {
                    debugMessage += method.Name.Comma();
                    methodsRetrieved.Add(method);
                    methodNames.Add(method.Name);
                }
            }
            debugMessage += "\nPublic static methods found: " + methodNames.Count.NewLine();
            if (methodNames.Count > 0) {
                // if names found, initalize  methods pop-up values
                selectedMethodNameIndex = EditorGUILayout.Popup("Available methods: " + methodNames.Count, selectedMethodNameIndex, methodNames.ToArray());
                selectedMethodName = methodNames[selectedMethodNameIndex];
                GUILayout.Space(15);
                return true;
            }
            else
                return false;

        }

        /// <summary>Get parameter types of method selected and draw fields for them </summary>
        /// <returns>Returns true if method has parameters</returns>
        private bool SelectNewActionParameters(MethodInfo methodInfo) {
            if (parameters.Count > 0) {
                for (int index = 0; index < parameters.Count; index++) {
                    parameters[index] = Drawings.DrawArgumentIfType(
                        parameters[index], parameterTypes[index], parametersNames[index],
                        shouldShowName: true, shouldShowType: false);
                }
                EditorUtility.SetDirty(targetInstance);
                return true;
            }
            else {
                parameters.Clear();
                parameterTypes.Clear();
                parametersNames.Clear();
                // Method parameters signature
                ParameterInfo[] infos = methodInfo.GetParameters();
                debugMessage += "Paremeters found for method: " + methodInfo.Name.Bold().Comma() + infos.Length.NewLine();
                if (infos.Length > 0) {
                    foreach (ParameterInfo info in infos) {
                        debugMessage += "Parameter name: " + info.Name + ", type: " + info.ParameterType.Name.Bold().NewLine();
                        // if parameter derives from System or UnityEngine.Object
                        if (info.ParameterType.IsSubclassOf(typeof(System.Object)) || info.ParameterType.IsSubclassOf(typeof(UnityEngine.Object))) {
                            if (info.HasDefaultValue)
                                parameters.Add(info.DefaultValue);
                            else
                                parameters.Add(null);
                            parameterTypes.Add(info.ParameterType);
                            parametersTypesNames.Add(info.ParameterType.Name);
                            parametersNames.Add(info.Name);
                        }
                        else {
                            Debug.LogError("Cannot assign method! All method's parameters types must derive either from System.Object or UnityEngine.Object. Error occured with parameter " + info.Name + " of type: " + info.ParameterType);
                            EditorGUILayout.HelpBox("Cannot assign method! All method's parameters types must derive either from System.Object or UnityEngine.Object. Error occured with parameter " + info.Name + " of type: " + info.ParameterType, MessageType.Error);
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>Construct and add the new action to the list specified</summary>
        /// <param name="actionList"></param>
        private void AddNewAction(List<SerializedAction_Instance> actionList) {
            if (selectedTimeline == ActionTimeline.OnPointerEnterInteraction && triggerObject == null) {
                EditorGUILayout.HelpBox("Trigger gameobject has not been defined", MessageType.Error);
                Debug.LogError("Trigger gameobject has not been defined");
            }
            else {
                SerializedAction_Instance action;
                action = new SerializedAction_Instance(triggerObject, objType, selectedMethodName, parameters.ToArray(), parametersNames, parametersTypesNames);
                actionList.Add(action);
                EditorUtility.SetDirty(targetInstance);
                try {
                    debugMessage += "Added new action with".NewLine() +
                        "Timeline: " + selectedTimeline.Bold().NewLine() +
                        "Trigger: " + action.TriggerInput.name.Bold().NewLine() +
                        "Type: " + action.ClassName.Bold() +
                        "Method: " + action.MethodName.Bold().NewLine() +
                        "Total parameters: " + action.Arguments.Length.Bold().NewLine();
                    for (int i = 0; i < action.Arguments.Length; i++) {
                        debugMessage += "Name: " + action.ArgumentNames[i].Bold().Comma() +
                        "Type: " + action.ArgumentTypesNames[i].Comma() +
                        "Object: " + action.Arguments[i].Bold();
                    }
                    Debug.Log(debugMessage.NewLine(2));
                    debugMessage = "";
                }
                catch (Exception ex) {
                    Debug.LogError("SerializedAction " + "Error".Colored(Color.red) + " during SerializedAction creation:".NewLine() +
                        debugMessage.NewLine(2) + "ERROR: ".Bold() + ex.Message.NewLine(2));
                    debugMessage = "";
                }

                // Reset values
                hasPressedGetMethods = false;
                hasPressedGetParameters = false;
                triggerObject = null;
                selectedMethodNameIndex = 0;
                selectedMethodName = "";
                methodsRetrieved.Clear();
                methodNames.Clear();
                parameters.Clear();
                parameterTypes.Clear();
                parametersNames.Clear();
            }
        }

        /// <summary>Draws the inspector for adding a new action to the list specified </summary>
        /// <param name="actionsList">The list to add new action</param>
        private void AddNewActionToListInspector(List<SerializedAction_Instance> actionsList) {
            if (SelectNewActionMonoScript()) {
                if (GUILayout.Button("Get methods") || hasPressedGetMethods) {
                    if (SelectNewActionMethod()) {
                        hasPressedGetMethods = true;
                        if (GUILayout.Button("Get parameters for method: " + selectedMethodName)) {
                            hasPressedGetParameters = true;
                            parameters.Clear();
                            parameterTypes.Clear();
                            parametersNames.Clear();
                        }
                        if (hasPressedGetParameters) {
                            if (SelectNewActionParameters(methodsRetrieved[selectedMethodNameIndex]))
                                GUILayout.Space(15);
                            else
                                EditorGUILayout.HelpBox("Method does not have any parameters", MessageType.Info);
                            if (GUILayout.Button("Add method for execution " + selectedMethodName))
                                AddNewAction(actionsList);
                        }
                    }
                }
            }
        }
    }
}
