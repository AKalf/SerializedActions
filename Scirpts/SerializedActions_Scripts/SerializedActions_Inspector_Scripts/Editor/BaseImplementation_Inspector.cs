#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using ActionTimeline = SerializedAction_Instance.ActionTimeline;

namespace SerializedActions.Editors {
    [CustomEditor(typeof(SerializedActionsManager), true)]
    public class BaseImplementation_Inspector : Editor {

        public ActionTimeline selectedTimeline = ActionTimeline.OnPointerEnterInteraction;
        private bool[] showParameters = null;

        private bool showDefaultInspector = false;
        public override void OnInspectorGUI() {
            showDefaultInspector = EditorGUILayout.Foldout(showDefaultInspector, "Show default inspector");
            if (showDefaultInspector)
                base.DrawDefaultInspector();
            EditorGUILayout.Space();

            SerializedActionsManager targetInstance = (SerializedActionsManager)target;
            SerializedObject objInstance = new SerializedObject(target);
            objInstance.Update();

            // Draw stored serialized actions
            DrawInspector(targetInstance, objInstance);
            GUILayout.Space(50);
            // Open editor window for registering new action
            if (GUILayout.Button("Add new action", GUILayout.Width(100)))
                SerializedActions_NewActionEditorWindow.ShowWindow(targetInstance);
            // Delete all saved data
            if (GUILayout.Button("Delete all delegates")) {
                targetInstance.OnStartActions.Clear();
                targetInstance.OnPointEnterActions.Clear();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
            objInstance.Update();
            objInstance.ApplyModifiedProperties();
        }

        /// <summary>Draws the inspector of a SerializedAction data holder</summary>
        public void DrawInspector(SerializedActionsManager targetInstance, SerializedObject serializedObj) {
            selectedTimeline = (ActionTimeline)EditorGUILayout.EnumPopup(selectedTimeline);
            List<SerializedAction_Instance> actionsToShow = null;
            switch (selectedTimeline) {
                case ActionTimeline.OnAwake: // On Awake actions
                    actionsToShow = FilterActionsBasedOnString(targetInstance.OnAwakeActions);
                    DrawInspectorForSerializedList(serializedObj, actionsToShow, targetInstance.OnAwakeActions, selectedTimeline);
                    break;
                case ActionTimeline.OnStart: // On Start actions
                    actionsToShow = FilterActionsBasedOnString(targetInstance.OnStartActions);
                    DrawInspectorForSerializedList(serializedObj, actionsToShow, targetInstance.OnStartActions, selectedTimeline);
                    break;
                case ActionTimeline.OnEnable: // On Enable actions
                    actionsToShow = FilterActionsBasedOnString(targetInstance.OnEnableActions);
                    DrawInspectorForSerializedList(serializedObj, actionsToShow, targetInstance.OnEnableActions, selectedTimeline);
                    break;
                case ActionTimeline.OnDisable: // On Disable actions
                    actionsToShow = FilterActionsBasedOnString(targetInstance.OnDisableActions);
                    DrawInspectorForSerializedList(serializedObj, actionsToShow, targetInstance.OnDisableActions, selectedTimeline);
                    break;
                case ActionTimeline.OnPointerEnterInteraction: // On PointerEnter actions
                    actionsToShow = FilterActionsBasedOnString(targetInstance.OnPointEnterActions);
                    DrawInspectorForSerializedList(serializedObj, actionsToShow, targetInstance.OnPointEnterActions, selectedTimeline);
                    break;
            }
            GUILayout.Space(25);
            serializedObj.Update();
            serializedObj.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws Editor.TextFields for input and searches for serialized actions in a list that meet the criteria.
        /// The user can search either by action's trigger name or action's method name.
        /// </summary>
        /// <param name="actionsToFilter">The list to search</param>
        /// <returns>Returns a list with all SerializedActions that meet he criteria </returns>
        private static List<SerializedAction_Instance> FilterActionsBasedOnString(List<SerializedAction_Instance> actionsToFilter) {
            List<SerializedAction_Instance> actionsToShow = actionsToFilter;
            string triggerSearchString = EditorGUILayout.TextField("Search actions by trigger name: ", SerializedActionSearch.TriggerSearchString);
            string methodSearchString = EditorGUILayout.TextField("Search actions by method name: ", SerializedActionSearch.MethodSearchString);
            actionsToShow = SerializedActionSearch.GetActionsWithTriggerName(actionsToFilter, triggerSearchString);
            actionsToShow = SerializedActionSearch.GetMethodsWithSearchKey(actionsToShow, methodSearchString);
            return actionsToShow;
        }
        /// <summary>Draw all the arguments of an action</summary>
        private void DrawArguments(SerializedAction_Instance action) {
            System.Object[] args = action.XmlDeserializeFromString(action.SerializedArray); // Get arguments values from XML
            bool areThereChanges = false;
            if (args.Length != action.UnityArguments.Length)
                Debug.LogError("Unity arguments length does not equal deserialized arguments legth for action with method " + action.MethodName.Bold());
            for (int i = 0; i < action.UnityArguments.Length; i++) {
                if (args[i] == null)
                    args[i] = action.UnityArguments[i]; // Populate array with the values from deserialization
            }
            EditorGUILayout.LabelField("Total arguments deserialized: " + args.Length);
            for (int paramIndex = 0; paramIndex < args.Length; paramIndex++) {
                string paramName = "";
                if (action.ArgumentNames != null && paramIndex < action.ArgumentNames.Count)
                    paramName = action.ArgumentNames[paramIndex]; // Get name from seriliazed names in action object
                else if (args[paramIndex] != null)
                    paramName = args[paramIndex].ToString(); // if couldnt retrieve from action's list, assign value.ToString()
                Type argType = FindPrimitiveType(action.ArgumentTypesNames[paramIndex]); // Try to find type as primitive
                if (argType == null) // so it is not primitive
                    argType = SerializedAction_Instance.GetType(action.ArgumentTypesNames[paramIndex]); // any other case
                if (argType != null) { // Draw argument to inspector
                    System.Object oldArg = args[paramIndex];
                    args[paramIndex] = Drawings.DrawArgumentIfType(args[paramIndex], argType, paramName);
                    if (oldArg != args[paramIndex])
                        areThereChanges = true;
                }
                if (areThereChanges) {
                    if (action.Arguments == null)
                        action.Arguments = new System.Object[args.Length];
                    if (action.UnityArguments == null)
                        action.UnityArguments = new UnityEngine.Object[args.Length];
                    if (argType != null && (argType == typeof(UnityEngine.Object) || argType.IsSubclassOf(typeof(UnityEngine.Object))))
                        action.UnityArguments[paramIndex] = args[paramIndex] as UnityEngine.Object;
                    else {
                        action.Arguments[paramIndex] = args[paramIndex];
                        action.SerializedArray = action.XmlSerializeToString(action.Arguments);
                    }
                    EditorUtility.SetDirty(target);
                }

            }

        }

        /// <summary>Searches for primitive type whose name matches provided string</summary>
        /// <returns>Returns the primitive type whose name matches provided string</returns>
        private Type FindPrimitiveType(string typeInString) {
            if (typeInString == typeof(int).Name)
                return typeof(int);
            else if (typeInString == typeof(float).Name)
                return typeof(float);
            else if (typeInString == typeof(string).Name)
                return typeof(string);
            else if (typeInString == typeof(bool).Name)
                return typeof(bool);
            else {
                return null;
            }
        }
        /// <summary>Draws the inspector for a given list.</summary>
        /// <param name="objRef">The target object reference</param>
        /// <param name="listToShow">The list containing the actions to actually display (in case filters have been applied)</param>
        /// <param name="realList">The actual list the displayed actions belong to. Is used in case the user selects to delete an action from existance</param>
        /// <param name="selectedTimeline">In case the list is "OnPointerEnterInteraction", draw field for the action's trigger object</param>
        private void DrawInspectorForSerializedList(SerializedObject objRef,
            List<SerializedAction_Instance> listToShow, List<SerializedAction_Instance> realList,
            ActionTimeline selectedTimeline) {
            EditorGUILayout.LabelField("Total actions: " + listToShow.Count, EditorStyles.largeLabel);
            GUILayout.Space(10);
            if (listToShow.Count > 0) {
                if (showParameters == null)
                    showParameters = new bool[listToShow.Count];
                for (int i = 0; i < listToShow.Count; i++) {
                    SerializedAction_Instance action = listToShow[i];
                    // Draw action's trigger GameObject -------------------------------------------------------------------------------------------------------
                    if (selectedTimeline == ActionTimeline.OnPointerEnterInteraction) {
                        action.TriggerInput = Drawings.UnityObjectField<UnityEngine.Object>(action.TriggerInput, action.TriggerInput.GetType(), "Trigger: ", true, false);
                        if (action.TriggerInput.GetType().IsSubclassOf(typeof(UnityEngine.UI.Selectable)) == false)
                            action.TriggerType = (UnityEngine.EventSystems.EventTriggerType)EditorGUILayout.EnumPopup(action.TriggerType);
                    }
                    Color prev = GUI.color;
                    GUI.backgroundColor = Color.green;
                    EditorGUILayout.LabelField("Method: " + action.MethodName.Bold() + ", in class: " + action.ClassName.Bold(), InspectorElements.InfoField.Style, InspectorElements.InfoField.Options);
                    GUI.backgroundColor = prev;
                    // Draw paremeter fields -------------------------------------------------------------------------------------------------------
                    if (action.UnityArguments != null && action.UnityArguments.Length > 0 || (action.Arguments != null && action.Arguments.Length > 0)) {
                        if (i < showParameters.Length && (showParameters[i] = EditorGUILayout.Foldout(showParameters[i], "Parameters", true)))
                            DrawArguments(action);
                    }
                    else
                        EditorGUILayout.LabelField("Total parameters: 0");
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    // Check action for erros -------------------------------------------------------------------------------------------------------
                    if (GUILayout.Button("Check action", EditorStyles.miniButtonRight, GUILayout.Width(100))) {
                        UnitTests.SerializedParametersTest.Instance().CheckSingleAction(action, target as SerializedActionsManager);
                        EditorUtility.SetDirty(objRef.targetObject);
                    }
                    GUILayout.FlexibleSpace();
                    prev = GUI.color;
                    GUI.backgroundColor = new Color(2, 0.0f, 0);
                    // Delete action -----------------------------------------------------------------------------------------------------------------
                    if (GUILayout.Button("Delete action", EditorStyles.miniButtonRight, GUILayout.Width(150))) {
                        realList.Remove(action);
                        EditorUtility.SetDirty(objRef.targetObject);
                    }
                    GUI.backgroundColor = prev;
                    EditorGUILayout.EndHorizontal();
                    InspectorElements.HorizontalLine(Color.black, 1, 1000, new Vector2(15, 10));
                }
            }
        }

        /// <summary>Helping class to filter lists</summary>
        private static class SerializedActionSearch {

            private static string triggerSearchString = "", methodSearchString = "";
            public static string TriggerSearchString { get => triggerSearchString; }
            public static string MethodSearchString { get => methodSearchString; }
            /// <summary>Searches for actions whose trigger gameobject's name contains provided string </summary>
            /// <param name="actions">The list of actions to search in</param>
            /// <param name="searchString">The string to check if contained in actions' triggers</param>
            /// <returns>Returns a list of actions that met the criteria</returns>
            public static List<SerializedAction_Instance> GetActionsWithTriggerName(List<SerializedAction_Instance> actions, string searchString) {
                triggerSearchString = searchString;
                if (string.IsNullOrEmpty(triggerSearchString))
                    return actions;
                List<SerializedAction_Instance> results = new List<SerializedAction_Instance>();
                for (int i = 0; i < actions.Count; i++) {
                    if (actions[i].TriggerInput.name.ToLower().Contains(searchString.ToLower()))
                        results.Add(actions[i]);
                }
                return results;
            }
            /// <summary>Searches for actions whose method's name contains provided string </summary>
            /// <param name="actions">The list of actions to search in</param>
            /// <param name="searchString">The string to check if contained in actions' method name</param>
            /// <returns>Returns a list of actions that met the criteria</returns>
            public static List<SerializedAction_Instance> GetMethodsWithSearchKey(List<SerializedAction_Instance> actions, string searchString) {
                methodSearchString = searchString;
                if (string.IsNullOrEmpty(methodSearchString))
                    return actions;
                List<SerializedAction_Instance> results = new List<SerializedAction_Instance>();
                for (int i = 0; i < actions.Count; i++) {
                    if (actions[i].MethodName.ToLower().Contains(searchString.ToLower())) {
                        results.Add(actions[i]);
                    }
                }
                return results;
            }
        }
    }
}
#endif
