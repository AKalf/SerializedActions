using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMethods : MonoBehaviour {
    [BaseImplementationMethod(0)]
    public static void DebugGameobjectName(bool b, string s, int a) {
        Debug.Log(b);
    }
    [BaseImplementationMethod(1)]
    public static void DebugOther(int number, string aString, bool b, GameObject collider) {
        Debug.Log("number " + number);
    }
}
