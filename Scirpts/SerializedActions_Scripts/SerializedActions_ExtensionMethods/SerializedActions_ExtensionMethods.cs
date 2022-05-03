using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SerializedActions.Extensions {
    public static class SerializedActions_ExtensionMethods {

        private static readonly Type[] TYPES_TO_SEARCH = new Type[] {
           typeof(GameObject),typeof(UnityEngine.Component),
           typeof(UnityEngine.UI.Selectable), typeof(UnityEngine.Object),
           typeof(UnityEngine.CanvasGroup),typeof(UnityEngine.Collider),
           typeof(int), typeof(float), typeof(string),typeof(bool),
            typeof(Int32), typeof(Single), typeof(String),typeof(Boolean)
        };

        // String Extensions
        #region String Extensions
        public static string Bold(this string str) {
            str = "<b>" + str + "</b>";
            return str;
        }
        public static string Bold(this object str) {
            string result = "<b>" + str.ToString() + "</b>";
            return result;
        }
        public static string Colored(this string str, Color color) {
            str = "<color=" + color.ToString().ToLower() + ">" + str + "</color>";
            return str;
        }

        public static string NewLine(this string str, int numberLines = 1) {
            for (int i = 0; i < numberLines + 1; i++) {
                str += '\n';
            }
            return str;
        }
        public static string NewLine(this object str, int numberLines = 1) {
            string result = str == null ? "null" : str.ToString();
            for (int i = 0; i < numberLines + 1; i++) {
                result += '\n';
            }
            str = result;
            return result;
        }
        public static string Comma(this string str) {
            str = str + ", ";
            return str;
        }
        #endregion
        // Type Extensions
        #region Type Extensions
        public static object GetDefaultValue(this Type type) {
            if (type.IsIntType())
                return 0;
            else if (type.IsFloatType())
                return 0.0f;
            else if (type.IsStringType())
                return "";
            else if (type.IsBoolType())
                return false;
            else
                return null;

        }
        public static Type GetTypeFromName(this Type type, string typeName) {
            try {
                type = Type.GetType(typeName);
                if (type == null) {
                    foreach (Type registeredType in TYPES_TO_SEARCH) {
                        type = registeredType.Name == typeName ? registeredType : registeredType.FindTypeInAssembly(typeName) ?? registeredType.FindTypeInModule(typeName);
                        if (type != null)
                            return type;
                    }
                }
                if (type == null)
                    Debug.LogError("Error while trying to get type: " + typeName + " while de-serialising action");
                return type;
            }
            catch (Exception ex) {
                Debug.LogError("Error while trying to get type: " + typeName + " while de-serialising action\n Error: " + ex.Message);
                return null;
            }
        }
        public static Type FindTypeInAssembly(this Type type, string typeNameToFind) {
            return type.Assembly.GetTypes().FirstOrDefault(t => t.Name == typeNameToFind);
        }
        public static Type FindTypeInModule(this Type type, string typeNameToFind) {
            return type.Module.GetTypes().FirstOrDefault(t => t.Name == typeNameToFind);
        }
        // Is of Type
        #region Is-of-type
        public static bool IsPrimitiveType(this Type type) {
            if (type == null)
                return false;
            if (type.IsIntType() || type.IsFloatType() || type.IsStringType() || type.IsBoolType())
                return true;
            return false;
        }
        public static bool IsIntType(this Type type) {
            if (type == null)
                return false;
            if (type.Equals(typeof(int)) || type.Equals(typeof(Int32)))
                return true;
            return false;
        }
        public static bool IsFloatType(this Type type) {
            if (type == null)
                return false;
            if (type == typeof(float) || type == typeof(Single))
                return true;
            return false;
        }
        public static bool IsStringType(this Type type) {
            if (type == null)
                return false;
            if (type == typeof(string) || type == typeof(String))
                return true;
            return false;
        }
        public static bool IsBoolType(this Type type) {
            if (type == null)
                return false;
            if (type == typeof(bool) || type == typeof(Boolean))
                return true;
            return false;
        }
        public static bool IsUnityObjectType(this Type type) {
            if (type == typeof(UnityEngine.Object) || type.IsSubclassOf(typeof(UnityEngine.Object)))
                return true;
            return false;
        }
        #endregion
        #endregion
        public static object[] GetParametersAsSystemObjects(this List<SerializedActions_SerializedParameters> parametersList) {
            object[] result = new object[parametersList.Count];
            for (int i = 0; i < result.Length; i++) {
                result[i] = parametersList[i];
            }
            return result;
        }
    }
}
