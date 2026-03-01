using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathFindingLinkMonoBehaviour))]
public class PathFindingLinkMonoBehaviourEditor : Editor
{

    private void OnSceneGUI()
    {
        PathFindingLinkMonoBehaviour pathFindingLinkMonoBehaviour = (PathFindingLinkMonoBehaviour)target;

        EditorGUI.BeginChangeCheck();
        Vector3 newLinkPositionA = Handles.PositionHandle(pathFindingLinkMonoBehaviour.linkPositionA, Quaternion.identity);
        Vector3 newLinkPositionB = Handles.PositionHandle(pathFindingLinkMonoBehaviour.linkPositionB, Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(pathFindingLinkMonoBehaviour, "Change link position");
            pathFindingLinkMonoBehaviour.linkPositionA = newLinkPositionA;
            pathFindingLinkMonoBehaviour.linkPositionB = newLinkPositionB;

        }
    }
}
