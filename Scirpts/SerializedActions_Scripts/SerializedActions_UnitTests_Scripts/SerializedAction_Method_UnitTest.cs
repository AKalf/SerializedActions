using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SerializedAction_Method_UnitTest : SerializedActions_UnitTests {
    public static bool CheckMethods(SerializedAction_Instance action, Type type, SerializedAction_MonoBehaviour implementation, List<ClassAndMethods> classAndMethods, ref string debugMessage) {
        debugMessage = "\n-----|Checking method: <b>" + action.MethodName + "</b> of class: <b>" + type.Name + "</b>";
        bool allGood = true;
        ClassAndMethods cm = GetStructByType(type.Name, classAndMethods);
        if (type.GetMethod(action.MethodName) == null) {
            debugMessage += "\n<color=yello>----|</color><b>WARNING:</b> Could not find method <b>" + action.MethodName + "</b> in class";
            for (int i = 0; i < cm.MethodsNames.Count; i++) {
                if (cm.MethodsNames[i] == action.MethodName) {
                    debugMessage += "\nMethod match: " + cm.MethodsNames[i];
                    debugMessage += ("\nStart searching for methods with ID: " + cm.MethodsIDs[i]);
                    MethodInfo methodActual = cm.GetMethodById(cm.MethodsIDs[i]);
                    if (methodActual != null) {
                        debugMessage += "\n<color=green>----|<b>Conflic resolved</b></color> with actual method: <b>" + methodActual.Name + "</b>";
                        action.MethodName = methodActual.Name;
                        cm.MethodsNames[i] = methodActual.Name;
                    }
                    else {
                        debugMessage += "\n <color=Red>---|<b>ERROR!</b></color> Could not find actual method: " + action.MethodName;
                        Debug.LogError(debugMessage + "\n\n", implementation.gameObject);
                        allGood = false;
                    }
                }
            }
        }
        else {
            debugMessage += "\nMethod: <b>" + action.MethodName + "</b> is good!";
        }
        return allGood;
    }
}
