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
        Rigidbody turretRb = tower.gameObject.GetComponent<Rigidbody>();
        Rigidbody bodyRb = body.gameObject.GetComponent<Rigidbody>();
        if (turretRb == null)
        {
            turretRb = tower.gameObject.AddComponent<Rigidbody>();
        }
        if (bodyRb == null)
        {
            bodyRb = body.gameObject.AddComponent<Rigidbody>();
        }

        // Настройка массы для башни и корпуса
        turretRb.mass = 1000f; // Большая масса для тяжести
        bodyRb.mass = 2000f;   // Ещё больше масса для корпуса

        tower.transform.parent = null;

        // Убираем силу подлёта (значения минимальны)
        turretRb.AddForce(Vector3.up * 2f + Random.insideUnitSphere * 1f);
        turretRb.AddTorque(Random.insideUnitSphere * 1f);

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

            WheelCollider wheelCollider = wheel.GetComponentInParent<WheelCollider>();
            if (wheelCollider != null)
            {
                Destroy(wheelCollider);
            }

            if (wheel.GetComponent<Collider>() == null)
            {
                wheel.gameObject.AddComponent<BoxCollider>();
            }

            wheel.position += Vector3.up * 0.5f;

            wheelRb.mass = 500f; // Большая масса для колес

            wheel.parent = null;
            wheelRb.AddForce(Random.insideUnitSphere * 1f); // Очень слабый импульс
            wheelRb.AddTorque(Random.insideUnitSphere * 1f); // Очень слабое вращение

            Destroy(wheel.gameObject, 10f);
        }

        foreach (Transform wheel in leftWheels)
        {
            Rigidbody wheelRb = wheel.gameObject.GetComponent<Rigidbody>();
            if (wheelRb == null)
            {
                wheelRb = wheel.gameObject.AddComponent<Rigidbody>();
            }

            WheelCollider wheelCollider = wheel.GetComponentInParent<WheelCollider>();
            if (wheelCollider != null)
            {
                Destroy(wheelCollider);
            }

            if (wheel.GetComponent<Collider>() == null)
            {
                wheel.gameObject.AddComponent<BoxCollider>();
            }

            wheel.position += Vector3.up * 0.5f;

            wheelRb.mass = 500f; // Большая масса

            wheel.parent = null;
            wheelRb.AddForce(Random.insideUnitSphere * 1f);
            wheelRb.AddTorque(Random.insideUnitSphere * 1f);

            Destroy(wheel.gameObject, 10f);
        }
    }

}
