using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodigoDeEmergencia.Core;
using CodigoDeEmergencia.Player;

namespace CodigoDeEmergencia.Enemies
{
    /// <summary>
    /// Generador procedural de mini-enemigos (25 HP en escala 0-100).
    /// Valida mediante Raycast que el punto de aparición esté sobre suelo válido sin atascarse en muros
    /// y reproduce un efecto suave de materialización holográfica antes de activar al dron.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("--- Prefab del Mini-Enemigo ---")]
        [Tooltip("Prefab del enemigo con 25 HP y barra flotante.")]
        [SerializeField] private GameObject smallEnemyPrefab;

        [Header("--- Intervalos de Generación Terrestre por Proximidad ---")]
        [Tooltip("Tiempo mínimo entre apariciones de refuerzo terrestre (en segundos).")]
        [SerializeField] private float minSpawnInterval = 6.0f;

        [Tooltip("Tiempo máximo entre apariciones de refuerzo terrestre.")]
        [SerializeField] private float maxSpawnInterval = 9.0f;

        [Tooltip("Límite máximo de enemigos activos simultáneos cercanos al jugador.")]
        [SerializeField] private int maxActiveEnemies = 5;

        [Tooltip("Multiplicador de vida de los drones según el nivel (1.0 = Nivel 1, +20% por nivel).")]
        [SerializeField] private float difficultyMultiplier = 1.0f;

        [Tooltip("Coordenada X a partir de la cual inicia la zona de seguridad previa al Jefe y no se generan más drones.")]
        [SerializeField] private float bossZoneStartX = 1135f;

        [Header("--- Activación por Proximidad a lo largo del Mapa (Sin Caída del Cielo) ---")]
        [Range(0f, 1f)]
        [SerializeField] private float ceilingDropChance = 0f;

        [Tooltip("Distancia horizontal a la que se activan los enemigos distribuidos a lo largo del mapa conforme avanza el jugador.")]
        [SerializeField] private float proximityActivationDistance = 17.5f;

        public float CeilingDropChance => ceilingDropChance;

        private Transform playerTransform;
        private float nextSpawnTime;
        private bool spawnerLockedByBoss;
        private readonly List<GameObject> activeEnemies = new List<GameObject>();

        private struct MapEncounterPoint
        {
            public Vector2 position;
            public bool triggered;
        }

        private List<MapEncounterPoint> mapEncounters = new List<MapEncounterPoint>();

        private void OnEnable()
        {
            GameEvents.OnBossArenaLocked += HandleBossArenaLocked;
            GameEvents.OnPlayerDied += HandlePlayerDied;
        }

        private void OnDisable()
        {
            GameEvents.OnBossArenaLocked -= HandleBossArenaLocked;
            GameEvents.OnPlayerDied -= HandlePlayerDied;
        }

