using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SerializedActions {
    public static class SerializedActions_ExtensionMethods {
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
            string result = str.ToString();
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

    }
}
