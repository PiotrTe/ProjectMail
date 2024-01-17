using UnityEngine;
using System.Collections.Generic; // For using List

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    private List<GameObject> enemies = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
        Debug.Log("Enemy added");
    }

    public void RemoveEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);
        Debug.Log("Enemy removed");
    }

    public GameObject[] GetEnemies()
    {
        return enemies.ToArray();
    }
}
