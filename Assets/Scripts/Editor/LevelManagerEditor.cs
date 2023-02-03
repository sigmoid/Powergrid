using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		LevelManager levelManager = (LevelManager)target;

		if (GUILayout.Button("Generate Lines"))
		{
			levelManager.GenerateLines();
		}
		base.OnInspectorGUI();
	}
}
