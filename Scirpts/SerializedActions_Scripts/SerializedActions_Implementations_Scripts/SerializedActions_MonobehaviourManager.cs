using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[Serializable]
/// <summary>Derive your class from this in order to make it compatible with SerializedActions Inspector </summary>
public class SerializedActions_MonobehaviourManager : MonoBehaviour {
    [SerializeField]
    public List<SerializedAction_Container>
        OnAwakeActions = new List<SerializedAction_Container>(),
        OnStartActions = new List<SerializedAction_Container>(),
        OnEnableActions = new List<SerializedAction_Container>(),
        OnDisableActions = new List<SerializedAction_Container>(),
        OnInteractionActions = new List<SerializedAction_Container>();
    [NonSerialized]
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
        foreach (SerializedAction_Container action in OnInteractionActions) {
            if (action.TriggerInput.GetType().IsSubclassOf(typeof(Selectable)))
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
        if (OnInteractionActions != null && OnInteractionActions.Count > 0)
            AssignPointerEnterActionsToGameObjects();
    }

    private void InvokeSerialisedAction(List<SerializedAction_Container> actions, string timeline) {
        if (actions != null && actions.Count > 0) {
            foreach (SerializedAction_Container action in actions) {
#if UNITY_EDITOR
                Debug_ActionDeserialisation(action, timeline);
#endif
                action.GetAction(timeline).Invoke();
            }
        }
        else
            actions = null;
    }
    private void SetGameObjectTrigger(SerializedAction_Container action) {
        GameObject triggerAsG = null;
#if UNITY_EDITOR
        try {
#endif
            if (action.TriggerInput.GetType() == typeof(GameObject))
                triggerAsG = action.TriggerInput as GameObject;
            else if (action.TriggerInput.GetType() == typeof(MonoBehaviour) || action.TriggerInput.GetType().IsSubclassOf(typeof(MonoBehaviour)))
                triggerAsG = ((MonoBehaviour)action.TriggerInput).gameObject;
            else if (action.TriggerInput.GetType() == typeof(Component) || action.TriggerInput.GetType().IsSubclassOf(typeof(Component)))
                triggerAsG = ((Component)action.TriggerInput).gameObject;
            Debug.LogError(triggerAsG.name);
            EventTrigger trigger = triggerAsG.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = triggerAsG.gameObject.AddComponent<EventTrigger>();
            Debug.LogError(trigger.name);
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = action.TriggerType;
            entry.callback.AddListener(data => {
                if (action.Action == null)
                    action.GetAction("Selectables").Invoke();
                else
                    action.Action.Invoke();
            });
            trigger.triggers.Add(entry);
            Debug.LogError("Event added");
#if UNITY_EDITOR
            AddDebug_ActionTrigger(action, triggerAsG);
        }
        catch (Exception ex) {
            CatchListException(ex, "On Selectable");
        }
#endif
    }
    private void SetSelectableTrigger(SerializedAction_Container action) {
        Selectable input = action.TriggerInput as Selectable;
#if UNITY_EDITOR
        AddDebug_ActionTrigger(action, input);
        try {
#endif
            UnityAction unityAction = action.Action ?? action.GetAction("Selectables").Invoke;
            // Button 
            if (input.GetType() == typeof(Button))
                ((Button)input).onClick.AddListener(unityAction);
            // Toggle
            else if (input.GetType() == typeof(Toggle))
                ((Toggle)input).onValueChanged.AddListener(value => unityAction.Invoke());
            // Dropdown
            else if (input.GetType() == typeof(Dropdown))
                ((Dropdown)input).onValueChanged.AddListener(value => unityAction.Invoke());
            // Input Field
            else if (input.GetType() == typeof(InputField))
                ((InputField)input).onEndEdit.AddListener(value => unityAction.Invoke());

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
    private void Debug_ActionDeserialisation(SerializedAction_Container action, string timeline) {
        debugMessage += "Starting invokation of <b>" + timeline + "<b>\n";
        try {
            debugMessage += "Deserialized action: " + action.MethodName + '\n';
            if (action.Parameters != null)
                Debug_ActionParameters(action.Parameters);
            action.GetAction(timeline);
            debugMessage += "Deserialisation has finished successfully for action with method: " + action.MethodName + '\n';
        }
        catch (Exception ex) {
            CatchListException(ex, timeline);
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex.InnerException, true);
            System.Diagnostics.StackFrame frame = st.GetFrame(0);
            int line = frame.GetFileLineNumber();
            Debug.LogError("Errow while debugging action: " + action.MethodName + ", of script type: " + action.ClassName + ", on instance: " + this.name +
                "\nException: " + ex.InnerException.Message +
                "\nClass: " + ex.InnerException.TargetSite.DeclaringType +
                "\nMethod: " + ex.InnerException.TargetSite +
                "\nLine: " + line +
                "\n\nDebug: " + debugMessage + '\n' +
                "\nStack-trace: " + ex.InnerException.StackTrace + '\n',
                this.gameObject);
        }
    }
    private void Debug_ActionParameters(List<SerializedActions.SerializedActions_SerializedParameters> arguments) {
        debugMessage += "Total arguments " + arguments.Count + '\n';
        for (int i = 0; i < arguments.Count; i++) {
            string paramName = GetDebug_ParameterName(i, arguments);
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
    private void AddDebug_ActionTrigger(SerializedAction_Container action, UnityEngine.Object input) {
        if (input == null)
            Debug.LogError("Action has <NULL> trigger! Instance with error: " + this.name + "\n Method: " + action.MethodName + ", Script with method: " + action.ClassName);
        debugMessage += "Deserialized method name: " + action.MethodName + ", with trigger: " + action.TriggerInput.name + '\n';
        if (action.Parameters != null)
            Debug_ActionParameters(action.Parameters);
    }
    private string GetDebug_ParameterName(int currentArgumentIndex, List<SerializedActions.SerializedActions_SerializedParameters> arguments) {
        string paramName = "";
        if (currentArgumentIndex < arguments.Count && string.IsNullOrEmpty(arguments[currentArgumentIndex].ParameterName) == false)
            paramName = arguments[currentArgumentIndex].ParameterName;
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
        Debug.LogError("Error while invoking list: <b>" + listName ?? "" + "</b>" +
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

