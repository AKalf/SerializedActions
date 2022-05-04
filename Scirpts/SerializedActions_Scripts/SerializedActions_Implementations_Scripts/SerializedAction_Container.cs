#undef DEBUG_SerializedActions

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using SerializedParameters = SerializedActions.SerializedActions_SerializedParameters;
using SerializedActions.Extensions;


[System.Serializable]
public class SerializedAction_Container {
    public enum ActionTimeline { OnAwake, OnStart, OnEnable, OnDisable, OnInteraction }
    /// <summary>/// The object that triggeres the action ///</summary>
    [SerializeField]
    public UnityEngine.Object TriggerInput;

    [SerializeField]
    public string ClassName = "";

    [SerializeField]
    /// <summary>/// The name of the method of the action ///</summary>
    public string MethodName = "";

    /// <summary>/// The parameter values to invoke the method with///</summary>
    [SerializeField]
    public List<SerializedParameters> Parameters = new List<SerializedParameters>();

    [SerializeField]
    public UnityEngine.EventSystems.EventTriggerType TriggerType = UnityEngine.EventSystems.EventTriggerType.PointerEnter;

    /// <summary>/// The action produced after deserializing and processing ///</summary>
    public UnityAction Action = null;

    /// <summary>/// To use only internally. The arguments of type UnityEngine.Vector. They are stored seperatly and seriliazation is handled by Unity. ///</summary>
    // private Vector3[] vectorArguments;

    public SerializedAction_Container(UnityEngine.Object trigger, MonoScript type, string methodName, List<SerializedParameters> parameters) {

        // Assign trigger input
        this.TriggerInput = trigger;
        // Assign method
        this.MethodName = methodName;
        // Get class
        this.ClassName = type.GetClass().FullName;
        // Get parameters. If we just do: this.parameters = parameters, the values are no serialized and we end up with an empty list
        foreach (SerializedParameters param in parameters)
            this.Parameters.Add(param);
        #region Debug
#if UNITY_EDITOR && DEBUG_SerializedActions
        DebugInstanceConstruction(trigger, type, methodName, args, paramNames, argumentTypesNames);
        SerializedActions_UnitTests.Instance().AddAction(this, type, type.GetClass().GetMethod(this.methodName));
#endif
        #endregion
    }

    public UnityAction GetAction(string timeline) {
        // Debug 
        #region Debug
#if UNITY_EDITOR && DEBUG_SerializedActions
        // Leave this above actual code
        DebugActionManifestation(timeline);
#endif
        #endregion        
        if (this.Parameters == null)
            this.Parameters = new List<SerializedParameters>();

        Type type = null;
        type = Type.GetType(ClassName);
        if (type == null)
            type.GetTypeFromName(ClassName);
        MethodInfo method = type.GetMethod(MethodName);
        this.Action = () => method.Invoke(TriggerInput, Parameters.GetParametersAsSystemObjects());
        return Action;
    }

