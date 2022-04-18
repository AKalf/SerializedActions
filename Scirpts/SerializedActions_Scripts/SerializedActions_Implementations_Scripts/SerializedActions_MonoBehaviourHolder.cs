using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>Derive your class from this in order to make it compatible with SerializedActions Inspector </summary>
public class SerializedActions_MonoBehaviourHolder : MonoBehaviour {
    [SerializeField]
    public List<SerializedAction>
        OnAwakeActions = new List<SerializedAction>(),
        OnStartActions = new List<SerializedAction>(),
        OnEnableActions = new List<SerializedAction>(),
        OnDisableActions = new List<SerializedAction>(),
        OnPointEnterActions = new List<SerializedAction>();

    private string debugMessage;

    // Unity Callbacks
    #region Unity Callbacks
    private void Awake() {
        OnAwake();
    }
    private void Start() {
        OnStart();
    }
    #endregion

    protected void AssignPointerEnterActionsToGameObjects() {
        // Assign SerialisedActions to Selectables
        foreach (SerializedAction action in OnPointEnterActions) {
            if (action.triggerInput.GetType().IsSubclassOf(typeof(Selectable)))
                SetSelectableTrigger(action);
            else
                SetGameObjectTrigger(action);
        }
#if UNITY_EDITOR
        debugMessage = "";
#endif
    }

    /// <summary>Invokes actions to be trigger at Awake() </summary>
    protected void OnAwake() {
        if (OnAwakeActions != null && OnAwakeActions.Count > 0)
            InvokeSerialisedAction(OnAwakeActions, "OnAwake");
    }
    /// <summary>Invokes actions to be trigger at Start() </summary>
    protected void OnStart() {
        // Invoke OnStart serialised actions
        if (OnStartActions != null && OnStartActions.Count > 0)
            InvokeSerialisedAction(OnStartActions, "OnStart");
        if (OnPointEnterActions != null && OnPointEnterActions.Count > 0)
            AssignPointerEnterActionsToGameObjects();
    }

    private void InvokeSerialisedAction(List<SerializedAction> actions, string timeline) {
        if (actions != null && actions.Count > 0) {
            foreach (SerializedAction action in actions) {
#if UNITY_EDITOR
                Debug_ActionDeserialisation(action, timeline);
#endif
                action.GetAction(timeline).Invoke();
            }
        }
        else
            actions = null;
    }
    private void SetGameObjectTrigger(SerializedAction action) {
        GameObject triggerAsG = null;
#if UNITY_EDITOR
        AddDebug_ActionTrigger(action, triggerAsG);
        try {
#endif
            if (action.triggerInput.GetType() == typeof(GameObject))
                triggerAsG = action.triggerInput as GameObject;
            else if (action.triggerInput.GetType().IsSubclassOf(typeof(MonoBehaviour)))
                triggerAsG = ((MonoBehaviour)action.triggerInput).gameObject;
            else if (action.triggerInput.GetType().IsSubclassOf(typeof(Component)))
                triggerAsG = ((Component)action.triggerInput).gameObject;

            EventTrigger trigger = triggerAsG.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = triggerAsG.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = action.triggerType;
            entry.callback.AddListener(data => { if (action.action == null) action.GetAction("Selectables").Invoke(); else action.action.Invoke(); });
            trigger.triggers.Add(entry);
#if UNITY_EDITOR
        }
        catch (Exception ex) {
            CatchListException(ex, "On Selectable");
        }
#endif
    }


    private void SetSelectableTrigger(SerializedAction action) {
        Selectable input = action.triggerInput as Selectable;
#if UNITY_EDITOR
        AddDebug_ActionTrigger(action, input);
        try {
#endif
            // Button 
            #region Button
            if (input.GetType() == typeof(Button)) {
                if (action.action == null)
                    ((Button)input).onClick.AddListener(action.GetAction("Selectables").Invoke);
                else
                    ((Button)input).onClick.AddListener(action.action);
            }
            #endregion
            // Toggle
            #region Toggle
            else if (input.GetType() == typeof(Toggle)) {
                if (action.action == null)
                    ((Toggle)input).onValueChanged.AddListener(value => action.GetAction("Selectables").Invoke());
                else
                    ((Toggle)input).onValueChanged.AddListener(value => action.action.Invoke());
            }
            #endregion
            // Dropdown
            #region Dropdown
            else if (input.GetType() == typeof(Dropdown)) {
                if (action.action == null)
                    ((Dropdown)input).onValueChanged.AddListener(value => action.GetAction("Selectables").Invoke());
                else
                    ((Dropdown)input).onValueChanged.AddListener(value => action.action.Invoke());
            }
            #endregion
            // Input Field
            #region Input Field
            else if (input.GetType() == typeof(InputField)) {
                if (action.action == null)
                    ((InputField)input).onEndEdit.AddListener(value => action.GetAction("Selectables").Invoke());
                else
                    ((InputField)input).onEndEdit.AddListener(value => action.action.Invoke());
            }
            #endregion
#if UNITY_EDITOR
        }
        catch (Exception ex) {
            CatchListException(ex, "On Selectable");
        }
#endif
    }
    // OnEnable functions
    #region OnEnable functions
    void OnEnable() {
        ManualEnable();
    }
    /// <summary> Manually call all actioins in the /OnEnable/ SerializedActions list</summary>
    public void ManualEnable() {
        InvokeSerialisedAction(OnEnableActions, nameof(OnEnableActions));
    }
    #endregion

