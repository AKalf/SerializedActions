#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SerializedActions.Extensions;
using MonoManager = SerializedActions_MonobehaviourManager;
namespace SerializedActions.UnitTests {
    public class SerializedActions_UnitTestForType : SerializedActions_MethodsRegisters {
        public static Type CheckType(SerializedAction_Container action, List<MonoScript> monoscripts, MonoManager implementation, List<MethodsOfType> classesAndMethods) {
            string debugMessage = "\n\n-----|TYPE UNIT TEST " +
                " --- CLASS NAME: " + action.ClassName.Bold() +
                " --- METHOD: " + action.MethodName.Bold() +
                " --- MANAGER: " + implementation.gameObject.name.Bold().NewLine(1);
            Type resolvedType = null;
            resolvedType = resolvedType.GetTypeFromName(action.ClassName);

            if (resolvedType != null) {
                debugMessage += "Type found successfully (" + resolvedType.Name.Bold() + ")";
                return resolvedType;
            }
            else {
                EditorUtility.SetDirty(implementation);
                Debug.LogWarning(string.Format(TypeConflictWarning, action.ClassName, implementation.gameObject.name), implementation.gameObject);
                debugMessage += (string.Format(TypeConflictWarning, action.ClassName, implementation.gameObject.name));

                for (int i = 0; i < classesAndMethods.Count; i++) {
                    MethodsOfType methodOfType = classesAndMethods[i];
                    if (methodOfType.TypeName == action.ClassName) {
                        Type foundType = CheckAndRetrieveClass(methodOfType, monoscripts[i], ref debugMessage);
                        methodOfType.TypeName = foundType.Name;
                        action.ClassName = foundType.Name;
                        // Debug resolve
                        debugMessage += string.Format(ResolvedConflict, action.ClassName, foundType.Name);
                        Debug.Log(string.Format(ResolvedConflict, action.ClassName, foundType.Name), implementation.gameObject);
                        classesAndMethods[i].TypeName = foundType.Name;
                        EditorUtility.SetDirty(Instance());
                        AssetDatabase.SaveAssets();
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
            " -- CLASS: <b>{0}</b>, " + // Class name 
            " -- MANAGER: <b>{1}</b>\n"; // Implementation name
        /// <summary>
        /// 2 Parameters:
        ///- Class name
        ///- Resolved class name
        /// </summary>
        private const string ResolvedConflict = "<color=Green>----|" +
            "Conflict resolved: </color>" +
            "-- CLASS: <b>{0}</b>, " + // Class name
            " -- ACTUAL CLASS: <b>{1}</b>\n"; // Resolved class name
        /// <summary>
        /// 2 Parameters:
        ///- Class name
        ///- Resolved class name
        /// </summary>
        private const string CouldNotResolveType = "SerializedAction <color=Red>----|ERROR</color>:" +
            "Could NOT resolve conflict of class. " +
            "CLASS: <b>{0}</b>, " + // Class name
            "you need to do it manually.\n";
        /// <summary>
        /// 2 Parameters:
        ///- Class name
        ///- Resolved class name
        /// </summary>
        private const string CouldNotFindTypeInMonoScript = "SerializedAction <color=Red>----|ERROR</color>:" +
            "Could NOT find CLASS with NAME " + "<b>{0}</b>, in " + // Class name
            " -- MONOSCRIPT : <b>{1}</b>. " + // Monoscript
            "You need to do it manually.\n";
        #endregion
    }
}
#endif