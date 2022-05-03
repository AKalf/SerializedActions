#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SerializedActions.Extensions;
namespace SerializedActions.UnitTests {
    public class SerializedActions_UnitTestForType : SerializedActions_MethodsRegisters {
        public static Type CheckType(SerializedAction_Container action, List<MonoScript> monoscripts, SerializedActions_MonobehaviourManager implementation, List<MethodsOfType> classesAndMethods) {
            string debugMessage = "\n\n-----|Checking Class Name for action with " +
                "Method: " + action.MethodName.Bold().Comma() +
                "Implementation: " + implementation.gameObject.name.Bold().Comma() +
                "Class name: " + action.ClassName.Bold().NewLine();
            Type resolvedType = null;
            resolvedType = resolvedType.GetTypeFromName(action.ClassName);

            if (resolvedType != null) {
                debugMessage += "Type found successfully (" + resolvedType.Name.Bold() + ")";
                return resolvedType;
            }
            else {
                Debug.LogWarning(string.Format(TypeConflictWarning, action.ClassName, implementation.gameObject.name), implementation.gameObject);
                debugMessage += (string.Format(TypeConflictWarning, action.ClassName, implementation.gameObject.name));

                for (int i = 0; i < classesAndMethods.Count; i++) {
                    MethodsOfType cm = classesAndMethods[i];
                    if (cm.TypeName == action.ClassName) {
                        Type foundType = CheckAndRetrieveClass(cm, monoscripts[i], ref debugMessage);
                        cm.TypeName = foundType.Name;
                        action.ClassName = foundType.Name;
                        // Debug resolve
                        debugMessage += string.Format(ResolvedConflict, action.ClassName, foundType.Name);
                        Debug.Log(string.Format(ResolvedConflict, action.ClassName, foundType.Name), implementation.gameObject);
                        return foundType;
                    }
                }
                debugMessage += string.Format(CouldNotResolveType, action.ClassName);
                Debug.LogError(string.Format(CouldNotResolveType, action.ClassName) + "\nDebug:\n" + debugMessage + "\n\n", implementation.gameObject);

            }
            return null;
        }
        private static Type CheckAndRetrieveClass(MethodsOfType methodOfType, MonoScript mono, ref string debugMessage) {
            Type t = null;
            t = t.GetTypeFromName(methodOfType.TypeName);
            if (t == null) {
                debugMessage += string.Format(ResolvedConflict, methodOfType.TypeName, mono.GetClass().Name);
                Debug.Log(string.Format(ResolvedConflict, methodOfType.TypeName, mono.GetClass().Name) + "\nDebug:\n" + debugMessage + "\n\n");
                methodOfType.TypeName = mono.GetClass().Name;
            }
            t = t.GetTypeFromName(methodOfType.TypeName);
            if (t == null) {
                debugMessage += string.Format(CouldNotFindTypeInMonoScript, methodOfType.TypeName, mono.name);
                Debug.LogError(string.Format(CouldNotFindTypeInMonoScript, methodOfType.TypeName, mono.name) + "\n Debug:\n" + debugMessage + "\n\n");

            }
            return t;
        }

        // Debug strings
        #region Debug Strings
        /// <summary>
        /// 2 Parameters:
        ///- Class name
        ///- Implementation name
        /// </summary>
        private const string TypeConflictWarning = "<color=Yellow>WARNING</color>: " +
            "Trying to resolve conflict of " +
            "Class name: <b>{0}</b>, " + // Class name 
            "Implementation: <b>{1}</b>\n"; // Implementation name
        /// <summary>
        /// 2 Parameters:
        ///- Class name
        ///- Resolved class name
        /// </summary>
        private const string ResolvedConflict = "<color=Green>----|</color>" +
            "Conflict resolved: " +
            "Class name: <b>{0}</b>, " + // Class name
            "Resolved with class: <b>{1}</b>\n"; // Resolved class name
        /// <summary>
        /// 2 Parameters:
        ///- Class name
        ///- Resolved class name
        /// </summary>
        private const string CouldNotResolveType = "SerializedAction <color=Red>----|ERROR</color>:" +
            "Conflict for " +
            "Class name: <b>{0}</b>, " + // Class name
            "could not be resolved. You need to do it manually.\n";
        /// <summary>
        /// 2 Parameters:
        ///- Class name
        ///- Resolved class name
        /// </summary>
        private const string CouldNotFindTypeInMonoScript = "SerializedAction <color=Red>----|ERROR</color>:" +
            "Could not find " +
            "Class name: +<b>{0}</b>, in " + // Class name
            "Monoscript: <b>{1}</b>. " + // Monoscript
            "You need to do it manually.\n";
        #endregion
    }
}
#endif