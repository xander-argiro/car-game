using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TargetSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject package;
    public GameObject destination;
    public WorldSpawner worldSpawner;
    public TargetArrow arrow;

    public int score = 0;
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI gameOverText;

    public AudioSource audioSource;
    public AudioClip packagePickupSound;
    public AudioClip deliveryCompleteSound;

    float minSpawnDist = 10f;
    float collectDistance = 1f;
    private bool carryingPackage = false;

    public float gameTime = 60f; // Time limit in seconds
    private float currentTime;
    public bool gameOver = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        destination.SetActive(false);

        Invoke(nameof(SpawnInitialPackage), 0.1f);

        arrow.SetTarget(package.transform);

        scoreText.text = "Deliveries Completed: 0";
        gameOverText.enabled = false;

        currentTime = gameTime;
    }

    void SpawnInitialPackage()
    {
        SpawnTarget(package, true);
    }

    void SpawnTarget(GameObject targetObject, bool ignoreDistance)
    {
        List<Vector3> candidates = new List<Vector3>();

        foreach (var kvp in worldSpawner.GetActiveChunks())
        {
            GameObject chunkObject = kvp.Value;

            if (chunkObject == null)
            {
                continue;
            }

            if (!ignoreDistance && Vector3.Distance(player.position, chunkObject.transform.position) < minSpawnDist)
            {
                continue;
            }

            Tilemap road = chunkObject.transform.Find("Road")?.GetComponent<Tilemap>();

            if (road == null)
            {
                continue;
            }

            foreach (Vector3Int cellPosition in road.cellBounds.allPositionsWithin)
            {
                if (road.HasTile(cellPosition))
                {
                    Vector3 worldPosition = road.GetCellCenterWorld(cellPosition);
                    candidates.Add(worldPosition);
                }
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("No target positions found");
            return;
        }

        targetObject.transform.position = candidates[Random.Range(0, candidates.Count)];
        targetObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        // Timer logic
        if (gameOver)
        {
            return;
        }

        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            gameOver = true;
            GameOver();
        }

        timerText.text = "Time Remaining: " + Mathf.CeilToInt(currentTime);

        // Package logic
        if (player == null || worldSpawner == null)
        {
            return;
        }

        if (!carryingPackage)
        {
            float distance = Vector3.Distance(player.position, package.transform.position);

            if (distance < collectDistance)
            {
                // Pick up package
                carryingPackage = true;
                package.SetActive(false);

                SpawnTarget(destination, false);

                arrow.SetTarget(destination.transform);

                AudioSource.PlayClipAtPoint(packagePickupSound, transform.position);

                Debug.Log("Package picked up!");
            }
        }
        else
        {
            float distance = Vector3.Distance(player.position, destination.transform.position);

            if (distance < collectDistance)
            {
                // Deliver package
                carryingPackage = false;
                destination.SetActive(false);

                score++;
                scoreText.text = "Deliveries Completed: " + score;

                SpawnTarget(package, false);

                arrow.SetTarget(package.transform);

                AudioSource.PlayClipAtPoint(deliveryCompleteSound, transform.position);

                Debug.Log("Delivery complete!");
            }
        }
    }

    public void GameOver()
    {
        package.SetActive(false);
        destination.SetActive(false);
        arrow.gameObject.SetActive(false);

        gameOverText.enabled = true;
    }
}
