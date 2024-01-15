using UnityEditor;
using UnityEngine;

public class CheckpointPlacerEditor : EditorWindow
{
    private GameObject _checkpointPrefab;
    private GameObject _patrolRoutePrefab;
    private GameObject _currentPatrolRoute;
    private GameObject _enemyPrefab; // Enemy prefab
    private int _numberOfEnemies = 3; // Number of enemies
    private int checkpointCounter = 0;
    private bool _placingMode = false;

    [MenuItem("Tools/Checkpoint Placer")]
    public static void ShowWindow()
    {
        GetWindow<CheckpointPlacerEditor>("Checkpoint Placer");
    }
    void OnEnable()
    {
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

        // Add fields to select enemy prefab and number of enemies.
        _enemyPrefab = (GameObject)EditorGUILayout.ObjectField("Enemy Prefab", _enemyPrefab, typeof(GameObject), false);
        _numberOfEnemies = EditorGUILayout.IntField("Number of Enemies", _numberOfEnemies);

        _placingMode = GUILayout.Toggle(_placingMode, "Toggle Placing Mode", "Button");

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
                    checkpointCounter = 1; // Reset the counter for each new patrol route
                    Undo.RegisterCreatedObjectUndo(_currentPatrolRoute, "Create Patrol Route");
                    PatrolRoute patrolRouteScript = _currentPatrolRoute.GetComponent<PatrolRoute>();
                    if (patrolRouteScript != null)
                    {
                        patrolRouteScript.enemyPrefab = _enemyPrefab;
                        patrolRouteScript.numberOfEnemies = _numberOfEnemies;
                    }
                    
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
