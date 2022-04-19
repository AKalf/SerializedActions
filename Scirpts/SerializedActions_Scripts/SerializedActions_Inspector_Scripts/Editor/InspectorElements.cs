using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SerializedActions.Editors {
    public static class InspectorElements {
        private static Element paramName = default, primitiveType = default, numbersField = default, stringField = default, boolField = default,
            objectField = default, infoField = default;
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
        // Custom Elements Get Properties
        #region Custom Elements Get Properties
        public static Element ParamName => paramName.Equals(default(Element)) ? paramName = new Element(Styles.ParamName, Options.ParamNames) : paramName;
        public static Element PrimitiveType => primitiveType.Equals(default(Element)) ? primitiveType = new Element(Styles.PrimitiveType, Options.PrimitiveType) : primitiveType;
        public static Element NumbersField => numbersField.Equals(default(Element)) ? numbersField = new Element(Styles.NumbersField, Options.NumbersField) : numbersField;
        public static Element StringField => stringField.Equals(default(Element)) ? stringField = new Element(Styles.StringField, Options.StringField) : stringField;
        public static Element BoolField => boolField.Equals(default(Element)) ? boolField = new Element(Styles.BoolField, Options.BoolField) : boolField;
        public static Element ObjectField => objectField.Equals(default(Element)) ? objectField = new Element(Styles.ObjectField, Options.ObjectField) : ObjectField;
        public static Element InfoField => infoField.Equals(default(Element)) ? infoField = new Element(Styles.InfoField, Options.InfoField) : infoField;
        #endregion

        private static class Options {
            public static GUILayoutOption[]
                ParamNames = { GUILayout.Width(Consts.paramNameWidth), GUILayout.ExpandWidth(false) },
                PrimitiveType = { GUILayout.Width(Consts.primitiveTypeWidth), GUILayout.ExpandWidth(false) },
                NumbersField = { GUILayout.Width(Consts.numbersFieldWidth), GUILayout.ExpandWidth(false) },
                StringField = { GUILayout.ExpandWidth(true) },
                BoolField = { GUILayout.ExpandWidth(true), GUILayout.MaxWidth(230), GUILayout.MinWidth(20) },
                ObjectField = { GUILayout.ExpandWidth(true) },
                InfoField = { GUILayout.ExpandWidth(true) };
        }

        private static class Styles {
            private static GUIStyle paramName = null, primitiveType = null, numbersField = null, stringField = null, objectField = null, info = null;
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

        public static void HorizontalLine(Color color, float height, float width, Vector2 margin) {
            GUILayout.Space(margin.x);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, height, GUILayout.MaxWidth(width)), color);
            GUILayout.Space(margin.y);
        }
    }
}
