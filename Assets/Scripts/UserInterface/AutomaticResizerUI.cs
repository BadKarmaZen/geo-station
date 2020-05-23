using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutomaticResizer))]
public class AutomaticResizerUI : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    if(GUILayout.Button("Adjust Size"))
    {
      (target as AutomaticResizer)?.AdjustSize();
    }
  }
}
