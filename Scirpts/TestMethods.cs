using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMethods : MonoBehaviour {
    [BaseImplementationMethod(0)]
    public static void DebugGameobjectName(GameObject gameObject) {
        Debug.Log(gameObject.name);
    }
}
