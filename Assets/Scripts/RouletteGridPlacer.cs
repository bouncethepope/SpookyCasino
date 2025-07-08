using UnityEngine;
using UnityEditor;

public class RouletteGridPlacer : EditorWindow
{
    private Vector2 startPosition = Vector2.zero;
    private Vector2 spacing = new Vector2(2.0f, 2.0f);

    [MenuItem("Tools/Roulette Grid Placer")]
    public static void ShowWindow()
    {
        GetWindow<RouletteGridPlacer>("Roulette Grid Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        startPosition = EditorGUILayout.Vector2Field("Start Position", startPosition);
        spacing = EditorGUILayout.Vector2Field("Spacing", spacing);

        if (GUILayout.Button("Place Selected Numbers"))
        {
            PlaceSelected();
        }
    }

    private void PlaceSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected.");
            return;
        }

        // Sort by the leading number from name (e.g., "13_0" => 13)
        System.Array.Sort(selectedObjects, (a, b) =>
        {
            int numA = ExtractLeadingNumber(a.name);
            int numB = ExtractLeadingNumber(b.name);
            return numA.CompareTo(numB);
        });

        int totalRows = Mathf.CeilToInt(selectedObjects.Length / 3f);

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            int row = i / 3;
            int column = i % 3;

            // Flip Y: bottom row becomes row 0
            int invertedRow = (totalRows - 1) - row;

            Vector2 newPos = startPosition + new Vector2(column * spacing.x, invertedRow * spacing.y);
            Undo.RecordObject(selectedObjects[i].transform, "Place Number");
            selectedObjects[i].transform.position = newPos;
        }

        Debug.Log("Roulette numbers placed using correct mirrored layout.");
    }

    // Extract the leading number from GameObject name, e.g., "14_0" => 14
    private int ExtractLeadingNumber(string name)
    {
        string[] parts = name.Split('_');
        if (parts.Length == 0)
        {
            Debug.LogWarning($"Invalid name format: {name}");
            return 0;
        }

        if (int.TryParse(parts[0], out int number))
        {
            return number;
        }

        Debug.LogWarning($"Failed to parse number from: {name}");
        return 0;
    }
}