    // OnDisable functions
    #region OnDisable functions
    private void OnDisable() {
        ManualDisable();
    }
    /// <summary> Manually call all actioins in the /OnDisable/ SerializedActions list</summary>
    public void ManualDisable() {
        InvokeSerialisedAction(OnDisableActions, nameof(OnDisableActions));
    }
    #endregion

    //  Debug Functions
    #region Debug Functions
#if UNITY_EDITOR
    private void Debug_ActionDeserialisation(SerializedAction action, string timeline) {
        debugMessage += "Starting invokation of <b>" + timeline + "<b>\n";
        try {
            debugMessage += "Deserialized action: " + action.methodName + '\n';
            if (action.unityArguments != null)
                Debug_ActionParameters(action.unityArguments, action.argumentNames, true);
            if (action.arguments != null)
                Debug_ActionParameters(action.arguments, action.argumentNames, false);
            action.GetAction(timeline).Invoke();
            debugMessage += "Deserialisation has finished successfully for action with method: " + action.methodName + '\n';
        }
        catch (Exception ex) {
            CatchListException(ex, timeline);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex.InnerException, true);
            System.Diagnostics.StackFrame frame = st.GetFrame(0);
            int line = frame.GetFileLineNumber();
            Debug.LogError("Errow while debugging action: " + action.methodName + ", of script type: " + action.ClassName + ", on instance: " + this.name +
                "\nException: " + ex.InnerException.Message +
                "\nClass: " + ex.InnerException.TargetSite.DeclaringType +
                "\nMethod: " + ex.InnerException.TargetSite +
                "\nLine: " + line +
                "\n\nDebug: " + debugMessage + '\n' +
                "\nStack-trace: " + ex.InnerException.StackTrace + '\n',
                this.gameObject);
        }
    }
    private void Debug_ActionParameters<T>(T[] arguments, List<string> parameterNames, bool isUnityArgument) {
        debugMessage += "Total arguments deriving from " + (isUnityArgument ? nameof(UnityEngine.Object) : nameof(System.Object)) + ": " + arguments.Length + '\n';
        for (int i = 0; i < arguments.Length; i++) {
            string paramName = GetDebug_ParameterName(i, arguments, parameterNames);
            AddDebug_Parameter(paramName, arguments[i], true);
        }

    }
    private string AddDebug_Parameter<T>(string paramName, T argument, bool isUnityEngineObject) {
        string newMessage = "Parameter: " + paramName + ", " +
            "value: " + argument.ToString() + ".\n" +
            (isUnityEngineObject ?
            "Is derived from UnityEngine.Object?: " + argument.GetType().IsSubclassOf(typeof(UnityEngine.Object)) + '\n' :
            "Is derived from System.Object?: " + argument.GetType().IsSubclassOf(typeof(System.Object)) + '\n');
        debugMessage += newMessage;
        return newMessage;
    }
    private void AddDebug_ActionTrigger(SerializedAction action, UnityEngine.Object input) {
        if (input == null)
            Debug.LogError("Action has <NULL> trigger! Instance with error: " + this.name + "\n Method: " + action.methodName + ", Script with method: " + action.ClassName);
        debugMessage += "Deserialized method name: " + action.methodName + ", with trigger: " + action.triggerInput.name + '\n';
        if (action.unityArguments != null)
            Debug_ActionParameters(action.unityArguments, action.argumentNames, true);
        if (action.arguments != null)
            Debug_ActionParameters(action.arguments, action.argumentNames, false);
    }
    private string GetDebug_ParameterName<T>(int currentArgumentIndex, T[] arguments, List<string> parmatersNames) {
        string paramName = "";
        if (currentArgumentIndex < parmatersNames.Count && string.IsNullOrEmpty(parmatersNames[currentArgumentIndex]) == false)
            paramName = parmatersNames[currentArgumentIndex];
        else {
            if (arguments[currentArgumentIndex] == null)
                paramName = "<b><color=red>\nCould not retrieve parameter name!</color></b>";
            else
                paramName = arguments[currentArgumentIndex].ToString();
        }
        return paramName;

    }
    private void CatchListException(Exception ex, string listName) {
        Exception actual = ex;
        while (actual.InnerException != null)
            actual = actual.InnerException;
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(actual, true);
        System.Diagnostics.StackFrame frame = st.GetFrame(0);
        int line = frame.GetFileLineNumber();
        Debug.LogError("Error while invoking list: <b>" + listName + "</b>" +
            "\nException: " + ex.InnerException.Message +
            "\nClass: " + ex.InnerException.TargetSite.DeclaringType +
            "\nMethod: " + ex.InnerException.TargetSite +
            "\nLine: " + line +
            "\n\nDebug: " + debugMessage + '\n' +
            "\nStack-trace: " + ex.InnerException.StackTrace + '\n',
            this.gameObject);
    }
#endif
    #endregion
}

