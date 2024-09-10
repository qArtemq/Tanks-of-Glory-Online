using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    [Header("Main Setting")]
    public Transform tower;
    public Transform body;
    public Transform[] rightWheels;
    public Transform[] leftWheels;

    [Header("Prefab Settings")]
    public GameObject tankPrefab;
    public Transform spawnPoint;

    [Header("Audio")]
    public AudioClip killSound;
    private AudioSource killAudioSource;

    [Header("Effect")]
    public GameObject explosionEffect;
    public GameObject explosionSmokeEffect;

    Player player;
    public bool isDestroyed = false;

    private GameManager gameManager;


    void Start()
    {
        player = GetComponent<Player>();
        player.AudioSources = GetComponents<AudioSource>();
        killAudioSource = player.AudioSources[4];

        gameManager = FindObjectOfType<GameManager>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isDestroyed) // Для теста при нажатии клавиши Space
        {
            DestroyTank();
        }
    }

    void DestroyTank()
    {
        isDestroyed = true;
        player.isDestroyed = true;

        killAudioSource.clip = killSound;
        killAudioSource.volume = 1f;
        killAudioSource.Play();

        GameObject boom = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Destroy(boom, 2f);

        GameObject smoke = Instantiate(explosionSmokeEffect, transform.position, Quaternion.identity);
        Destroy(smoke, 10f);

        DetachAndLaunchTower();
        DetachAndLaunchWheels();

        gameManager.TankToSpawn(body);

        Destroy(gameObject, 10f);
    }

    void DetachAndLaunchTower()
    {
        // Проверяем, есть ли у башни компонент Rigidbody, если нет, добавляем его
        Rigidbody turretRb = tower.gameObject.GetComponent<Rigidbody>();
        Rigidbody bodyRb = body.gameObject.GetComponent<Rigidbody>();
        if (turretRb == null)
        {
            turretRb = tower.gameObject.AddComponent<Rigidbody>();
        }
        tower.transform.parent = null; // Отсоединяем башню от корпуса
        if (bodyRb == null)
        {
            bodyRb = body.gameObject.AddComponent<Rigidbody>();
        }

        // Задаём импульс для взлёта башни
        turretRb.AddForce(Vector3.up * 100f + Random.insideUnitSphere * 50f); // Вверх и случайное направление
        turretRb.AddTorque(Random.insideUnitSphere * 100f); // Добавляем кручение для реализма
        Destroy(body.gameObject, 10f);

        Destroy(tower.gameObject, 10f);
    }

    void DetachAndLaunchWheels()
    {
        foreach (Transform wheel in rightWheels)
        {
            Rigidbody wheelRb = wheel.gameObject.GetComponent<Rigidbody>();
            if (wheelRb == null)
            {
                wheelRb = wheel.gameObject.AddComponent<Rigidbody>();
            }
            wheel.parent = null; // Отсоединяем колесо
            wheelRb.AddForce(Random.insideUnitSphere * 50f);
            wheelRb.AddTorque(Random.insideUnitSphere * 20f);
            Destroy(wheel.gameObject, 10f);
        }

        foreach (Transform wheel in leftWheels)
        {
            Rigidbody wheelRb = wheel.gameObject.GetComponent<Rigidbody>();
            if (wheelRb == null)
            {
                wheelRb = wheel.gameObject.AddComponent<Rigidbody>();
            }
            wheel.parent = null;
            wheelRb.AddForce(Random.insideUnitSphere * 10f);
            wheelRb.AddTorque(Random.insideUnitSphere * 5f);
            Destroy(wheel.gameObject, 10f);
        }
    }
}