        private void HandleBossArenaLocked()
        {
            spawnerLockedByBoss = true;
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] != null)
                {
                    Destroy(activeEnemies[i]);
                }
            }
            activeEnemies.Clear();
        }

        private void HandlePlayerDied()
        {
            // Al morir y reaparecer en un Checkpoint, limpiar enemigos lejanos y reactivar puntos por delante del jugador
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] != null)
                {
                    Destroy(activeEnemies[i]);
                }
            }
            activeEnemies.Clear();

            if (playerTransform != null)
            {
                for (int i = 0; i < mapEncounters.Count; i++)
                {
                    MapEncounterPoint pt = mapEncounters[i];
                    if (pt.position.x > playerTransform.position.x + 12f)
                    {
                        pt.triggered = false;
                        mapEncounters[i] = pt;
                    }
                }
            }
        }

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }

            BuildMapProximityEncounters();
            ScheduleNextSpawn();
        }

        private void BuildMapProximityEncounters()
        {
            mapEncounters.Clear();
            // En el Nivel 1 (bossZoneStartX > 500), distribuir puntos de encuentro a lo largo de los 6 módulos
            if (bossZoneStartX < 300f) return;

            for (int m = 0; m < 6; m++)
            {
                float bX = m * 210f;
                // Puntos en Planta 1 y plataformas del laberinto de cada módulo (nunca más allá de X = 1130m)
                Vector2[] modulePoints = new Vector2[]
                {
                    new Vector2(bX + 34f, -0.8f),
                    new Vector2(bX + 62f, 2.75f),
                    new Vector2(bX + 94f, -0.8f),
                    new Vector2(bX + 126f, 2.75f),
                    new Vector2(bX + 168f, -0.8f)
                };

                for (int i = 0; i < modulePoints.Length; i++)
                {
                    if (modulePoints[i].x < bossZoneStartX - 10f)
                    {
                        mapEncounters.Add(new MapEncounterPoint
                        {
                            position = modulePoints[i],
                            triggered = false
                        });
                    }
                }
            }
        }

        private void Update()
        {
            if (spawnerLockedByBoss || playerTransform == null || smallEnemyPrefab == null) return;
            if (Time.timeScale <= 0.01f) return;

            activeEnemies.RemoveAll(e => e == null);

            // Despachar enemigos que hayan quedado muy atrás del jugador (> 38m) para liberar cupo a los que están adelante
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] != null && Vector2.Distance(activeEnemies[i].transform.position, playerTransform.position) > 38f)
                {
                    Destroy(activeEnemies[i]);
                    activeEnemies.RemoveAt(i);
                }
            }

            // 1. Activar enemigos situados a lo largo del mapa conforme el jugador se acerca a su posición
            float playerX = playerTransform.position.x;
            float playerY = playerTransform.position.y;
            for (int i = 0; i < mapEncounters.Count; i++)
            {
                MapEncounterPoint pt = mapEncounters[i];
                if (pt.triggered) continue;

                float distX = pt.position.x - playerX;
                if (distX >= -4.0f && distX <= proximityActivationDistance && Mathf.Abs(pt.position.y - playerY) <= 12.0f)
                {
                    pt.triggered = true;
                    mapEncounters[i] = pt;
                    if (activeEnemies.Count < maxActiveEnemies + 1)
                    {
                        SpawnEnemyOnGroundAt(pt.position);
                    }
                }
            }

            // 2. Refuerzo terrestre moderado al frente del jugador (nunca desde el cielo y nunca cerca del Checkpoint del Jefe)
            if (Time.time >= nextSpawnTime && activeEnemies.Count < maxActiveEnemies && playerX < bossZoneStartX - 18f)
            {
                SpawnEnemy();
                ScheduleNextSpawn();
            }
        }

        private void SpawnEnemy()
        {
            if (spawnerLockedByBoss || playerTransform.position.x > bossZoneStartX - 15f) return;

            bool spawnRight = Random.value > 0.20f;
            float distanceX = Random.Range(10.5f, 15.0f);
            float candidateX = Mathf.Clamp(playerTransform.position.x + (spawnRight ? distanceX : -distanceX), -16f, bossZoneStartX - 15f);
            float candidateY = Mathf.Clamp(playerTransform.position.y + 0.8f, -0.8f, 58.0f);

            SpawnEnemyOnGroundAt(new Vector2(candidateX, candidateY));
        }

        private void SpawnEnemyOnGroundAt(Vector2 approxPos)
        {
            // Ajustar siempre al ras del suelo o plataforma mediante Raycast hacia abajo (nunca cayendo del cielo)
            Vector2 rayStart = new Vector2(approxPos.x, approxPos.y + 2.2f);
            RaycastHit2D[] hits = Physics2D.RaycastAll(rayStart, Vector2.down, 8.0f);
            float groundY = approxPos.y;
            bool foundGround = false;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D c = hits[i].collider;
                if (c == null || c.isTrigger) continue;
                if (c.GetComponentInParent<PlayerMovement>() != null || c.GetComponentInParent<IDamageable>() != null) continue;
                groundY = hits[i].point.y + 0.72f;
                foundGround = true;
                break;
            }

            if (!foundGround)
            {
                groundY = Mathf.Max(-1.02f, approxPos.y);
            }

            Vector3 spawnPos = new Vector3(approxPos.x, groundY, 0f);
            GameObject enemy = Instantiate(smallEnemyPrefab, spawnPos, Quaternion.identity);
            SmallEnemyAI spawnedAi = enemy.GetComponent<SmallEnemyAI>();
            if (spawnedAi != null) spawnedAi.SetDifficultyMultiplier(difficultyMultiplier * GameSettings.EnemyHealthDifficultyMultiplier);
            activeEnemies.Add(enemy);
            StartCoroutine(MaterializeEnemySmoothly(enemy));
        }

        private IEnumerator MaterializeEnemySmoothly(GameObject enemy)
        {
            if (enemy == null) yield break;

            SmallEnemyAI ai = enemy.GetComponent<SmallEnemyAI>();
            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();

            if (ai != null) ai.enabled = false;
            float savedGrav = rb != null ? rb.gravityScale : 2.5f;
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }

            enemy.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            float elapsed = 0f;
            const float duration = 0.45f;

            while (elapsed < duration && enemy != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                enemy.transform.localScale = new Vector3(t, t, 1f);
                if (sr != null)
                {
                    sr.color = Color.Lerp(new Color(0.2f, 0.95f, 1.0f, 0.45f), Color.white, t);
                }
                yield return null;
            }

            if (enemy != null)
            {
                enemy.transform.localScale = Vector3.one;
                if (sr != null) sr.color = Color.white;
                if (rb != null) rb.gravityScale = savedGrav;
                if (ai != null) ai.enabled = true;
            }
        }

        private void ScheduleNextSpawn()
        {
            nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval) * GameSettings.EnemySpawnIntervalMultiplier;
        }

        public void SetEnemyPrefab(GameObject prefab)
        {
            smallEnemyPrefab = prefab;
        }
    }
}
