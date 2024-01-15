using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "PatrolRoute", menuName = "AI/Patrol Route", order = 1)]

public class CheckpointPlacerEditor : EditorWindow
{
    private GameObject _checkpointPrefab;
    private GameObject _patrolRoutePrefab; // Add this line
    private GameObject _patrolRouteInstance; // To store the instance
    private GameObject _currentPatrolRoute; // The current instance of the patrol route
    private int checkpointCounter = 0; // Counter for naming checkpoints
    private bool _placingMode = false;

    [MenuItem("Tools/Checkpoint Placer")]
    public static void ShowWindow()
    {
        GetWindow<CheckpointPlacerEditor>("Checkpoint Placer");
    }
    void OnEnable()
    {
        // Load the patrol route prefab and checkpoint prefab by name in the assets
        LoadPrefabs();
    }
    private void LoadPrefabs()
    {
        _checkpointPrefab = LoadPrefabByName("Checkpoint");
        _patrolRoutePrefab = LoadPrefabByName("PatrolRoute");

        if (_checkpointPrefab == null)
        {
            Debug.LogWarning("Checkpoint prefab not found.");
        }

        if (_patrolRoutePrefab == null)
        {
            Debug.LogWarning("PatrolRoutePrefab not found.");
        }
    }
    private GameObject LoadPrefabByName(string prefabName)
    {
        string[] guids = AssetDatabase.FindAssets(prefabName + " t:prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) return prefab;
        }
        return null;
    }
    void OnGUI()
    {
        GUILayout.Label("Place Checkpoints", EditorStyles.boldLabel);

        // Display fields for prefabs (optional, to show what's loaded)
        _checkpointPrefab = (GameObject)EditorGUILayout.ObjectField("Checkpoint Prefab", _checkpointPrefab, typeof(GameObject), false);
        _patrolRoutePrefab = (GameObject)EditorGUILayout.ObjectField("Patrol Route Prefab", _patrolRoutePrefab, typeof(GameObject), false);

        // Toggle for placing mode
        bool newPlacingMode = GUILayout.Toggle(_placingMode, "Checkpoint Placing Mode", "Button");

        if (newPlacingMode && !_placingMode)
        {
            // Instantiatea new patrol route from the prefab
            if (_patrolRoutePrefab != null)
            {
                _currentPatrolRoute = Instantiate(_patrolRoutePrefab);
                Undo.RegisterCreatedObjectUndo(_currentPatrolRoute, "Create Patrol Route");
            }
            else
            {
                Debug.LogError("Patrol Route Prefab is not assigned. Please assign a valid prefab.");
            }
        }_placingMode = newPlacingMode;

        if (_placingMode)
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }
        else
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }
    void OnSceneGUI(SceneView sceneView)
    {
        if (!_placingMode)
            return;

        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (_currentPatrolRoute == null && _patrolRoutePrefab != null)
                {
                    _currentPatrolRoute = Instantiate(_patrolRoutePrefab);
                    Undo.RegisterCreatedObjectUndo(_currentPatrolRoute, "Create Patrol Route");
                    checkpointCounter = 0; // Reset the counter for each new patrol route
                }

                GameObject newCheckpoint = PrefabUtility.InstantiatePrefab(_checkpointPrefab) as GameObject;
                if (newCheckpoint != null)
                {
                    newCheckpoint.transform.position = hit.point;
                    newCheckpoint.transform.SetParent(_currentPatrolRoute.transform, true);
                    newCheckpoint.name = "Checkpoint " + checkpointCounter; // Assign a unique name
                    Undo.RegisterCreatedObjectUndo(newCheckpoint, "Create Checkpoint");

                    if (_currentPatrolRoute.transform.childCount == 1)
                    {
                        _currentPatrolRoute.transform.position = newCheckpoint.transform.position;
                        newCheckpoint.transform.localPosition = Vector3.zero; // Reset local position in case the patrol route moves
                    }

                    checkpointCounter++; // Increment the counter for the next checkpoint
                }

                e.Use(); // Consume the event to prevent it from propagating
            }
        }
    }
}
