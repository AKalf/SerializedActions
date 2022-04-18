using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Elements = SerializedActions_InspectorElements;
public class EditorDrawing {
    /// <summary>Draw a field for an object of primitive type (int, float, bool, string) </summary>
    /// <param name="obj">The actual object</param> 
    /// <param name="type">The type of the object (int, float, bool, string are supported)</param>
    /// <param name="paramName">The name of the field</param>
    /// <returns>Returns the updated value of the field</returns>
    public static System.Object PrimitiveField(
        // Method parameters
        System.Object obj,
        System.Type type,
        string paramName) {
        // BODY

        if (type == typeof(int) || type == typeof(System.Int32)) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Int: ", Elements.PrimitiveType.Style, Elements.PrimitiveType.Options);
            EditorGUILayout.LabelField(paramName, Elements.ParamName.Style, Elements.ParamName.Options);
            int value = 0;
            value = EditorGUILayout.IntField(obj != null ? (System.Int32)obj : value, Elements.NumbersField.Style, Elements.NumbersField.Options);
            EditorGUILayout.EndHorizontal();
            return value;
        }
        else if (type == typeof(float)) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Float: ", Elements.PrimitiveType.Style, Elements.PrimitiveType.Options);
            EditorGUILayout.LabelField(paramName, Elements.ParamName.Style, Elements.ParamName.Options);
            float value = 0.0f;
            value = EditorGUILayout.FloatField(obj != null ? (float)obj : value, Elements.NumbersField.Style, Elements.NumbersField.Options);
            EditorGUILayout.EndHorizontal();
            return value;
        }
        else if (type == typeof(string) || type == typeof(System.String)) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("String: ", Elements.PrimitiveType.Style, Elements.PrimitiveType.Options);
            EditorGUILayout.LabelField(paramName, Elements.ParamName.Style, Elements.ParamName.Options);
            string value = "";
            value = EditorGUILayout.TextField(obj != null ? (System.String)obj : value, Elements.StringField.Style, Elements.StringField.Options);
            EditorGUILayout.EndHorizontal();
            return value;
        }
        else if (type == typeof(bool) || type == typeof(System.Boolean)) {
            bool value = false;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(paramName, Elements.BoolField.Style, Elements.BoolField.Options);
            value = EditorGUILayout.Toggle(obj != null ? (System.Boolean)obj : value);
            EditorGUILayout.EndHorizontal();
            return value;
        }
        else {
            Debug.LogError("SerializedActions Inpector <color=Red>ERROR</color>: Dont know how to draw primitive type: " + type?.Name);
            return null;
        }

    }

    /// <summary>Draw a field for an object of UnityEngine.Object type</summary>
    /// <param name="obj">The actual object</param> 
    /// <param name="type">The type of the object (should derive from UnityEngine.Object)</param>
    /// <param name="paramName">The name of the field</param>
    /// <returns>Returns the updated value of the field</returns>
    public static T UnityObjectField<T>(
        // Method parameters
        UnityEngine.Object obj,
        Type type,
        string paramName,
        bool shouldShowName = true,
        bool shouldShowType = true) where T : class {
        // BODY

        EditorGUILayout.BeginHorizontal();
        if (shouldShowName) {
            EditorGUILayout.LabelField("Name: ", Elements.PrimitiveType.Style, Elements.PrimitiveType.Options);
            EditorGUILayout.LabelField(paramName, Elements.ParamName.Style, Elements.ParamName.Options);
        }
        if (shouldShowType) {
            EditorGUILayout.LabelField("Type: ", Elements.PrimitiveType.Style, Elements.PrimitiveType.Options);
            EditorGUILayout.LabelField(type.Name, Elements.ParamName.Style, Elements.ParamName.Options);
        }
        EditorGUILayout.EndHorizontal();

        return EditorGUILayout.ObjectField(obj, type, true, Elements.ObjectField.Options) as T;

    }

}
