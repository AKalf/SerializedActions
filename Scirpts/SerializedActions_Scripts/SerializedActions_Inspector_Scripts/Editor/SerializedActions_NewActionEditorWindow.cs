#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using SerializedActions.Extensions;
using ActionTimeline = SerializedAction_Container.ActionTimeline;
using SerializedParameters = SerializedActions.SerializedActions_SerializedParameters;
using ActionContainer = SerializedAction_Container;
namespace SerializedActions.Editors {
    public class SerializedActions_NewActionEditorWindow : EditorWindow {

        /// <summary>The timeline to trigger this action </summary>
        private ActionTimeline selectedTimeline = ActionTimeline.OnInteraction;
        private string debugMessage = "";
        private GUIStyle richTextStyle = new GUIStyle();
        private SerializedActions_MonobehaviourManager targetInstance = null;
        private static SerializedActions_NewActionEditorWindow window = null;
        /// <summary>The UI element which will trig the action </summary>
        private UnityEngine.Object triggerObject = null;
        /// <summary>A helping field that holds the object the user selected to retrieve methods from</summary>
        private MonoScript monoscript = null;
        /// <summary>The type user selected to retreive methods</summary>
        private System.Type type = null;
        /// <summary> /// The index of the selected method in the available methods list /// </summary>
        private int selectedMethodNameIndex = 0;
        /// <summary>/// Methods reflection representetion for script ///</summary>
        private List<MethodInfo> methodsRetrieved = new List<MethodInfo>();
        /// <summary>/// Names of availble functions to trigger ///</summary>
        private List<string> methodNames = new List<string>();
        /// <summary> /// The parameters of the action to be created /// </summary>
        private List<SerializedParameters> parameters = new List<SerializedParameters>();

        private UnityEngine.EventSystems.EventTriggerType triggerType = UnityEngine.EventSystems.EventTriggerType.PointerEnter;

        private bool hasPressedGetMethods = false;
        private bool hasPressedGetParameters = false;

        [MenuItem("Window/Serialized Actions/New Action")]
        public static void ShowWindow() {
            window = EditorWindow.GetWindow(typeof(SerializedActions_NewActionEditorWindow)) as SerializedActions_NewActionEditorWindow;
            window.richTextStyle.richText = true;
            window.richTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
        }
        public static void ShowWindow(SerializedActions_MonobehaviourManager target) {
            SerializedActions_NewActionEditorWindow window = EditorWindow.GetWindow(typeof(SerializedActions_NewActionEditorWindow)) as SerializedActions_NewActionEditorWindow;
            window.targetInstance = target;
            window.richTextStyle.richText = true;
            window.richTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
        }

        void OnGUI() {
            if (targetInstance != null) {
                targetInstance = SerializedActions_EditorDrawings.UnityObjectField<SerializedActions_MonobehaviourManager>(targetInstance, targetInstance.GetType(), "Target behaviour", true, false);
                selectedTimeline = (ActionTimeline)EditorGUILayout.EnumPopup(selectedTimeline);
                debugMessage = "Selected timeline for new action: " + selectedTimeline.Bold();
                if (selectedTimeline == ActionTimeline.OnInteraction)
                    SelectNewActionTrigger();
                // Check selected timeline and draw corresponding inspector for list
                DrawNewActionInspectorForList(
                    selectedTimeline == ActionTimeline.OnAwake ? targetInstance.OnAwakeActions :        // Case is OnAwake 
                    selectedTimeline == ActionTimeline.OnStart ? targetInstance.OnStartActions :        // Case is OnStart 
                    selectedTimeline == ActionTimeline.OnEnable ? targetInstance.OnEnableActions :      // Case is OnEnable 
                    selectedTimeline == ActionTimeline.OnDisable ? targetInstance.OnDisableActions :    // Case is OnEnable 
                    targetInstance.OnInteractionActions);                                               // Case is on object click
            }
            else
                targetInstance = SerializedActions_EditorDrawings.UnityObjectField<SerializedActions_MonobehaviourManager>(null, typeof(UnityEngine.Object), "Target behaviour", true, false);
        }

