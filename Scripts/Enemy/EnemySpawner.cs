using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;


public class EnemySpawner : MonoBehaviour
{
    public int initialEnemiesPerWave;
    public int currentEnemiesPerWave;

    private float spawnDelay = 1f;
    private bool hasInstantiated;

    public int currentWave= 0;
    public float waveCoolDown;

    public bool inCoolDown;
    public float coolDownCounter;

    public List<Enemy> currentEnemiesAlive;
    List<Enemy> enemiesToRemove = new List<Enemy>();
    public GameObject[] spawnPoints;
    public GameObject[] weaponPrefab;
    public GameObject healingBottles;
    public GameObject weaponSpawnPoint;
    public GameObject healingBottleSpawnPoint;
    public GameObject[]enemyPrefab;
    public GameObject spawnMenu;
    public TMP_Text wave;
    public TMP_Text enemy;
    public TMP_Text time;


    private PlayerController player;
    void Start()
    {
        currentEnemiesPerWave = initialEnemiesPerWave;

        spawnMenu.SetActive(false);

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        StartNextWave();
    }

    void Update()
    {
        foreach (Enemy enemy in currentEnemiesAlive)
        {
            if (enemy.isDead)
            {
                enemiesToRemove.Add(enemy); 
            }
        }

        foreach (Enemy enemy in enemiesToRemove)
        {
            currentEnemiesAlive.Remove(enemy);

            enemy.slider.gameObject.SetActive(false);
            
            StartCoroutine(DestroyAfterDelay(enemy, 6f));
        }

        enemiesToRemove.Clear();

        if (currentEnemiesAlive.Count() == 0 && inCoolDown == false)
        {
            StartCoroutine(WaveCoolDown());
        }

        if (inCoolDown)
        {
            coolDownCounter -= Time.deltaTime;
        }
        else
        {
            coolDownCounter = waveCoolDown;
        }

        if (player.playerIsDead) spawnMenu.SetActive(false);

        UpdateUI();

        time.enabled = inCoolDown;
    }

    public void StartNextWave()
    {
        currentEnemiesAlive.Clear();

        currentWave++;

        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < currentEnemiesPerWave; i++)
        {
            Vector3 spawnOffset = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));

            int index = Random.Range(0, spawnPoints.Length - 2);

            Vector3 spawnPosition = spawnPoints[index].transform.position + spawnOffset;

            var Enemy = Instantiate(enemyPrefab[0], spawnPosition, Quaternion.identity);

            Enemy enemyScript = Enemy.GetComponent<Enemy>();

            currentEnemiesAlive.Add(enemyScript);

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private IEnumerator WaveCoolDown()
    {
        inCoolDown = true;

        SpawnHealingBottles();

        SpawnWeapons();

        yield return new WaitForSeconds(waveCoolDown);

        inCoolDown = false;

        currentEnemiesPerWave += 2;

        StartNextWave();
    }

    private IEnumerator DestroyAfterDelay(Enemy enemy, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(enemy.gameObject);
    }

    public void InstantiateWitch()
    {
        for (int i = 0; i < 2; i++)
        {
            int index = Random.Range(9, 11);
            Instantiate(enemyPrefab[1], spawnPoints[index].transform.position + new Vector3(Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f)), Quaternion.identity);
        }
    }

    public void SpawnHealingBottles()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 spawnOffset = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
            Instantiate(healingBottles, healingBottleSpawnPoint.transform.position + spawnOffset, Quaternion.identity);
        }
    }

    public void SpawnWeapons()
    {
        if (currentWave <= 5)
        {
            Instantiate(weaponPrefab[currentWave -1], weaponSpawnPoint.transform.position + new Vector3(Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f)), Quaternion.identity);
        }
        else
        {
            foreach(GameObject w in weaponPrefab)
            {
                Instantiate(w, weaponSpawnPoint.transform.position + new Vector3(Random.Range(-20f, 20f), 0, Random.Range(-20f,20f)), Quaternion.identity);
            }
        }
    }

    public void UpdateUI()
    {
        wave.text = "Wave: " + currentWave;

        enemy.text = "Enemy: " + currentEnemiesAlive.Count;

        time.text = ((int)coolDownCounter).ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            spawnMenu.SetActive(true); 
        }
    }
}
