using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AttributeUsage(AttributeTargets.Method)]
public class BaseImplementationMethodAttribute : Attribute {
    [SerializeField]
    private int id = 0;
    public int MethodID => id;

    public BaseImplementationMethodAttribute(int ID) {
        id = ID;
    }
}
