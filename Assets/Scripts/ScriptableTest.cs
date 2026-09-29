using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New obj", menuName = "Scriptable")]
public class ScriptableTest : ScriptableObject
{
    public string testName = "Some name";
}

[CreateAssetMenu(fileName = "New obj", menuName = "Scriptable1")]
public class ScriptableTest1 : ScriptableObject
{
    public string testName = "Some name 1";
    public GameObject g;
}