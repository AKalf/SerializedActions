using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SerializedActions_InspectorElements {

    public static class Consts {
        public const int paramNameWidth = 150;
        public const int primitiveTypeWidth = 40;

        public const int numbersFieldWidth = 70;
        public const int stringFieldWidth = 200;

        public const int MinWidth = 15;
        public const int MaxWidth = 350;
        public const int objectFieldMaxWidth = 700;
    }

    public struct Element {
        public GUIStyle Style;
        public GUILayoutOption[] Options;

        public Element(GUIStyle style, GUILayoutOption[] options) {
            Style = style;
            Options = options;
        }
    }

    private static Element paramName = default;
    public static Element ParamName {
        get {
            if (paramName.Equals(default(Element)))
                paramName = new Element(Styles.ParamName, Options.ParamNames);
            return paramName;
        }
    }
    private static Element primitiveType = default;
    public static Element PrimitiveType {
        get {
            if (primitiveType.Equals(default(Element)))
                primitiveType = new Element(Styles.PrimitiveType, Options.PrimitiveType);
            return primitiveType;
        }
    }

    private static Element numbersField = default;
    public static Element NumbersField {
        get {
            if (numbersField.Equals(default(Element)))
                numbersField = new Element(Styles.NumbersField, Options.NumbersField);
            return numbersField;
        }
    }

    private static Element stringField = default;
    public static Element StringField {
        get {
            if (stringField.Equals(default(Element)))
                stringField = new Element(Styles.StringField, Options.StringField);
            return stringField;
        }
    }

    private static Element boolField = default;
    public static Element BoolField {
        get {
            if (boolField.Equals(default(Element)))
                boolField = new Element(Styles.BoolField, Options.BoolField);
            return boolField;
        }
    }
    private static Element objectField = default;
    public static Element ObjectField {
        get {
            if (objectField.Equals(default(Element)))
                objectField = new Element(Styles.ObjectField, Options.ObjectField);
            return objectField;
        }
    }

    private static Element infoField = default;
    public static Element InfoField {
        get {
            if (infoField.Equals(default(Element)))
                infoField = new Element(Styles.InfoField, Options.InfoField);
            return infoField;
        }
    }

    private static class Options {
        public static GUILayoutOption[] ParamNames = {
              GUILayout.Width(Consts.paramNameWidth), GUILayout.ExpandWidth(false)
        };

        public static GUILayoutOption[] PrimitiveType = {
            GUILayout.Width(Consts.primitiveTypeWidth), GUILayout.ExpandWidth(false)
        };

        public static GUILayoutOption[] NumbersField = {
            GUILayout.Width(Consts.numbersFieldWidth), GUILayout.ExpandWidth(false)
        };
        public static GUILayoutOption[] StringField = {
            GUILayout.ExpandWidth(true)
        };
        public static GUILayoutOption[] BoolField = {
            GUILayout.ExpandWidth(true), GUILayout.MaxWidth(230), GUILayout.MinWidth(20)
        };
        public static GUILayoutOption[] ObjectField = {
             GUILayout.ExpandWidth(true)
        };
        public static GUILayoutOption[] InfoField = {
             GUILayout.ExpandWidth(true)
        };
    }

    private static class Styles {
        static GUIStyle paramName = null;
        public static GUIStyle ParamName {
            get {
                if (paramName == null) {
                    paramName = new GUIStyle();
                    paramName.alignment = TextAnchor.MiddleLeft;
                    paramName.clipping = TextClipping.Overflow;
                    paramName.fixedWidth = Consts.paramNameWidth;
                    paramName.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                    paramName.fontStyle = FontStyle.Bold;
                    paramName.stretchWidth = false;
                }
                return paramName;

            }
        }

        static GUIStyle primitiveType = null;
        public static GUIStyle PrimitiveType {
            get {
                if (primitiveType == null) {
                    primitiveType = new GUIStyle();
                    primitiveType.alignment = TextAnchor.MiddleLeft;
                    primitiveType.clipping = TextClipping.Overflow;
                    primitiveType.fixedWidth = Consts.primitiveTypeWidth;
                    primitiveType.normal.textColor = EditorStyles.label.normal.textColor;
                    primitiveType.stretchWidth = false;
                }
                return primitiveType;
            }
        }

        static GUIStyle numbersField = null;
        public static GUIStyle NumbersField {
            get {
                if (numbersField == null) {
                    numbersField = new GUIStyle(EditorStyles.numberField);
                    numbersField.alignment = TextAnchor.MiddleRight;
                    numbersField.clipping = TextClipping.Overflow;
                    numbersField.fixedWidth = Consts.numbersFieldWidth;
                    numbersField.normal.textColor = new Color(0, 0.75f, 1);
                    numbersField.stretchWidth = false;
                }
                return numbersField;
            }
        }

        static GUIStyle stringField = null;
        public static GUIStyle StringField {
            get {
                if (stringField == null) {
                    stringField = new GUIStyle(EditorStyles.textField);
                    stringField.alignment = TextAnchor.MiddleLeft;
                    stringField.clipping = TextClipping.Overflow;
                    stringField.fixedWidth = Consts.stringFieldWidth;
                    stringField.normal.textColor = new Color(1, 0.8f, 0.2f);
                    stringField.stretchWidth = false;
                }
                return stringField;
            }
        }

        static GUIStyle boolField = null;
        public static GUIStyle BoolField {
            get {
                if (boolField == null) {
                    boolField = new GUIStyle();
                    boolField.alignment = TextAnchor.MiddleLeft;
                    boolField.clipping = TextClipping.Overflow;
                    boolField.normal.textColor = Color.white;
                    boolField.stretchWidth = false;
                }
                return boolField;
            }
        }

        static GUIStyle objectField = null;
        public static GUIStyle ObjectField {
            get {
                if (objectField == null) {
                    objectField = new GUIStyle(EditorStyles.objectFieldThumb);
                    objectField.clipping = TextClipping.Overflow;
                    objectField.normal.textColor = Color.white;
                    objectField.stretchWidth = false;
                }
                return boolField;
            }
        }

        static GUIStyle info = null;
        public static GUIStyle InfoField {
            get {
                if (info == null) {
                    info = new GUIStyle(EditorStyles.textArea);
                    info.clipping = TextClipping.Overflow;
                    info.alignment = TextAnchor.MiddleCenter;
                    info.normal.textColor = Color.white;
                    info.fontSize += 3;
                    info.stretchWidth = false;
                    info.richText = true;
                }
                return info;
            }
        }
    }




    private static T[] GetArrayCopy<T>(T[] array) {
        T[] newArray = new T[array.Length];
        array.CopyTo(newArray, 0);
        return newArray;
    }

    public static void HorizontalLine(Color color, float height, float width, Vector2 margin) {
        GUILayout.Space(margin.x);

        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, height, GUILayout.MaxWidth(width)), color);

        GUILayout.Space(margin.y);
    }
}
