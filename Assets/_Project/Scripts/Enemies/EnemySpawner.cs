using System.Collections.Generic;
using UnityEngine;

namespace CodigoDeEmergencia.Enemies
{
    /// <summary>
    /// Generador procedural de mini-enemigos (50 HP).
    /// Aparecen dinámicamente por la izquierda, la derecha o cayendo del techo.
    /// Incluye logs en consola para depuración y visibilidad de las oleadas.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("--- Prefab del Mini-Enemigo ---")]
        [Tooltip("Prefab del enemigo con 50 HP y barra flotante.")]
        [SerializeField] private GameObject smallEnemyPrefab;

        [Header("--- Intervalos de Generación ---")]
        [Tooltip("Tiempo mínimo entre apariciones (en segundos).")]
        [SerializeField] private float minSpawnInterval = 3.5f;

        [Tooltip("Tiempo máximo entre apariciones.")]
        [SerializeField] private float maxSpawnInterval = 6.0f;

        [Tooltip("Límite máximo de enemigos activos simultáneos en el mapa.")]
        [SerializeField] private int maxActiveEnemies = 6;

        [Header("--- Probabilidad de Caída desde el Techo ---")]
        [Tooltip("Probabilidad (0 a 1) de que el enemigo caiga del techo en vez de aparecer por los lados.")]
        [Range(0f, 1f)]
        [SerializeField] private float ceilingDropChance = 0.35f;

        private Transform playerTransform;
        private float nextSpawnTime;
        private readonly List<GameObject> activeEnemies = new List<GameObject>();

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }

            ScheduleNextSpawn();
        }

        private void Update()
        {
            if (playerTransform == null || smallEnemyPrefab == null) return;

            // Limpiar enemigos destruidos de la lista
            activeEnemies.RemoveAll(e => e == null);

            if (Time.time >= nextSpawnTime && activeEnemies.Count < maxActiveEnemies)
            {
                SpawnEnemy();
                ScheduleNextSpawn();
            }
        }

        private void SpawnEnemy()
        {
            // No generar drones adicionales si el jugador ya entró a la Arena del Jefe Final (X > 154)
            if (playerTransform.position.x > 154f) return;

            Vector3 spawnPos;
            bool dropFromCeiling = Random.value < ceilingDropChance;

            if (dropFromCeiling)
            {
                // Caer desde arriba sobre el campo de visión del jugador
                float offsetX = Random.Range(-4.0f, 4.0f);
                float posX = Mathf.Clamp(playerTransform.position.x + offsetX, -18f, 150f);
                float posY = Mathf.Min(playerTransform.position.y + 5.0f, 19.5f);
                spawnPos = new Vector3(posX, posY, 0f);
                Debug.Log($"<color=yellow><b>[SPAWNER]</b> ¡Mini-drone CAYENDO DEL TECHO en X={spawnPos.x:F1}, Y={spawnPos.y:F1}!</color>");
            }
            else
            {
                // Aparecer por los flancos a la altura actual del jugador
                bool spawnRight = Random.value > 0.35f;
                float distanceX = Random.Range(8.5f, 11.5f);
                float posX = Mathf.Clamp(playerTransform.position.x + (spawnRight ? distanceX : -distanceX), -18f, 150f);
                float posY = Mathf.Clamp(playerTransform.position.y + 1.2f, -1.0f, 18.0f);
                spawnPos = new Vector3(posX, posY, 0f);
                Debug.Log($"<color=yellow><b>[SPAWNER]</b> ¡Mini-drone aproximándose por la {(spawnRight ? "DERECHA" : "IZQUIERDA")} en X={posX:F1}, Y={posY:F1}!</color>");
            }

            GameObject enemy = Instantiate(smallEnemyPrefab, spawnPos, Quaternion.identity);
            activeEnemies.Add(enemy);
        }

        private void ScheduleNextSpawn()
        {
            nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        public void SetEnemyPrefab(GameObject prefab)
        {
            smallEnemyPrefab = prefab;
        }
    }
}
