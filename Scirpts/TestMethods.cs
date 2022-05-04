using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMethods : MonoBehaviour {
    [BaseImplementationMethod(0)]
    public static void DebugGameobjectNameb(bool b, float c, int var) {
        Debug.Log(b);
    }
    [BaseImplementationMethod(1)]
    public static void ThisIsMethod_A(int number, Collider collider) {
        Debug.Log("number " + number);
    }
}
