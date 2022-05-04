
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SerializedActions.Extensions;
using SerializedParameter = SerializedActions.SerializedActions_SerializedParameters;
using System.Linq;

namespace SerializedActions.UnitTests {
    public class SerializedActions_UnitTestForParameters : SerializedActions_MethodsRegisters {
        private static string debugMessage = "";
        private static SerializedAction_Container action = null;
        private static SerializedActions_MonobehaviourManager implementation = null;
        private static MethodInfo method = null;
        private static ParameterInfo[] actual = null;

        public static bool CheckMethodParameters(MethodInfo methodInfo, SerializedAction_Container serializedAction, SerializedActions_MonobehaviourManager imple) {

            action = serializedAction;
            implementation = imple;
            method = methodInfo;

            debugMessage = "\n-----|" + "UNIT TEST: SerializedAction Parameters".Bold().NewLine() +
                   "Method " + method.Name.Bold().NewLine() +
                   "Implementation: " + implementation.gameObject.name.Bold().NewLine();

            actual = method.GetParameters();
            debugMessage += "Actual parameters: " + actual.Length.Bold().Comma() + " deserialized: " + serializedAction.Parameters.Count.Bold();

            // Check length
            bool isLentgthEqual = IsArgumentsLentgthEqual(serializedAction.Parameters.Count, actual.Length);
            bool areTypesEqual = AreParameterTypesEqual();

            if (!isLentgthEqual || !areTypesEqual) {
                // Print user warning to check action 
                Debug.LogWarning(string.Format(NeedToCheckActionWarning, imple.name, methodInfo.Name, action.ClassName).NewLine(2), imple.gameObject);
                debugMessage += string.Format(NeedToCheckActionWarning, imple.name, methodInfo.Name, action.ClassName).NewLine();
                debugMessage += "Trying to resolve conflicts...".NewLine();

                SerializedParameter[] resolvedArguments = new SerializedParameter[actual.Length];
                // Try to resolve conflict by finding a parameter with same name and type
                ResolveArgumentsByName(resolvedArguments, serializedAction);
                // Try to resolve unresolved conflicts by finding parameters with the same index and type
                ResolveArgumentsByType(resolvedArguments, serializedAction);
                // Resolve null values by creating an empty instance of their type and assigns values to the action
                ForceResolve(resolvedArguments);
            }
            else {
                debugMessage += "Types were " + "correct!".Colored(Color.green);
                for (int i = 0; i < actual.Length; i++)
                    action.Parameters[i].ParameterName = actual[i].Name;
                Debug.Log(debugMessage.NewLine(2));
                return true;
            }
            Debug.Log(debugMessage.NewLine(2));
            debugMessage = "";
            return false;

        }

        private static bool IsArgumentsLentgthEqual(int serializedLength, int actualLength) {
            if (serializedLength != actualLength) {
                Debug.LogWarning(string.Format(ArgumentsLengthConflict, action.MethodName, action.ClassName).NewLine(2), implementation.gameObject);
                debugMessage += string.Format(ArgumentsLengthConflict, action.MethodName, action.ClassName);
                return false;
            }
            else
                return true;
        }
        private static bool AreParameterTypesEqual() {
            debugMessage += "\nChecking if types are equal...";
            bool areAllEqual = true;
            for (int i = 0; i < actual.Length; i++) {
                if (i < action.Parameters.Count && actual[i].ParameterType.Name != action.Parameters[i].ParameterTypeName) {
                    // Debig warnig for Type conflict
                    debugMessage += string.Format(ArgumentTypesConflict, actual[i].ParameterType.Name, action.Parameters[i].ParameterTypeName);
                    Debug.LogWarning(string.Format(ArgumentTypesConflict, actual[i].ParameterType.Name, action.Parameters[i].ParameterTypeName.NewLine(2)), implementation.gameObject);
                    areAllEqual = false;
                }
                else if (i >= action.Parameters.Count)
                    areAllEqual = false;
            }
            return areAllEqual;

        }
        private static void ResolveArgumentsByName(SerializedParameter[] resolvedArguments, SerializedAction_Container action) {
            for (int j = 0; j < actual.Length; j++) {
                for (int i = 0; i < action.Parameters.Count; i++) {
                    if (action.Parameters[i] != null
                        && AreStringsEqual(actual[j].Name, action.Parameters[i].ParameterName)
                        && AreStringsEqual(actual[j].ParameterType.Name, action.Parameters[i].ParameterTypeName)) {
                        // BODY:

                        string msg = string.Format(ResolvedConflict, actual[j].Name, actual[j].ParameterType.Name, j, method.Name, action.Parameters[i].ToString());
                        debugMessage += msg;
                        Debug.LogWarning(msg, implementation.gameObject);
                        resolvedArguments[j] = action.Parameters[i];
                    }

                }
            }
        }
        private static void ResolveArgumentsByType(SerializedParameter[] resolvedArguments, SerializedAction_Container action) {
            for (int j = 0; j < actual.Length; j++) {
                for (int i = 0; i < action.Parameters.Count; i++) {
                    if (resolvedArguments.Contains(action.Parameters[i]) == false) {
                        if (j < action.Parameters.Count && action.Parameters[i] != null && j == i && actual[j].ParameterType == action.Parameters[i].GetType()) {
                            string msg = string.Format(ResolvedConflict, actual[j].Name, actual[j].ParameterType.Name, j, method.Name, action.Parameters[i].ToString());
                            debugMessage += msg;
                            Debug.LogWarning(msg, implementation.gameObject);
                            resolvedArguments[j] = action.Parameters[i];
                        }
                    }
                }
            }
        }
        private static void ForceResolve(SerializedParameter[] resolvedArguments) {
            SerializedParameter[] oldParams = action.Parameters.ToArray();
            action.Parameters.Clear();
            for (int i = 0; i < resolvedArguments.Length; i++) {
                if (resolvedArguments[i] == null) {
                    if (oldParams[i].ParameterName == actual[i].Name && oldParams[i].ParameterType == actual[i].ParameterType)
                        action.Parameters.Add(oldParams[i]);
                    action.Parameters.Add(SerializedParameter.CreateSerializedParameter(
                        actual[i].Name, actual[i].ParameterType, actual[i].ParameterType.GetDefaultValue()));
                    string debugMsg = string.Format(NeedToAssignValueToParameter, implementation.name, method.Name, action.ClassName, actual[i].ParameterType.Name, actual[i].Name, i);
                    debugMessage += debugMsg;
                    Debug.LogError(debugMsg.NewLine(2), implementation.gameObject);
                }
                else
                    action.Parameters.Add(resolvedArguments[i]);
            }
        }
        private static bool AreStringsEqual(string string1, string string2) {
            if (string1 == string2)
                return true;
            else
                return false;
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
}