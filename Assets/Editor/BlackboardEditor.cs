using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Blackboard))]
public class BlackboardEditor : Editor
{
	private string asd;
	private bool toggle = true;

	public override bool RequiresConstantRepaint()
	{
		return true;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		base.OnInspectorGUI();

		var blackboard = (Blackboard)serializedObject.targetObject;

		toggle = EditorGUILayout.Foldout(toggle, "Entries");
		if (toggle)
		{
			foreach (var row in blackboard.ReadAll())
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.TextField(row.Key);
				if (row.Data is Object)
				{
					EditorGUILayout.ObjectField(row.Data as Object, typeof(Object), true);
				}
				else
				{
					EditorGUILayout.TextField(row.Data.ToString());
				}
				EditorGUILayout.EndHorizontal();
			}
		}
	}
}