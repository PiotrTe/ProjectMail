using UnityEngine;
using System.Collections.Generic; // For using List

public class EnemyManager : MonoBehaviour
{
    // Singleton instance
    public static EnemyManager Instance { get; private set; }

    // List to hold all enemies
    private List<GameObject> _enemies = new List<GameObject>();

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to add an enemy to the list
    public void AddEnemy(GameObject enemy)
    {
        if (!_enemies.Contains(enemy))
        {
            _enemies.Add(enemy);
        }
    }

    // Method to remove an enemy from the list
    public void RemoveEnemy(GameObject enemy)
    {
        if (_enemies.Contains(enemy))
        {
            _enemies.Remove(enemy);
        }
    }

    // Method to get the list of all enemies
    public List<GameObject> GetAllEnemies()
    {
        return _enemies;
    }
}