using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SerializedAction_Parameters_UnitTest : SerializedActions_UnitTests {
    private static string debugMessage = "";
    private static SerializedAction action = null;
    private static SerializedActions_MonoBehaviourHolder implementation = null;
    private static MethodInfo method = null;
    private static System.Object[] deserialized = null;
    private static ParameterInfo[] actual = null;

    public static bool CheckMethodParameters(MethodInfo methodInfo, SerializedAction serializedAction, SerializedActions_MonoBehaviourHolder imple) {

        action = serializedAction;
        implementation = imple;
        method = methodInfo;

        debugMessage = "\n-----|<b>UNIT TEST: SerializedAction Parameters</b>\n" +
               "Method <b>" + method.Name + "</b>\n" +
               "Implementation: <b>" + implementation.gameObject.name + "</b>";

        actual = method.GetParameters();
        if (GetDeserializedArguments() == false)
            return false;
        debugMessage += "\nActual parameters: <b>" + actual.Length + "</b>, deserialized: <b>" + deserialized.Length + "</b>";

        // Check length
        bool isLentgthEqual = IsArgumentsLentgthEqual(deserialized.Length, actual.Length);
        bool areTypesEqual = true;
        if (isLentgthEqual)
            areTypesEqual = AreParameterTypesEqual();

        if (!isLentgthEqual || !areTypesEqual) {
            // Print user warning to check action 
            Debug.LogWarning(string.Format(NeedToCheckActionWarning, imple.name, methodInfo.Name, action.ClassName) + "\n\n", imple.gameObject);
            debugMessage += string.Format(NeedToCheckActionWarning, imple.name, methodInfo.Name, action.ClassName);
            debugMessage += "\nTrying to resolve conflicts...";

            action.arguments = new System.Object[actual.Length];
            action.unityArguments = new UnityEngine.Object[actual.Length];

            List<System.Object> resolvedArguments = new List<object>();
            // Try to resolve conflict by finding a parameter with same name and type
            ResolveArgumentsByName(resolvedArguments);
            // Try to resolve unresolved conflicts by finding parameters with the same index and type
            ResolveArgumentsByType(resolvedArguments);
            // Resolve null values by creating an empty instance of their type.
            ForceResolve();

            // Reset parameter names and types
            action.argumentTypesNames.Clear();
            action.argumentNames.Clear();
            foreach (ParameterInfo info in actual) {
                action.argumentTypesNames.Add(info.ParameterType.Name);
                action.argumentNames.Add(info.Name);
            }
            action.serializedArray = action.XmlSerializeToString(action.arguments);
        }

        else {
            debugMessage += "\nTypes were <color=green>correct</color>!";
            action.argumentNames.Clear();
            foreach (ParameterInfo info in actual) {
                action.argumentNames.Add(info.Name);
            }
            Debug.Log(debugMessage + "\n\n");
            return true;
        }
        action.serializedArray = action.XmlSerializeToString(action.arguments);
        Debug.Log(debugMessage + "\n\n");
        debugMessage = "";
        return false;

    }

    private static bool GetDeserializedArguments() {
        deserialized = action.XmlDeserializeFromString(action.serializedArray);

        if (deserialized == null) {
            Debug.LogWarning(string.Format(deserializedArrayIsNUll, action.serializedArray), implementation.gameObject);
            return false;
        }
        else
            return true;
    }
    private static bool IsArgumentsLentgthEqual(int serializedLength, int actualLength) {
        if (serializedLength != actualLength) {
            // Debug warning 
            Debug.LogWarning(string.Format(ArgumentsLengthConflict, action.methodName, action.ClassName) + "\n\n", implementation.gameObject);
            debugMessage += string.Format(ArgumentsLengthConflict, action.methodName, action.ClassName);
            return false;
        }
        else
            return true;
    }
    private static bool AreParameterTypesEqual() {
        debugMessage += "\nChecking if types are equal...";
        bool areAllEqual = true;
        for (int i = 0; i < actual.Length; i++) {
            if (i < action.argumentTypesNames.Count && actual[i].ParameterType.Name != action.argumentTypesNames[i]) {
                // Debig warnig for Type conflict
                debugMessage += string.Format(ArgumentTypesConflict, actual[i].ParameterType.Name, action.argumentTypesNames[i]);
                Debug.LogWarning(string.Format(ArgumentTypesConflict, actual[i].ParameterType.Name, action.argumentTypesNames[i] + "\n\n"), implementation.gameObject);
                areAllEqual = false;
            }
            else if (i >= action.argumentTypesNames.Count)
                areAllEqual = false;
        }
        return areAllEqual;

    }
    private static void ResolveArgumentsByName(List<System.Object> resolvedArguments) {
        for (int j = 0; j < actual.Length; j++) {
            for (int i = 0; i < deserialized.Length; i++) {
                if (deserialized[i] != null
                    && AreStringsEqual(actual[j].Name, action.argumentNames[i])
                    && AreStringsEqual(actual[j].ParameterType.Name, action.argumentTypesNames[i])) {

                    // BODY:
                    string msg = string.Format(ResolvedConflict, actual[j].Name, actual[j].ParameterType.Name, j, method.Name, deserialized[i].ToString());
                    debugMessage += msg;
                    Debug.LogWarning(msg, implementation.gameObject);
                    AddEmptyArgument(deserialized[i], actual[j], j, action, ref debugMessage);
                    resolvedArguments.Add(deserialized[i]);
                }

            }
        }
    }
    private static void ResolveArgumentsByType(List<System.Object> resolvedArguments) {
        for (int j = 0; j < actual.Length; j++) {
            for (int i = 0; i < deserialized.Length; i++) {
                if (resolvedArguments.Contains(deserialized[i]) == false) {
                    if (j < deserialized.Length && deserialized[i] != null && j == i && actual[j].ParameterType == deserialized[i].GetType()) {
                        string msg = string.Format(ResolvedConflict, actual[j].Name, actual[j].ParameterType.Name, j, method.Name, deserialized[i].ToString());
                        debugMessage += msg;
                        Debug.LogWarning(msg, implementation.gameObject);
                        AddEmptyArgument(deserialized[i], actual[j], j, action, ref debugMessage);
                        resolvedArguments.Add(deserialized[i]);
                    }
                }
            }
        }
    }
    private static void ForceResolve() {
        for (int i = 0; i < action.arguments.Length; i++) {
            if (action.arguments[i] == null && action.unityArguments[i] == null) {
                if (actual[i].ParameterType.IsSubclassOf(typeof(UnityEngine.Object)) || actual[i].ParameterType == typeof(UnityEngine.Object))
                    AddEmptyArgument(new UnityEngine.Object(), actual[i], i, action, ref debugMessage);
                else
                    AddEmptyArgument(actual[i].DefaultValue, actual[i], i, action, ref debugMessage);
                string debugMsg = string.Format(NeedToAssignValueToParameter, implementation.name, method.Name, action.ClassName, actual[i].ParameterType.Name, actual[i].Name, i);
                debugMessage += debugMsg;
                Debug.LogError(debugMsg + "\n\n", implementation.gameObject);
            }
        }
    }
    private static bool AreStringsEqual(string string1, string string2) {
        if (string1 == string2)
            return true;
        else
            return false;
    }
    private static void AddEmptyArgument(System.Object argument, ParameterInfo actual, int indexOfArgument, SerializedAction action, ref string debugMessage) {
        // set the new value as the old one as Unity Object
        if (argument.GetType().IsSubclassOf(typeof(UnityEngine.Object)) || argument.GetType() == typeof(UnityEngine.Object))
            action.unityArguments[indexOfArgument] = argument as UnityEngine.Object;
        // set the new value as the old one as it is (should be primitive)
        else
            FindSystemType(action, argument, argument.GetType(), actual, indexOfArgument);

        // Set type name
        if (indexOfArgument < action.argumentTypesNames.Count)
            action.argumentTypesNames[indexOfArgument] = actual.ParameterType.Name;
        else
            action.argumentTypesNames.Add(actual.ParameterType.Name);

        // Set parameter name
        if (indexOfArgument < action.argumentNames.Count)
            action.argumentNames[indexOfArgument] = actual.Name;
        else
            action.argumentNames.Add(actual.Name);

    }
    private static void FindSystemType(SerializedAction action, System.Object arg, Type argumentType, ParameterInfo actual, int argumentIndex) {
        Debug.Log("Adding new system.obj field: " + actual.Name);
        if (arg == null) {
            Debug.Log("Default value: " + actual.RawDefaultValue);
            arg = actual.RawDefaultValue;
        }
        if (argumentType == typeof(int) || argumentType == typeof(float))
            action.arguments[argumentIndex] = arg == null ? 0 : arg;
        else if (argumentType == typeof(string))
            action.arguments[argumentIndex] = arg == null ? "" : arg;
        else if (argumentType == typeof(bool))
            action.arguments[argumentIndex] = arg == null ? false : arg;
    }
    // Debug strings
    #region Debug Strings
    /// <summary>
    /// 1 Parameter:
    ///- Deserialized array as string
    /// </summary>
    private const string deserializedArrayIsNUll =
        "<b>\nSerializedAction</b> <color=red>ERROR:</color> Deserialized null arguments array from serialized array: \n{0}";

    /// <summary>
    /// 2 Parameters:
    ///- Method name
    ///- Class name
    /// </summary>
    private const string ArgumentsLengthConflict =
        "\n<b>SerializedAction</b> <color=yellow>WARNIG:</color>" + " " +
        "<b>Arguments conflict!</b>" +
        "\nSerialized arguments length does not equal actual number of parameters. Trying to resolve..." +
        "\nMethod: <b>{0}</b>" + // Method
        "\nClass <b>{1}</b>"; // Class name
    /// <summary>
    /// 2 Parameters:
    ///- Actual type name
    ///- Serialized type name
    /// </summary>
    private const string ArgumentTypesConflict =
        "\n<color=yellow>WARNIG:</color> " +
        "There was <color=red>CONFLICT</color> between argument types. " +
        "Actual type: <b>{0}</b>, " + // Actual Type
        "serialized type: <b>{1}</b>"; // Serialized Type
    /// <summary>
    ///  3 Parameters:
    ///- Implementation name
    ///- Method name
    ///- Class name
    /// </summary>
    private const string NeedToCheckActionWarning = "<b>\nSerialized Action</b> <color=yellow>WARNING</color> " +
        "<b>You need to check SerializedAction parameters to make sure assigned values are correct</b>" +
        "\nSerialized Action on gameobject: {0}" + // Implementation
        "\nMethod: <b>{1}</b>" + // Method name
        "\nType: <b>{2}</b>."; // Class name
    /// <summary>
    /// 6 Parameters:
    ///- Implementation name
    ///- Method name
    ///- Class name
    ///- Actual parameter type name
    ///- Actual parameter name
    ///-  Parameter index
    /// </summary>
    private const string NeedToAssignValueToParameter = "\n<color=red>ERROR</color> " +
        "You need to assign a parameter value for Serialized Action on " +
        "Gameobject: <b>{0}</b>, " + // Implementation name
        "Method: <b>{1}</b>, " + // Method name
        "Type: <b>{2}</b>, " + // Class name
        "Parameter Type: <b>{3}</b>, " + // Actual parameter type name
        "Parameter Name: <b>{4}</b>, " + // Actual parameter name
        "Index: {5}"; // Parameter index

    /// <summary>
    /// Total parameters: 5
    ///- Parameter name
    ///- Parameter type name
    ///- Parameter index
    ///- Method name
    ///- Resolved value
    /// </summary>
    private const string ResolvedConflict = "\n<color=green>----|<b>RESOLVED</b></color> " +
        "Parameter name: <b>{0}</b>, " + // Parameter name
        "Parameter type: <b>{1}</b>, " + // Parameter type name
        "Parameter index: <b>{2}</b>, " + // Parameter index
        "Method: <b>{3}</b>" + //Method name
        "was resolved with value:  <b>{4}</b>"; // Resolved value

    #endregion
}
