using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameController))]
public class GameControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GameController gc = (GameController)target;

        if (GUILayout.Button("Simulate Steps"))
            gc.PredictPositions();

    }
}
