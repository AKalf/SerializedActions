using System;
using System.Reflection;
using UnityEngine;
using SerializedActions.Extensions;
namespace SerializedActions.Debugs {
    public static class SerializedActions_Debugs {

        public static void AssertParameterVaribales(object value, Type type, string parameterName) {
            if (string.IsNullOrEmpty(parameterName) && value == null && type == null)
                Debug.LogError("Parameter has everything NULL!");
            else if (string.IsNullOrEmpty(parameterName) == false) {
                if (type == null)
                    Debug.LogError("Parameter: " + parameterName.Bold() + " has null as type value.");
                else if (type.IsUnityObjectType() == false && value == null)
                    Debug.LogError("Parameter: " + parameterName.Bold() + "of type: " + type.Name.Bold() + " is primitive but has NULL as value");
            }

        }
        public static void DebugParameterWithDefaultValue(SerializedActions_SerializedParameters parameter, ref string debugMessage) {
            debugMessage += "Parameter name: " + parameter.ParameterName + ", " +
                "Type: " + parameter.ParameterType.Name.Bold() + ", " +
                "Default value: " + parameter.ParameterType.GetDefaultValue().NewLine();
        }
        public static void DebugParameterWithDefaultValue(ParameterInfo parameter, ref string debugMessage) {
            debugMessage += "Parameter name: " + parameter.Name + ", " +
                "Type: " + parameter.ParameterType.Name.Bold() + ", " +
                "Default value: " + parameter.ParameterType.GetDefaultValue().NewLine();
        }
        public static void DebugRegisterNewAction(SerializedAction_Container action, SerializedAction_Container.ActionTimeline selectedTimeline, ref string debugMessage) {
            try {
                DebugActionVariables(action, selectedTimeline, ref debugMessage);
                if (action.Parameters != null) {
                    for (int i = 0; i < action.Parameters.Count; i++) {
                        debugMessage += "Name: " + action.Parameters[i]?.ParameterName.Bold().Comma() +
                        "Type: " + action.Parameters[i]?.ParameterTypeName.Comma() +
                        "Object: " + action.Parameters[i]?.Value.ToString().Bold();
                    }
                }
            }
            catch (Exception ex) {
                Debug.LogError("SerializedAction " + "Error".Colored("red") + " during SerializedAction creation:".NewLine() +
                    debugMessage.NewLine(2) + "ERROR: ".Bold() + ex.Message.NewLine(2));
                debugMessage = "";
            }
        }
        public static void DebugActionVariables(SerializedAction_Container action, SerializedAction_Container.ActionTimeline selectedTimeline, ref string debugMessage) {
            debugMessage += "Added new action with".NewLine() +
                   "Timeline: " + selectedTimeline.ToString().Bold().NewLine() +
                   "Trigger: " + action.TriggerInput?.name.Bold().NewLine() +
                   "Type: " + action.ClassName.Bold() +
                   "Method: " + action.MethodName.Bold().NewLine() +
                   "Total parameters: " + (action.Parameters?.Count).ToString().Bold().NewLine();
        }
    }
}
