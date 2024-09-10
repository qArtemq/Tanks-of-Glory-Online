using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    [Header("Main Setting")]
    public Transform tower;
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
    private bool isDestroyed = false;

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
        if (Input.GetKeyDown(KeyCode.F) && !isDestroyed) // ��� ����� ��� ������� ������� Space
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

        gameManager.DestroyTank(gameObject);
    }

    void DetachAndLaunchTower()
    {
        // ���������, ���� �� � ����� ��������� Rigidbody, ���� ���, ��������� ���
        Rigidbody turretRb = tower.gameObject.GetComponent<Rigidbody>();
        if (turretRb == null)
        {
            turretRb = tower.gameObject.AddComponent<Rigidbody>();
        }
        tower.transform.parent = null; // ����������� ����� �� �������

        // ����� ������� ��� ����� �����
        turretRb.AddForce(Vector3.up * 100f + Random.insideUnitSphere * 50f); // ����� � ��������� �����������
        turretRb.AddTorque(Random.insideUnitSphere * 100f); // ��������� �������� ��� ��������

        // ������� ����� ����� ��������� �����
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
            wheel.parent = null; // ����������� ������
            wheelRb.AddForce(Random.insideUnitSphere * 150f);
            wheelRb.AddTorque(Random.insideUnitSphere * 50f);
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
            wheelRb.AddForce(Random.insideUnitSphere * 100f);
            wheelRb.AddTorque(Random.insideUnitSphere * 50f);
            Destroy(wheel.gameObject, 10f);
        }
    }
    IEnumerator RespawnTank()
    {
        yield return new WaitForSeconds(15f);

        // ������ ����� ���� �� ����� spawnPoint
        Instantiate(tankPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