        /// <summary>Draw the field for the trigger object </summary>
        /// <returns>Returns true if the field value wasnt null</returns>
        private bool SelectNewActionTrigger() {
            EditorGUILayout.LabelField("New action".Bold(), richTextStyle, GUILayout.Height(20));
            // Set the UI element that will trigger the action
            triggerObject = SerializedActions_EditorDrawings.DrawField(triggerObject, typeof(UnityEngine.Object), "New Input: ") as UnityEngine.Object;
            if (triggerObject != null) {
                Selectable selectable = null;
                if (triggerObject.GetType().Equals(typeof(GameObject))) {
                    selectable = ((GameObject)triggerObject).GetComponent<Selectable>();
                    if (selectable != null)
                        triggerObject = selectable;
                }
                debugMessage += "\nTrigger object selected: " + triggerObject.name.Bold();
                if (triggerObject.GetType().IsSubclassOf(typeof(UnityEngine.UI.Selectable)) == false)
                    triggerType = (UnityEngine.EventSystems.EventTriggerType)EditorGUILayout.EnumPopup(triggerType);
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
            monoscript = SerializedActions_EditorDrawings.UnityObjectField<MonoScript>(monoscript, typeof(MonoScript), "Script with class: ", true, false);
            if (monoscript != null) { // if object has been assigned
                type = monoscript.GetClass(); // Get the class of the monoscript assigned
                debugMessage += "Type selected: " + type?.Bold().NewLine();
            }
            if (type != null) {
                GUILayout.TextArea("Type selected: " + type?.Bold().NewLine(), richTextStyle);
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
                int prevValue = selectedMethodNameIndex;
                selectedMethodNameIndex = EditorGUILayout.Popup("Available methods: " + methodNames.Count, selectedMethodNameIndex, methodNames.ToArray());
                if (prevValue != selectedMethodNameIndex)
                    hasPressedGetParameters = false;
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
                foreach (SerializedParameters param in parameters) {
                    object oldValue = param.Value;
                    object newValue = SerializedActions_EditorDrawings.DrawField(param.Value, param.ParameterType, param.ParameterName, shouldShowName: true, shouldShowType: false);
                    if (oldValue != newValue) {
                        param.Value = newValue;
                        param.ParameterType = newValue.GetType();
                        param.ParameterTypeName = param.ParameterType.Name;
                        EditorUtility.SetDirty(targetInstance);
                    }
                }
                return true;
            }
            else {
                parameters.Clear();
                // Method parameters signature
                ParameterInfo[] infos = methodInfo.GetParameters();
                debugMessage += "Paremeters found for method: " + methodInfo.Name.Bold().Comma() + infos.Length.NewLine();
                if (infos.Length > 0) {
                    foreach (ParameterInfo info in infos) {
                        // if parameter derives from System or UnityEngine.Object
                        if (info.ParameterType.IsPrimitiveType() || info.ParameterType.IsUnityObjectType()) {
                            Debugs.SerializedActions_Debugs.DebugParameterWithDefaultValue(info, ref debugMessage);
                            SerializedParameters newParameter = SerializedParameters.CreateSerializedParameter(
                                info.Name, info.ParameterType, info.ParameterType.GetDefaultValue());
                            parameters.Add(newParameter);
                        }
                        else {
                            Debug.LogError("Cannot assign method! All method's parameters types must derive either from System.Object or UnityEngine.Object. Error occured with parameter " + info.Name + " of type: " + info.ParameterType);
                            EditorGUILayout.HelpBox("Cannot assign method! All method's parameters types must derive either from System.Object or UnityEngine.Object. Error occured with parameter " + info.Name + " of type: " + info.ParameterType, MessageType.Error);
                        }
                    }
                }
                Debug.Log(debugMessage.NewLine(2));
                return false;
            }
        }

        /// <summary>Construct and add the new action to the list specified</summary>
        /// <param name="actionList"></param>
        private void RegisterAction(List<ActionContainer> actionList) {
            if (selectedTimeline == ActionTimeline.OnInteraction && triggerObject == null) {
                EditorGUILayout.HelpBox("Trigger gameobject has not been defined", MessageType.Error);
                Debug.LogError("Trigger gameobject has not been defined");
            }
            else {
                ActionContainer action;
                action = new ActionContainer(triggerObject, monoscript, methodNames[selectedMethodNameIndex], parameters);
                actionList.Add(action);
                SerializedActions.UnitTests.SerializedActions_MethodsRegisters.Instance().AddAction(targetInstance, action, monoscript, methodsRetrieved[selectedMethodNameIndex]);
                EditorUtility.SetDirty(targetInstance);
                Debugs.SerializedActions_Debugs.DebugRegisterNewAction(action, selectedTimeline, ref debugMessage);
                Debug.Log(debugMessage.NewLine(2));
                debugMessage = "";

                // Reset values
                hasPressedGetMethods = false;
                hasPressedGetParameters = false;
                triggerObject = null;
                selectedMethodNameIndex = 0;
                methodsRetrieved.Clear();
                methodNames.Clear();
                parameters.Clear();
                Repaint();
            }
        }

        /// <summary>Draws the inspector for adding a new action to the list specified </summary>
        /// <param name="actionsList">The list to add new action</param>
        private void DrawNewActionInspectorForList(List<ActionContainer> actionsList) {
            if (SelectNewActionMonoScript()) {
                if (GUILayout.Button("Get methods") || hasPressedGetMethods) {
                    if (SelectNewActionMethod()) {
                        hasPressedGetMethods = true;
                        if (GUILayout.Button("Get parameters for method: " + methodNames[selectedMethodNameIndex])) {
                            hasPressedGetParameters = true;
                            parameters.Clear();
                        }
                        if (hasPressedGetParameters) {
                            if (SelectNewActionParameters(methodsRetrieved[selectedMethodNameIndex]))
                                GUILayout.Space(15);
                            else
                                EditorGUILayout.HelpBox("Method does not have any parameters", MessageType.Info);
                            if (GUILayout.Button("Add method for execution " + methodNames[selectedMethodNameIndex]))
                                RegisterAction(actionsList);
                        }
                    }
                }
            }
        }
    }
}
#endif