    // Debug functions
    #region Debug
#if UNITY_EDITOR && DEBUG_SerializedActions
    private void DebugInstanceConstruction(UnityEngine.Object trigger, MonoScript type, string methodName, System.Object[] args, List<string> paramNames, List<string> paramTypesNames) {
        string localDebugMessage = "";
        if (paramNames.Count != args.Length) {
            Debug.LogError("Arguments array length is not equal to registered argument names for new SerializedAction." +
                " Aruments array: " + args.Length + ", names count: " + paramNames.Count + ". Method: " + methodName + " of class: " + type.GetClass().FullName);
        }
        localDebugMessage = "##### Creating new SerializedAction instance..." +
            "\n Trigger object: <b>" + (trigger == null ? "NULL" : trigger.name) + "</b>" +
            "\n Class type: <b>" + type.GetClass().FullName + "</b>" +
            "\n Method name: <b>" + methodName + "</b>" +
            "\n Total arguments: " + (args == null ? "0" : args.Length.ToString());
        for (int i = 0; i < args.Length; i++)
            localDebugMessage += i + ") Object: <b>" + args[i].ToString() + "</b> of type: <b>" + paramTypesNames[i] + "</b>, with registered name: <b>" + paramNames[i] + "</b>";

        Debug.Log(localDebugMessage + "\n\n");
        if (args.Length > 0) {
            localDebugMessage = "Starting arguments serialisation...";
            for (int i = 0; i < args.Length; i++) {
                localDebugMessage += "\n Serializing parameter: <b>" + paramNames[i] + "</b>. Object: " + args[i].ToString();
                if (args[i].GetType().IsSubclassOf(typeof(UnityEngine.Object)))
                    localDebugMessage += "\nSubclass of <b>UnityEngine.Object</b>";
                else
                    localDebugMessage += "\nSubclass of <b>System.Object</b>";
            }
            Debug.Log(localDebugMessage + "\n\n");
        }
        localDebugMessage = "";
        localDebugMessage += "\n<b>-- Serialized array --</b>\n";
        localDebugMessage += '\n' + serializedArray + "\n\n";
        Debug.Log(localDebugMessage + "\n\n");
        localDebugMessage = ("##### Finished creating new SerializedAction");
        localDebugMessage += "\nTrigger input: <b>" + (trigger == null ? "NULL" : trigger.name) + "</b>";
        localDebugMessage += "\nClass: <b>" + ClassName + "</b> Method: <b>" + methodName + "</b>";
        System.Object[] deserializedArgs = XmlDeserializeFromString(serializedArray);
        localDebugMessage += "\nTotal arguments: " + deserializedArgs.Length;
        for (int i = 0; i < deserializedArgs.Length; i++)
            localDebugMessage += "\n" + i + ")  <b>" + argumentNames[i] + ":  " + deserializedArgs[i].ToString() + "</b>";
        Debug.Log(localDebugMessage + "\n\n");
        localDebugMessage = "";

    }
    private void DebugActionManifestation(string timeline) {
        string localDebug = "##### Starting action manifestation...";
        localDebug += "\nTimeline: <b>" + timeline + "</b>Method: <b>" + methodName + "</b> of type: <b>" + ClassName + "</b>, toatal arguments: " + XmlDeserializeFromString(serializedArray).Length;
        localDebug += "\n\nTrying to get actual class from name...";
        Type testType;
        try {
            testType = GetType(ClassName);
            if (testType == null) {
                localDebug += "<color=Red><b>EXCEPTION</b></color>: Could not get actual class from class name: <b>" + ClassName + "</b>, timeline: <b>" + timeline + "</b>";
                Debug.Log(localDebug + "\n\n");
                Debug.LogError("<color=Red><b>EXCEPTION</b></color>: Could not get actual class from class name: <b>" + ClassName + "</b>, Timeline: <b>" + timeline + "</b>");
            }
            else
                DebugMethodManifestation(ref testType, ref localDebug, timeline);
        }
        catch (Exception ex) {
            localDebug += "<color=Red><b>EXCEPTION</b></color>: Could not get actual class from class name: <b>" + ClassName + "</b>, excpetion message: " + ex.Message;
            Debug.Log(localDebug + "\n\n");
            Debug.LogError("<color=Red><b>EXCEPTION</b></color>: Could not get actual class from class name: <b>" + ClassName + "</b>, excpetion message: " + ex.Message);
        }

    }
    private void DebugMethodManifestation(ref Type testType, ref string localDebug, string timeline) {
        localDebug += "\n\nTrying to get actual method from name...";
        MethodInfo methodInfo;
        try {
            methodInfo = testType.GetMethod(methodName);
            if (methodInfo == null) {
                localDebug += "<color=Red><b>EXCEPTION</b></color>: Could not get actual method from method name: <b>" + methodName + "</b>, timeline: <b>" + timeline + "</b>";
                Debug.Log(localDebug + "\n\n");
                Debug.LogError("<color=Red><b>EXCEPTION</b></color>: Could not get actual method from method name: <b>" + methodName + "</b>, timeline: <b>" + timeline + "</b>");
            }
            else
                DebugArgumentsManifastation(ref localDebug, timeline);
        }
        catch (Exception ex) {
            localDebug += "<color=Red><b>EXCEPTION</b></color>: Could not get actual method from method name: <b>" + methodName + "</b>, timeline: <b>" + timeline + "</b>, excpetion message: " + ex.Message;
            Debug.Log(localDebug + "\n\n");
            Debug.LogError("<color=Red><b>EXCEPTION</b></color>: Could not get actual method from method name: <b>" + methodName + "</b>, timeline: <b>" + timeline + "</b>, excpetion message: " + ex.Message);
        }

    }
    private void DebugArgumentsManifastation(ref string localDebug, string timeline) {
        localDebug += "\n\nChecking arguments...";
        arguments = XmlDeserializeFromString(serializedArray);
        ParameterInfo[] actualParames = GetType(ClassName).GetMethod(methodName).GetParameters();
        if (arguments == null) {
            localDebug += "\n<color=Red><b>SerializedAction ERROR</b></color>: Could not deserialise arguments from XML. Method: <b>" + methodName + "</b> of class: <b>" + ClassName + "</b>, timeline: <b>" + timeline + "</b>, serialized array: \n" + serializedArray;
            Debug.Log(localDebug + "\n\n");
            Debug.LogError("<color=Red><b>SerializedAction ERROR</b></color>: Could not deserialise arguments from XML. Method: <b>" + methodName + "</b> of class: <b>" + ClassName + "</b>, timeline: <b>" + timeline + "</b>, serialized array: \n" + serializedArray);
        }
        else if (arguments.Length != actualParames.Length) {
            localDebug += "\n<color=Red><b>SerializedAction ERROR</b></color>: Number of arguments deserialized does not equal number of arguments registered uppon construction. " +
                "\nDeserialised: " + arguments.Length + ", actual: " + actualParames.Length + ". Method: <b>" + methodName + "</b> of class: <b>" + ClassName + "</b>, timeline: <b>" + timeline + "</b>";
            Debug.Log(localDebug + "\n\n");
            Debug.LogError("<color=Red><b>SerializedAction ERROR</b></color>: Number of arguments deserialized does not equal number of arguments registered uppon construction. " +
                "\nDeserialised: " + arguments.Length + ", actual: " + actualParames.Length + ". Method: <b>" + methodName + "</b> of class: <b>" + ClassName + "</b>, timeline: <b>" + timeline + "</b>");
        }
        else {
            localDebug += "\nTotal arguments: " + arguments.Length;
            Debug.Log(localDebug + "\n\n");
            localDebug = "";
        }
    }
#endif
    #endregion

    #region Not-in-use
    //public static IEnumerable<Type> GetAllUserTypesOf(Type wantedType) {
    //    return Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(wantedType));
    //}
    #endregion
}
