using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMethods : MonoBehaviour {
    [BaseImplementationMethod(0)]
    public static void DebugGameobjectOrsOMEName(bool b, float c, int var) {
        Debug.Log(b);
    }
    [BaseImplementationMethod(1)]
    public static void DebugTheOther(int number, string aString, bool b, GameObject collider) {
        Debug.Log("number " + number);
    }
}
