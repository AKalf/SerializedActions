using System;
using UnityEngine;
using SerializedActions.Extensions;
namespace SerializedActions {

    [Serializable]
    public class SerializedActions_SerializedParameters {
        public enum SupportedTypes { UnityObject, Int, Float, String, Bool }
        [SerializeField] public string ParameterName = "";
        [SerializeField] public string ParameterTypeName = "";
        [NonSerialized] private Type parameterType = null;
        public virtual Type ParameterType {
            get {
                if (parameterType == null)
                    parameterType = parameterType.GetTypeFromName(ParameterTypeName);
                return parameterType;
            }
            //set {
            //    parameterType = value;
            //    ParameterTypeName = parameterType.Name;
            //}
        }
        [SerializeField][HideInInspector] private SupportedTypes thisType = SupportedTypes.UnityObject;
        public SupportedTypes ThisType => thisType;
        [SerializeField] private int intValue = 0;
        [SerializeField] private float floatValue = 0;
        [SerializeField] private string stringValue = "";
        [SerializeField] private bool boolValue = false;
        [SerializeField] private UnityEngine.Object objectValue = null;
        public virtual object Value {
            get {
                switch (thisType) {
                    case SupportedTypes.Int: return intValue;
                    case SupportedTypes.Float: return floatValue;
                    case SupportedTypes.String: return stringValue;
                    case SupportedTypes.Bool: return boolValue;
                    case SupportedTypes.UnityObject: return objectValue;
                }
                return null;
            }
            set {
                Type type = value.GetType();
                if (type.IsIntType()) intValue = (int)value;
                else if (type.IsFloatType()) floatValue = (float)value;
                else if (type.IsStringType()) stringValue = (string)value;
                else if (type.IsBoolType()) boolValue = (bool)value;
                else objectValue = (UnityEngine.Object)value;
            }
        }

        private SerializedActions_SerializedParameters() { }
        public static SerializedActions_SerializedParameters CreateSerializedParameter(string parameterName, Type type, object value) {
            SerializedActions_SerializedParameters newParam = new SerializedActions_SerializedParameters();
            if (type.IsIntType()) {
                newParam.thisType = SupportedTypes.Int;
                newParam.parameterType = typeof(int);
                newParam.ParameterTypeName = newParam.parameterType.Name;
                newParam.ParameterName = parameterName;
                newParam.intValue = value == null || value.GetType() == typeof(DBNull) ? 0 : Convert.ToInt32(value);
                return newParam;
            }

            else if (type.IsFloatType()) {
                newParam.thisType = SupportedTypes.Float;
                newParam.parameterType = typeof(float);
                newParam.ParameterTypeName = newParam.parameterType.Name;
                newParam.ParameterName = parameterName;
                newParam.floatValue = value == null || value.GetType() == typeof(DBNull) ? 0.0f : (float)value;
                return newParam;
            }
            else if (type.IsStringType()) {
                newParam.thisType = SupportedTypes.String;
                newParam.parameterType = typeof(string);
                newParam.ParameterTypeName = newParam.parameterType.Name;
                newParam.ParameterName = parameterName;
                newParam.stringValue = value == null || value.GetType() == typeof(DBNull) ? "" : (string)value;
                return newParam;
            }
            else if (type.IsBoolType()) {
                newParam.thisType = SupportedTypes.Bool;
                newParam.parameterType = typeof(bool);
                newParam.ParameterTypeName = newParam.parameterType.Name;
                newParam.ParameterName = parameterName;
                newParam.boolValue = value == null || value.GetType() == typeof(DBNull) ? false : (bool)value;
                return newParam;
            }
            else {
                newParam.thisType = SupportedTypes.UnityObject;
                newParam.parameterType = type;
                newParam.ParameterTypeName = newParam.parameterType.Name;
                newParam.ParameterName = parameterName;
                newParam.objectValue = value == null || value.GetType() == typeof(DBNull) ? null : Convert.ChangeType(value, type) as UnityEngine.Object;
                return newParam;
            }
        }

    }
}
