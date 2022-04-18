#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using ActionTimeline = SerializedAction.ActionTimeline;
using ActionsHolder = SerializedActions_MonoBehaviourHolder;

[CustomEditor(typeof(ActionsHolder), true)]
public class BaseImplementation_Inspector : Editor {

    public ActionTimeline selectedTimeline = ActionTimeline.OnPointerEnterInteraction;
    private bool[] showParameters = null;

    private bool showDefaultInspector = false;
    public override void OnInspectorGUI() {
        showDefaultInspector = EditorGUILayout.Foldout(showDefaultInspector, "Show default inspector");
        if (showDefaultInspector)
            base.DrawDefaultInspector();
        EditorGUILayout.Space();

        ActionsHolder targetInstance = (ActionsHolder)target;
        SerializedObject objInstance = new SerializedObject(target);
        objInstance.Update();

        // Draw stored serialized actions
        DrawInspector(targetInstance, objInstance);
        GUILayout.Space(50);
        // Open editor window for registering new action
        if (GUILayout.Button("Add new action", GUILayout.Width(100)))
            NewAction_EditorWindow.ShowWindow(targetInstance);
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
    public void DrawInspector(ActionsHolder targetInstance, SerializedObject serializedObj) {
        selectedTimeline = (ActionTimeline)EditorGUILayout.EnumPopup(selectedTimeline);
        List<SerializedAction> actionsToShow = null;
        switch (selectedTimeline) {
            case ActionTimeline.OnAwake:
                actionsToShow = FilterActionsBasedOnString(targetInstance.OnAwakeActions);
                DrawInspectorForSerializedList(serializedObj, actionsToShow, targetInstance.OnAwakeActions, selectedTimeline);
                break;
            case ActionTimeline.OnStart:
                actionsToShow = FilterActionsBasedOnString(targetInstance.OnStartActions);
                DrawInspectorForSerializedList(serializedObj, actionsToShow, targetInstance.OnStartActions, selectedTimeline);
                break;
            case ActionTimeline.OnEnable:
                actionsToShow = FilterActionsBasedOnString(targetInstance.OnEnableActions);
                DrawInspectorForSerializedList(serializedObj, actionsToShow, targetInstance.OnEnableActions, selectedTimeline);
                break;
            case ActionTimeline.OnDisable:
                actionsToShow = FilterActionsBasedOnString(targetInstance.OnDisableActions);
                DrawInspectorForSerializedList(serializedObj, actionsToShow, targetInstance.OnDisableActions, selectedTimeline);
                break;
            case ActionTimeline.OnPointerEnterInteraction:
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
    private static List<SerializedAction> FilterActionsBasedOnString(List<SerializedAction> actionsToFilter) {
        List<SerializedAction> actionsToShow = actionsToFilter;
        string triggerSearchString = EditorGUILayout.TextField("Search actions by trigger name: ", SerializedActionSearch.TriggerSearchString);
        string methodSearchString = EditorGUILayout.TextField("Search actions by method name: ", SerializedActionSearch.MethodSearchString);
        actionsToShow = SerializedActionSearch.GetActionsWithTriggerName(actionsToFilter, triggerSearchString);
        actionsToShow = SerializedActionSearch.GetMethodsWithSearchKey(actionsToShow, methodSearchString);
        return actionsToShow;
    }

    private void DrawArguments(SerializedAction action) {
        System.Object[] args = action.XmlDeserializeFromString(action.serializedArray); // Get arguments values from XML
        bool areThereChanges = false;
        if (args.Length != action.unityArguments.Length)
            Debug.LogError("Unity arguments length does not equal deserialized arguments legth for action with method <b>" + action.methodName + "</b>");
        for (int i = 0; i < action.unityArguments.Length; i++) {
            if (args[i] == null)
                args[i] = action.unityArguments[i]; // Populate array with the values from deserialization
        }
        EditorGUILayout.LabelField("Total arguments deserialized: " + args.Length);
        for (int paramIndex = 0; paramIndex < args.Length; paramIndex++) {
            string paramName = "";
            if (action.argumentNames != null && paramIndex < action.argumentNames.Count)
                paramName = action.argumentNames[paramIndex]; // Get name from seriliazed names in action object
            else if (args[paramIndex] != null)
                paramName = args[paramIndex].ToString(); // if couldnt retrieve from action's list, assign value.ToString()
            Type argType = FindPrimitiveType(action.argumentTypesNames[paramIndex]); // Try to find type as primitive
            if (argType == null) // so it is not primitive
                argType = SerializedAction.GetType(action.argumentTypesNames[paramIndex]); // any other case
            System.Object oldArg = args[paramIndex];

            if (args[paramIndex] == null) {   // Argument has not been defined
                args[paramIndex] = EditorDrawing.UnityObjectField<UnityEngine.Object>(
                    args[paramIndex] as UnityEngine.Object, // The object value
                    SerializedAction.GetType(action.argumentTypesNames[paramIndex]), // Retrieve type for serialized name
                    paramName); // Parameter name
                continue;
            }
            // Argument is type of UnityEngine.Object
            else if (argType != null && (argType.IsSubclassOf(typeof(UnityEngine.Object)) || argType == typeof(UnityEngine.Object))) {
                args[paramIndex] = EditorDrawing.UnityObjectField<UnityEngine.Object>(args[paramIndex] as UnityEngine.Object, argType, paramName);
                if (oldArg != args[paramIndex])
                    areThereChanges = true;
            }
            // Argument is type of Component
            else if (argType != null && (argType.IsSubclassOf(typeof(Component)) || argType == typeof(Component))) {
                args[paramIndex] = EditorDrawing.UnityObjectField<Component>(args[paramIndex] as Component, argType, paramName);
                if (oldArg != args[paramIndex])
                    areThereChanges = true;
            }
            // Argument is type of Primitive
            else {
                args[paramIndex] = EditorDrawing.PrimitiveField(args[paramIndex], argType, paramName);
                if (oldArg.Equals(args[paramIndex]) == false)
                    areThereChanges = true;
            }
            if (areThereChanges) {
                if (action.arguments == null)
                    action.arguments = new System.Object[args.Length];
                if (action.unityArguments == null)
                    action.unityArguments = new UnityEngine.Object[args.Length];
                if (argType != null && (argType == typeof(UnityEngine.Object) || argType.IsSubclassOf(typeof(UnityEngine.Object))))
                    action.unityArguments[paramIndex] = args[paramIndex] as UnityEngine.Object;
                else {
                    action.arguments[paramIndex] = args[paramIndex];
                    action.serializedArray = action.XmlSerializeToString(action.arguments);
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
        List<SerializedAction> listToShow, List<SerializedAction> realList,
        ActionTimeline selectedTimeline) {
        EditorGUILayout.LabelField("Total actions: " + listToShow.Count, EditorStyles.largeLabel);
        GUILayout.Space(10);
        if (listToShow.Count > 0) {
            if (showParameters == null)
                showParameters = new bool[listToShow.Count];
            for (int i = 0; i < listToShow.Count; i++) {
                SerializedAction action = listToShow[i];
                // Draw action's trigger GameObject -------------------------------------------------------------------------------------------------------
                if (selectedTimeline == ActionTimeline.OnPointerEnterInteraction) {
                    action.triggerInput = EditorDrawing.UnityObjectField<UnityEngine.Object>(action.triggerInput, action.triggerInput.GetType(), "Trigger: ", true, false);
                    if (action.triggerInput.GetType().IsSubclassOf(typeof(UnityEngine.UI.Selectable)) == false)
                        action.triggerType = (UnityEngine.EventSystems.EventTriggerType)EditorGUILayout.EnumPopup(action.triggerType);
                }
                Color prev = GUI.color;
                GUI.backgroundColor = Color.green;
                EditorGUILayout.LabelField("Method: <b>" + action.methodName + "</b>, in class: <b>" + action.ClassName + "</b>", SerializedActions_InspectorElements.InfoField.Style, SerializedActions_InspectorElements.InfoField.Options);
                GUI.backgroundColor = prev;
                // Draw paremeter fields -------------------------------------------------------------------------------------------------------
                if (action.unityArguments != null && action.unityArguments.Length > 0 || (action.arguments != null && action.arguments.Length > 0)) {
                    if (i < showParameters.Length && (showParameters[i] = EditorGUILayout.Foldout(showParameters[i], "Parameters", true)))
                        DrawArguments(action);
                }
                else
                    EditorGUILayout.LabelField("Total parameters: 0");
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                // Check action for erros -------------------------------------------------------------------------------------------------------
                if (GUILayout.Button("Check action", EditorStyles.miniButtonRight, GUILayout.Width(100))) {
                    SerializedAction_Parameters_UnitTest.Instance().CheckSingleAction(action, target as ActionsHolder);
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
                SerializedActions_InspectorElements.HorizontalLine(Color.black, 1, 1000, new Vector2(15, 10));
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
        public static List<SerializedAction> GetActionsWithTriggerName(List<SerializedAction> actions, string searchString) {
            triggerSearchString = searchString;
            if (string.IsNullOrEmpty(triggerSearchString))
                return actions;
            List<SerializedAction> results = new List<SerializedAction>();
            for (int i = 0; i < actions.Count; i++) {
                if (actions[i].triggerInput.name.ToLower().Contains(searchString.ToLower()))
                    results.Add(actions[i]);
            }
            return results;
        }
        /// <summary>Searches for actions whose method's name contains provided string </summary>
        /// <param name="actions">The list of actions to search in</param>
        /// <param name="searchString">The string to check if contained in actions' method name</param>
        /// <returns>Returns a list of actions that met the criteria</returns>
        public static List<SerializedAction> GetMethodsWithSearchKey(List<SerializedAction> actions, string searchString) {
            methodSearchString = searchString;
            if (string.IsNullOrEmpty(methodSearchString))
                return actions;
            List<SerializedAction> results = new List<SerializedAction>();
            for (int i = 0; i < actions.Count; i++) {
                if (actions[i].methodName.ToLower().Contains(searchString.ToLower())) {
                    results.Add(actions[i]);
                }
            }
            return results;
        }
    }
}



#endif
