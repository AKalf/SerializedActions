#define UNITY_EDITOR
#undef DEBUG_SerializedActions

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class SerializedActionData {
    public enum ActionTimeline { OnAwake, OnStart, OnEnable, OnDisable, OnPointerEnterInteraction }
    /// <summary>/// The object that triggeres the action ///</summary>
    [SerializeField]
    public UnityEngine.Object TriggerInput;

    [SerializeField]
    public string ClassName = "";

    [SerializeField]
    /// <summary>/// The name of the method of the action ///</summary>
    public string MethodName = "";

    /// <summary>/// The string that holds the serialized arguments ///</summary>
    [SerializeField]
    public string SerializedArray = "";

    /// <summary>/// The arguments for the method ///</summary>
    public System.Object[] Arguments;

    /// <summary>/// The names of the arguments for debugging ///</summary>
    [SerializeField]
    public List<string> ArgumentNames = new List<string>();

    [SerializeField]
    public List<string> ArgumentTypesNames = new List<string>();
    /// <summary>/// To use only internally. The arguments of type UnityEngine.Object. They are stored seperatly and seriliazation is handled by Unity. ///</summary>
    [SerializeField]
    public UnityEngine.Object[] UnityArguments;

    [SerializeField]
    public UnityEngine.EventSystems.EventTriggerType TriggerType = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
    /// <summary>/// To use only internally. The arguments of type UnityEngine.Vector. They are stored seperatly and seriliazation is handled by Unity. ///</summary>
    // private Vector3[] vectorArguments;

    /// <summary>/// The action produced after deserializing and processing ///</summary>
    public UnityAction Action = null;

#if UNITY_EDITOR
    public SerializedActionData(UnityEngine.Object trigger, MonoScript type, string methodName, System.Object[] args, List<string> paramNames, List<string> paramTypesNames) {

        // Assign trigger input
        this.TriggerInput = trigger;
        // Assign method
        this.MethodName = methodName;
        // Get class
        this.ClassName = type.GetClass().FullName;

        // Get Arguments
        this.Arguments = new System.Object[args.Length];
        this.UnityArguments = new UnityEngine.Object[args.Length];
        // Filter arguments between objects that derive from System.Object and objects that derive from UnityEngine.Object
        for (int i = 0; i < args.Length; i++) {
            if (args[i] != null && args[i].GetType().IsSubclassOf(typeof(UnityEngine.Object))) {
                this.UnityArguments[i] = args[i] as UnityEngine.Object;
                this.Arguments[i] = null;
            }
            else {
                this.UnityArguments[i] = null;
                this.Arguments[i] = args[i] as System.Object;
            }
            // Assign argument names
            this.ArgumentNames.Add(paramNames[i]);
            // Assign argument type names
            this.ArgumentTypesNames.Add(paramTypesNames[i]);

        }
        // Serialise arguments to XML string
        this.SerializedArray = XmlSerializeToString(Arguments);
        #region Debug
#if UNITY_EDITOR && DEBUG_SerializedActions
        DebugInstanceConstruction(trigger, type, methodName, args, paramNames, argumentTypesNames);
        SerializedActions_UnitTests.Instance().AddAction(this, type, type.GetClass().GetMethod(this.methodName));
#endif
        #endregion
    }
#else
    public SerializedAction() { }
#endif
    public UnityAction GetAction(string timeline) {
        // Debug 
        #region Debug
#if UNITY_EDITOR && DEBUG_SerializedActions
        // Leave this above actual code
        DebugActionManifestation(timeline);
#endif
        #endregion

        Arguments = XmlDeserializeFromString(SerializedArray);
        if (this.Arguments == null) {
            this.Arguments = new System.Object[0];
        }

        Type type = GetType(ClassName);
        MethodInfo method = type.GetMethod(MethodName);
        this.Action = () => method.Invoke(TriggerInput, Arguments);
        return Action;
    }


    public string XmlSerializeToString(System.Object[] array) {
        var serializer = new System.Xml.Serialization.XmlSerializer(array.GetType());
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        using (System.IO.TextWriter writer = new System.IO.StringWriter(sb)) {
            serializer.Serialize(writer, array);
        }
#if UNITY_EDITOR && DEBUG_SerializedActions
        //Debug.Log("Trying to deserialize for testing...");
        //System.Object[] args = XmlDeserializeFromString(sb.ToString());
        //for (int i = 0; i < args.Length; i++) {
        //    Debug.Log("Serialized argument: " + i + ") " + argumentNames[i] + " = " + args[i].ToString() + " succesfully");
        //}
        //Debug.Log("XML: " + sb.ToString());
        //Debug.Log("############ SERILIZATION - END ############");
#endif
        return sb.ToString();
    }
    public System.Object[] XmlDeserializeFromString(string objectData) {
        System.Object[] result = null;
        if (string.IsNullOrEmpty(objectData) == false) {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(System.Object[]));

            using (System.IO.TextReader reader = new System.IO.StringReader(objectData)) {
                result = serializer.Deserialize(reader) as System.Object[];
            }
            if (UnityArguments.Length != result.Length)
                return null;
            if (result != null) {
                for (int i = 0; i < result.Length; i++) {
                    if (UnityArguments[i] != null)
                        result[i] = UnityArguments[i];
                }
            }
            else {
                result = new System.Object[UnityArguments.Length];
                for (int i = 0; i < result.Length; i++) {
                    result[i] = UnityArguments[i];
                }
            }
        }
        else {
            result = new System.Object[UnityArguments.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = UnityArguments[i];
        }
        return result;
    }

    public static Type GetType(string typeName) {
        try {
            Type type = Type.GetType(typeName);
            if (type == null)
                type = typeof(UnityEngine.GameObject).Module.GetTypes().FirstOrDefault(t => t.Name == typeName);
            else
                return type;
            if (type == null)
                type = typeof(UnityEngine.Component).Module.GetTypes().FirstOrDefault(t => t.Name == typeName);
            else
                return type;
            if (type == null)
                type = typeof(UnityEngine.UI.Selectable).Module.GetTypes().FirstOrDefault(t => t.Name == typeName);
            else
                return type;
            if (type == null)
                type = typeof(UnityEngine.Object).Assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
            else
                return type;
            if (type == null)
                type = typeof(CanvasGroup).Assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
            else
                return type;
            if (type == null)
                Debug.LogError("Error while trying to get type: " + typeName + " while de-serialising action");
            return type;
        }
        catch (Exception ex) {
            Debug.LogError("Error while trying to get type: " + typeName + " while de-serialising action\n Error: " + ex.Message);
            return null;
        }

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
