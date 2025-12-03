using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace PixelGame
{
    /// <summary>
    /// Represents a single room in the dungeon
    /// Handles enemy spawning, door management, and room state
    /// </summary>
    public class Room : MonoBehaviour
    {
        [Header("Room Info")]
        public RoomType roomType = RoomType.Combat;
        public int roomDifficulty = 1;

        [Header("Doors")]
        public List<Door> doors = new List<Door>();

        [Header("Enemy Spawning")]
        public List<Transform> enemySpawnPoints = new List<Transform>();
        public List<EnemyWave> enemyWaves = new List<EnemyWave>();

        [Header("Rewards")]
        public List<Transform> rewardSpawnPoints = new List<Transform>();
        public List<GameObject> rewardPrefabs = new List<GameObject>();

        [Header("Room Bounds")]
        public BoxCollider2D roomBounds;

        // State
        private bool isCleared = false;
        private bool isActive = false;
        private int currentWave = 0;
        private int enemiesRemaining = 0;
        private List<GameObject> spawnedEnemies = new List<GameObject>();

        public enum RoomType
        {
            Combat,
            Boss,
            Shop,
            Treasure,
            Event,
            Blacksmith,
            Rest
        }

        [System.Serializable]
        public class EnemyWave
        {
            public List<GameObject> enemies = new List<GameObject>();
            public float spawnDelay = 0.5f;
        }

        private void Start()
        {
            // Find room bounds if not assigned
            if (roomBounds == null)
            {
                roomBounds = GetComponentInChildren<BoxCollider2D>();
            }

            // Subscribe to events
            GameEvents.OnEnemyKilled += HandleEnemyKilled;
        }

        private void OnDestroy()
        {
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && !isActive)
            {
                OnPlayerEnter();
            }
        }

        public void OnPlayerEnter()
        {
            isActive = true;
            GameEvents.RoomEntered(this);

            // Handle based on room type
            switch (roomType)
            {
                case RoomType.Combat:
                    if (!isCleared)
                    {
                        CloseDoors();
                        StartCoroutine(SpawnWaveCoroutine(currentWave));
                    }
                    break;

                case RoomType.Boss:
                    if (!isCleared)
                    {
                        CloseDoors();
                        StartCoroutine(SpawnWaveCoroutine(0));
                    }
                    break;

                case RoomType.Treasure:
                    SpawnRewards();
                    break;

                case RoomType.Shop:
                    // Open shop UI
                    break;
            }
        }

        private IEnumerator SpawnWaveCoroutine(int waveIndex)
        {
            if (waveIndex >= enemyWaves.Count) yield break;

            EnemyWave wave = enemyWaves[waveIndex];

            foreach (var enemyPrefab in wave.enemies)
            {
                SpawnEnemy(enemyPrefab);
                yield return new WaitForSeconds(wave.spawnDelay);
            }
        }

        private void SpawnEnemy(GameObject enemyPrefab)
        {
            if (enemyPrefab == null || enemySpawnPoints.Count == 0) return;

            Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Count)];
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            enemy.transform.SetParent(transform);

            spawnedEnemies.Add(enemy);
            enemiesRemaining++;
        }

        private void HandleEnemyKilled(EnemyBase enemy)
        {
            if (!spawnedEnemies.Contains(enemy.gameObject)) return;

            spawnedEnemies.Remove(enemy.gameObject);
            enemiesRemaining--;

            if (enemiesRemaining <= 0)
            {
                currentWave++;

                if (currentWave < enemyWaves.Count)
                {
                    // Start next wave
                    StartCoroutine(SpawnWaveCoroutine(currentWave));
                }
                else
                {
                    // Room cleared
                    OnRoomCleared();
                }
            }
        }

        private void OnRoomCleared()
        {
            isCleared = true;

            OpenDoors();
            SpawnRewards();

            GameEvents.RoomCleared();

            // Visual feedback
            // Spawn cleared particles, play sound, etc.
        }

        private void CloseDoors()
        {
            foreach (var door in doors)
            {
                if (door != null)
                {
                    door.Close();
                }
            }
        }

        private void OpenDoors()
        {
            foreach (var door in doors)
            {
                if (door != null)
                {
                    door.Open();
                }
            }
        }

        private void SpawnRewards()
        {
            if (rewardPrefabs.Count == 0 || rewardSpawnPoints.Count == 0) return;

            for (int i = 0; i < rewardPrefabs.Count && i < rewardSpawnPoints.Count; i++)
            {
                if (rewardPrefabs[i] != null)
                {
                    Instantiate(rewardPrefabs[i], rewardSpawnPoints[i].position, Quaternion.identity);
                }
            }
        }

        /// <summary>
        /// Clean up room when moving to next floor
        /// </summary>
        public void Cleanup()
        {
            foreach (var enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }

            spawnedEnemies.Clear();
        }

        private void OnDrawGizmos()
        {
            // Draw room bounds
            if (roomBounds != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(roomBounds.transform.position, roomBounds.size);
            }

            // Draw spawn points
            Gizmos.color = Color.red;
            foreach (var point in enemySpawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.3f);
                }
            }

            // Draw reward points
            Gizmos.color = Color.yellow;
            foreach (var point in rewardSpawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.3f);
                }
            }
        }
    }
}
