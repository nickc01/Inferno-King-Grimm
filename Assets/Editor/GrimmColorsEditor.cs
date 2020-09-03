using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrimmColors))]
public class GrimmColorsEditor : Editor
{
	public override void OnInspectorGUI()
	{
		//Debug.Log("ONGUI");
		//var red = serializedObject.FindProperty("hueShiftRed");
		//var green = serializedObject.FindProperty("hueShiftGreen");
		//var blue = serializedObject.FindProperty("hueShiftBlue");

		//Vector3 oldValues = new Vector3(red.floatValue,green.floatValue,blue.floatValue);
		//Debug.Log("Old Values = " + oldValues);
		base.OnInspectorGUI();
		//Vector3 newValues = new Vector3(red.floatValue, green.floatValue, blue.floatValue);
		//Debug.Log("Old Values = " + oldValues.x);
		//Debug.Log("New Values = " + newValues.x);
		((GrimmColors)target).Refresh();
	}
}
