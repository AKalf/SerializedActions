using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SerializedActions.UnitTests {
    public class SerializedMethods : UnitTestsDataContainer {
        public static bool CheckMethods(SerializedAction_Instance action, Type type, SerializedAction_MonoBehaviour implementation, List<MethodsOfType> classAndMethods, ref string debugMessage) {
            debugMessage = "\n-----|Checking method: " + action.MethodName.Bold() + " of class: " + type.Name.Bold().NewLine();
            bool allGood = true;
            MethodsOfType cm = GetStructByType(type.Name, classAndMethods);
            if (type.GetMethod(action.MethodName) == null) {
                debugMessage += "----|".Colored(Color.yellow) + "WARNING:".Bold() + " Could not find method " + action.MethodName.Bold() + " in class".NewLine();
                for (int i = 0; i < cm.MethodsNames.Count; i++) {
                    if (cm.MethodsNames[i] == action.MethodName) {
                        debugMessage += "Method match: " + cm.MethodsNames[i].NewLine();
                        debugMessage += "Start searching for methods with ID: " + cm.MethodsIDs[i].NewLine();
                        MethodInfo methodActual = cm.GetMethodById(cm.MethodsIDs[i]);
                        if (methodActual != null) {
                            debugMessage += ("----|" + "Conflic resolved".Bold()).Colored(Color.green) + " with actual method: " + methodActual.Name.Bold().NewLine();
                            action.MethodName = methodActual.Name;
                            cm.MethodsNames[i] = methodActual.Name;
                        }
                        else {
                            debugMessage += ("---|" + "ERROR!".Bold()).Colored(Color.red) + " Could not find actual method: " + action.MethodName.Bold().NewLine();
                            Debug.LogError(debugMessage.NewLine(2), implementation.gameObject);
                            allGood = false;
                        }
                    }
                }
            }
            else {
                debugMessage += "Method: " + action.MethodName.Bold() + " is good!";
            }
            return allGood;
        }
    }
}
