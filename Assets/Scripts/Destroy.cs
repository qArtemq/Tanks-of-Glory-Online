using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Destroy : MonoBehaviourPunCallbacks
{
    [Header("Main Setting")]
    public Transform tower;
    public Transform body;
    public Transform[] rightWheels;
    public Transform[] leftWheels;

    [Header("Prefab Settings")]
    public GameObject tankPrefab;

    [Header("Audio")]
    public AudioClip killSound;
    private AudioSource killAudioSource;

    [Header("Effect")]
    public GameObject explosionEffect;
    public GameObject explosionSmokeEffect;

    Player player;
    public bool isDestroyed = false;
    private bool hasExploded = false;  // Флаг для предотвращения повторного взрыва

    ScoreManager scoreManager;

    private GameManager gameManager;

    private float groundHeight = -15f; // Максимальная высота от земли, при которой танк должен быть уничтожен
    private Health health; // Добавляем компонент здоровья

    void Start()
    {
        player = GetComponent<Player>();
        player.AudioSources = GetComponents<AudioSource>();
        killAudioSource = player.AudioSources[4];
        health = GetComponent<Health>(); // Получаем компонент здоровья
        gameManager = FindObjectOfType<GameManager>();
        scoreManager = GetComponent<ScoreManager>();
    }
    void Update()
    {
        if (!photonView.IsMine) return;

        if (transform.position.y < groundHeight || isDestroyed || health.health <= 0)
        {
            DestroyTank();
        }
    }
    void DestroyTank()
    {
        if (isDestroyed || !photonView.IsMine) return;  // Если танк уже уничтожен, не выполняем дальнейшие действия

        isDestroyed = true;
        player.isDestroyed = true;

        // Проверяем флаг перед воспроизведением звука
        if (!hasExploded)
        {
            killAudioSource.clip = killSound;
            killAudioSource.volume = 1f;
            killAudioSource.Play();
        }


        // Вызываем RPC, чтобы синхронизировать уничтожение танка
        photonView.RPC("RPCDestroyTank", RpcTarget.All);

        gameManager.TankToSpawn(body);

        StartCoroutine(DelayedDestroy(10f));
    }

    [PunRPC]
    void RPCDestroyTank()
    {
        // Проверяем флаг, чтобы взрыв произошел только один раз
        if (hasExploded) return;

        hasExploded = true;  // Помечаем, что взрыв уже произошел

        GameObject boom = PhotonNetwork.Instantiate(explosionEffect.name, transform.position, Quaternion.identity);
        StartCoroutine(Boom(boom, 1.5f));

        GameObject smoke = PhotonNetwork.Instantiate(explosionSmokeEffect.name, transform.position, Quaternion.identity);
        StartCoroutine(Smoke(smoke, 9f));

        // Устанавливаем физику для башни и колес
        DetachAndLaunchTower();
        DetachAndLaunchWheels();

        // Отключаем Rigidbody у танка, чтобы предотвратить проваливание
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY;
    }
    IEnumerator Boom(GameObject boom, float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(boom);
    }
    IEnumerator Smoke(GameObject smoke, float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(smoke);
    }

    void DetachAndLaunchTower()
    {
        if (!photonView.IsMine) return;
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
        turretRb.mass = 250f; // Большая масса для тяжести
        bodyRb.mass = 2000f;   // Ещё больше масса для корпуса

        tower.transform.parent = null;

        // Убираем силу подлёта (значения минимальны)
        turretRb.AddForce(Vector3.up * 2f + Random.insideUnitSphere * 1f);
        turretRb.AddTorque(Random.insideUnitSphere * 1f);

        // Если у башни есть PhotonView, удаляем через PhotonNetwork
        if (tower.GetComponent<PhotonView>() != null)
        {
            StartCoroutine(DelayedDestroyBody(10f));  // Удаляем через сеть
        }
        else
        {
            Destroy(tower.gameObject);  // Локальное удаление
        }
    }
    void DetachAndLaunchWheels()
    {
        if (!photonView.IsMine) return;

        foreach (Transform wheel in rightWheels)
        {
            DetachAndLaunchWheel(wheel);
        }

        foreach (Transform wheel in leftWheels)
        {
            DetachAndLaunchWheel(wheel);
        }
    }

    void DetachAndLaunchWheel(Transform wheel)
    {
        Rigidbody wheelRb = wheel.gameObject.GetComponent<Rigidbody>();
        if (wheelRb == null)
        {
            wheelRb = wheel.gameObject.AddComponent<Rigidbody>();
        }

        wheelRb.mass = 500f; // Большая масса для колес

        wheel.position += Vector3.up * 0.5f;

        wheel.parent = null;
        wheelRb.AddForce(Random.insideUnitSphere * 1f); // Очень слабый импульс
        wheelRb.AddTorque(Random.insideUnitSphere * 1f); // Очень слабое вращение

        StartCoroutine(DelayedDestroyWheels(wheel, 10f));
    }
    IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Удаляем танк на всех клиентах
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
    IEnumerator DelayedDestroyWheels(Transform wheel, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Если у объекта нет PhotonView, используем Destroy()
        if (wheel.GetComponent<PhotonView>() != null)
        {
            PhotonNetwork.Destroy(wheel.gameObject);
        }
        else
        {
            Destroy(wheel.gameObject);
        }
    }
    IEnumerator DelayedDestroyBody(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (tower.GetComponent<PhotonView>() != null)
        {
            PhotonNetwork.Destroy(tower.gameObject);
        }
        else
        {
            Destroy(tower.gameObject);
        }

        if (body.GetComponent<PhotonView>() != null)
        {
            PhotonNetwork.Destroy(body.gameObject);
        }
        else
        {
            Destroy(body.gameObject);
        }
    }
}