#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SerializedActions.Extensions;
using UnityEditor;

namespace SerializedActions.UnitTests {
    public class SerializedActions_UnitTestForMethod : SerializedActions_MethodsRegisters {
        public static bool CheckMethods(SerializedAction_Container action, Type type, SerializedActions_MonobehaviourManager implementation, List<MethodsOfType> classAndMethods, ref string debugMessage) {
            debugMessage = "\n-----|Checking method: " + action.MethodName.Bold() + " of class: " + type.Name.Bold().NewLine();
            bool allGood = true;
            MethodsOfType methodsOfType = GetStructByType(type.Name, classAndMethods);
            if (type.GetMethod(action.MethodName) == null) {
                EditorUtility.SetDirty(implementation);
                debugMessage += "----|".Colored(Color.yellow) + "WARNING:".Bold() + " Could not find method " + action.MethodName.Bold() + " in class".NewLine();
                for (int i = 0; i < methodsOfType.MethodsNames.Count; i++) {
                    if (methodsOfType.MethodsNames[i] == action.MethodName) {
                        debugMessage += "Method match: " + methodsOfType.MethodsNames[i].NewLine();
                        debugMessage += "Start searching for methods with ID: " + methodsOfType.MethodsIDs[i].NewLine();
                        MethodInfo methodActual = methodsOfType.GetMethodById(methodsOfType.MethodsIDs[i]);
                        if (methodActual != null) {
                            // Conflict resolved, method with registered name found
                            debugMessage += ("----|" + "Conflic resolved".Bold()).Colored(Color.green) + " with actual method: " + methodActual.Name.Bold().NewLine();
                            action.MethodName = methodActual.Name;
                            methodsOfType.MethodsNames[i] = methodActual.Name;
                            classAndMethods[i].MethodsNames[i] = methodActual.Name;
                            EditorUtility.SetDirty(SerializedActions_MethodsRegisters.Instance());
                            AssetDatabase.SaveAssets();
                        }
                        else {
                            // Error could not resolve method with registered name
                            debugMessage += ("---|" + "ERROR!".Bold()).Colored(Color.red) + " Could not find actual method: " + action.MethodName.Bold().NewLine();
                            Debug.LogError(debugMessage.NewLine(2), implementation.gameObject);
                            allGood = false;
                        }
                    }
                }
            }
            else
                debugMessage += "Method: " + action.MethodName.Bold() + " is good!";
            return allGood;
        }
    }
}
#endif