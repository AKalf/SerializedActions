#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SerializedActions.Extensions;

using ActionTimeline = SerializedAction_Container.ActionTimeline;
using SerializedParameters = SerializedActions.SerializedActions_SerializedParameters;
using ListOfContainers = System.Collections.Generic.List<SerializedAction_Container>;
using MonoBehaviourManager = SerializedActions_MonobehaviourManager;

namespace SerializedActions.Editors {
    [CustomEditor(typeof(MonoBehaviourManager), true)]
    public class SerializedActions_Manager_Inspector : Editor {

        public ActionTimeline selectedTimeline = ActionTimeline.OnInteraction;
        private bool[] showParameters = null;
        private bool showDefaultInspector = false;
        public override void OnInspectorGUI() {
            showDefaultInspector = EditorGUILayout.Foldout(showDefaultInspector, "Show default inspector");
            if (showDefaultInspector)
                base.DrawDefaultInspector();
            EditorGUILayout.Space();

            MonoBehaviourManager targetInstance = (MonoBehaviourManager)target;
            SerializedObject objInstance = new SerializedObject(target);
            objInstance.Update();
            // Draw stored serialized actions
            DrawInspector(targetInstance, objInstance);
            GUILayout.Space(50);
            if (GUILayout.Button("Add new action", GUILayout.Width(100))) // Opens editor window for registering new action
                SerializedActions_NewActionEditorWindow.ShowWindow(targetInstance);
            if (GUILayout.Button("Delete all delegates")) { // Deletes all saved data
                targetInstance.OnStartActions.Clear();
                targetInstance.OnInteractionActions.Clear();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
            objInstance.Update();
            objInstance.ApplyModifiedProperties();
        }

        /// <summary>Draws the inspector of a SerializedAction data holder</summary>
        public void DrawInspector(MonoBehaviourManager targetInstance, SerializedObject serializedObj) {
            selectedTimeline = (ActionTimeline)EditorGUILayout.EnumPopup(selectedTimeline);
            ListOfContainers filteredList = null;
            ListOfContainers actualList =
                selectedTimeline == ActionTimeline.OnAwake ? targetInstance.OnAwakeActions :
                selectedTimeline == ActionTimeline.OnStart ? targetInstance.OnStartActions :
                selectedTimeline == ActionTimeline.OnEnable ? targetInstance.OnEnableActions :
                selectedTimeline == ActionTimeline.OnDisable ? targetInstance.OnDisableActions :
                targetInstance.OnInteractionActions;
            filteredList = FilterActionsBasedOnString(actualList);
            DrawInspectorForSerializedList(serializedObj, filteredList, selectedTimeline, actualList);
            GUILayout.Space(25);
            serializedObj.Update();
            serializedObj.ApplyModifiedProperties();
        }

        /// <summary>Draws the inspector for a given list.</summary>
        /// <param name="objRef">The target object reference</param>
        /// <param name="listToShow">The list containing the actions to actually display (in case filters have been applied)</param>
        /// <param name="realList">The actual list the displayed actions belong to. Is used in case the user selects to delete an action from existance</param>
        /// <param name="selectedTimeline">In case the list is "OnPointerEnterInteraction", draw field for the action's trigger object</param>
        private void DrawInspectorForSerializedList(SerializedObject objRef, ListOfContainers listToShow, ActionTimeline selectedTimeline, ListOfContainers realList) {
            EditorGUILayout.LabelField("Total actions: " + listToShow.Count, EditorStyles.largeLabel);
            GUILayout.Space(10);
            if (listToShow.Count > 0) {
                if (showParameters == null)
                    showParameters = new bool[listToShow.Count];
                for (int i = 0; i < listToShow.Count; i++) {
                    SerializedAction_Container action = listToShow[i];
                    // Draw action's trigger GameObject -------------------------------------------------------------------------------------------------------
                    if (selectedTimeline == ActionTimeline.OnInteraction) {
                        action.TriggerInput = SerializedActions_EditorDrawings.UnityObjectField<UnityEngine.Object>(action.TriggerInput, action.TriggerInput.GetType(), "Trigger: ", true, false);
                        if (action.TriggerInput.GetType().IsSubclassOf(typeof(UnityEngine.UI.Selectable)) == false)
                            action.TriggerType = (UnityEngine.EventSystems.EventTriggerType)EditorGUILayout.EnumPopup(action.TriggerType);
                    }
                    Color prev = GUI.color;
                    GUI.backgroundColor = Color.green;
                    EditorGUILayout.LabelField("Method: " + action.MethodName.Bold() + ", in class: " + action.ClassName.Bold(), InspectorElements.InfoField.Style, InspectorElements.InfoField.Options);
                    GUI.backgroundColor = prev;
                    // Draw paremeter fields -------------------------------------------------------------------------------------------------------
                    if (action.Parameters != null && action.Parameters.Count > 0) {
                        if (i < showParameters.Length && (showParameters[i] = EditorGUILayout.Foldout(showParameters[i], "Parameters", true)))
                            DrawArguments(action);
                    }
                    else
                        EditorGUILayout.LabelField("Total parameters: 0");
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    // Check action for erros -------------------------------------------------------------------------------------------------------
                    if (GUILayout.Button("Check action", EditorStyles.miniButtonRight, GUILayout.Width(100))) {
                        UnitTests.SerializedActions_UnitTestForParameters.Instance().CheckSingleAction(action, target as MonoBehaviourManager);
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

        /// <summary>Draw all the arguments of an action</summary>
        private void DrawArguments(SerializedAction_Container action) {
            EditorGUILayout.LabelField("Total arguments deserialized: " + action.Parameters.Count);
            bool areThereChanges = false;
            for (int paramIndex = 0; paramIndex < action.Parameters.Count; paramIndex++) {
                SerializedParameters param = action.Parameters[paramIndex];
                if (param.ParameterType != null) { // Draw argument to inspector
                    object oldArg = action.Parameters[paramIndex].Value;
                    object newValue = SerializedActions_EditorDrawings.DrawField(param.Value, param.ParameterType, param.ParameterName);
                    if (newValue != null && oldArg != null && oldArg.Equals(newValue) == false) {
                        param.Value = newValue;
                        param.ParameterTypeName = param.ParameterType.Name;
                        areThereChanges = true;
                    }
                }
                if (areThereChanges) {
                    action.Parameters[paramIndex].Value = action.Parameters[paramIndex].Value;
                    EditorUtility.SetDirty(target);
                }
            }
        }

        /// <summary>
        /// Draws Editor.TextFields for input and searches for serialized actions in a list that meet the criteria.
        /// The user can search either by action's trigger name or action's method name.
        /// </summary>
        /// <param name="actionsToFilter">The list to search</param>
        /// <returns>Returns a list with all SerializedActions that meet he criteria </returns>
        private static ListOfContainers FilterActionsBasedOnString(ListOfContainers actionsToFilter) {
            List<SerializedAction_Container> actionsToShow = actionsToFilter;
            string triggerSearchString = EditorGUILayout.TextField("Search actions by trigger name: ", SerializedActionSearch.TriggerSearchString);
            string methodSearchString = EditorGUILayout.TextField("Search actions by method name: ", SerializedActionSearch.MethodSearchString);
            actionsToShow = SerializedActionSearch.GetActionsWithTriggerName(actionsToFilter, triggerSearchString);
            actionsToShow = SerializedActionSearch.GetMethodsWithSearchKey(actionsToShow, methodSearchString);
            return actionsToShow;
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
            public static ListOfContainers GetActionsWithTriggerName(ListOfContainers actions, string searchString) {
                triggerSearchString = searchString;
                if (string.IsNullOrEmpty(triggerSearchString))
                    return actions;
                List<SerializedAction_Container> results = new List<SerializedAction_Container>();
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
            public static ListOfContainers GetMethodsWithSearchKey(ListOfContainers actions, string searchString) {
                methodSearchString = searchString;
                if (string.IsNullOrEmpty(methodSearchString))
                    return actions;
                List<SerializedAction_Container> results = new List<SerializedAction_Container>();
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
