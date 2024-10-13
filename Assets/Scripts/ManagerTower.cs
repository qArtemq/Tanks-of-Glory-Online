using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ManagerTower : MonoBehaviourPunCallbacks
{
    [Header("Main Settings")]
    public Transform muzzle;
    public Transform tower;

    [Header("UI Settings")]
    public RectTransform crosshairUI; // Ссылка на UI-элемент прицела
    public Camera tankCamera;  // Камера, прикрепленная к танку

    [Header("Tower Setting")]
    public float speedTower = 2;

    [Header("Recoil Tower")]
    public float recoilForce = 10f;
    public float recoilDistance = 0.5f;
    public float recoilSpeed = 5f;
    private Vector3 defaultPosition;

    [Header("Audio Settings")]
    public AudioClip reloadSound;
    public AudioClip shotSound;
    public AudioClip towerSound;
    private AudioSource attackAudioSource;
    private AudioSource towerAudioSource;

    [Header("Effect Settings")]
    public float range = 100f;
    public ParticleSystem fireEffect;
    public ParticleSystem smokeEffect;
    public GameObject damageEffect;
    public GameObject damageEffectGround;

    [Header("Tank States")]
    public bool canShoot = true;
    public bool isTowerMoving = false;
    Rigidbody rb;
    Player player;
    [Header("Attack Settings")]
    public float damageAmount = 0.33f;  // Количество урона за выстрел
    ScoreManager scoreManager;

    Vector3 targetPosition;
    Quaternion targetRotation;
    void Start()
    {
        // Если это не мой танк, скрываем прицел
        if (!photonView.IsMine)
        {
            if (crosshairUI != null)
            {
                crosshairUI.gameObject.SetActive(false);  // Отключаем прицел
            }
            return;  // Выходим из метода, чтобы не инициализировать камеру и прицел
        }
        defaultPosition = muzzle.transform.localPosition;
        player = GetComponent<Player>();
        towerAudioSource = player.AudioSources[2];
        attackAudioSource = player.AudioSources[3];
        rb = GetComponent<Rigidbody>();
        scoreManager = GetComponent<ScoreManager>();
        // Привязываем дуло к башне, чтобы оно всегда двигалось вместе с башней
        muzzle.SetParent(tower);
    }

    void Update()
    {
        // Проверяем, уничтожен ли танк
        if (photonView.IsMine && !player.isDestroyed)
        {
            if (muzzle != null)
            {
                MoveTower();
                Attack();
                UpdateCrosshairPosition();  // Обновляем позицию прицела
            }
        }
    }
    void UpdateCrosshairPosition()
    {
        RaycastHit hit;
        Vector3 hitPoint;

        // Стреляем из дула танка, чтобы определить, куда он смотрит
        if (Physics.Raycast(muzzle.position, muzzle.forward, out hit, range))
        {
            hitPoint = hit.point;  // Точка попадания
        }
        else
        {
            // Если луч не попадает ни во что, используем точку на максимальной дистанции
            hitPoint = muzzle.position + muzzle.forward * range;
        }

        // Преобразуем точку в мировых координатах в экранные координаты для отображения прицела
        Vector3 screenPosition = tankCamera.WorldToScreenPoint(hitPoint);

        crosshairUI.position = screenPosition;

    }


    void Attack()
    {
        if (player.isInvisible || player.isDestroyed) return;

        if (Input.GetKeyDown(KeyCode.Space) && canShoot)
        {
            photonView.RPC("Shoot", RpcTarget.All);  // Вызываем метод Shoot для всех игроков

            photonView.RPC("PlayShotSound", RpcTarget.All);

            StartCoroutine(WaitAttack());

            photonView.RPC("RecoilShoot", RpcTarget.All);

            CameraShake.Instance.ShakeCamera(5f, 1f);

            Vector3 recoilForce = -transform.forward * recoilSpeed;  // Увеличиваем силу отдачи
            rb.AddForce(recoilForce, ForceMode.VelocityChange);
        }
    }
    [PunRPC]
    void PlayShotSound()
    {
        attackAudioSource.volume = 3f;
        attackAudioSource.PlayOneShot(shotSound);
    }
    [PunRPC]
    void RecoilShoot()
    {
        Vector3 recoilPosition = defaultPosition - new Vector3(0, 0, recoilDistance);

        muzzle.DOLocalMove(recoilPosition, 0.2f).OnComplete(() =>
        {
            muzzle.DOLocalMove(defaultPosition, 0.2f);
        });
    }
    [PunRPC]
    void Shoot()
    {
        PlayFire();
        ProcessRaycast();
    }
    void PlayFire()
    {
        fireEffect.Play();
        smokeEffect.Play();
        StartCoroutine(Smoke());
    }
    public IEnumerator Smoke()
    {
        yield return new WaitForSeconds(2f);

        smokeEffect.Stop();

    }
    private void ProcessRaycast()
    {
        // Получаем команду атакующего игрока
        string attackerTeam = TeamManager.selectedTeam;
        RaycastHit hit;

        // Используем направление дула для вычисления выстрела
        if (Physics.Raycast(muzzle.position, muzzle.forward, out hit, range))
        {
            CreateHitImpuct(hit);
            Health target = hit.transform.GetComponent<Health>();
            if (target != null)
            {
                target.TakeDamage(damageAmount);  // Передаем значение урона

                if (target.health <= 0)
                {
                    photonView.RPC("UpdateScore", RpcTarget.All, attackerTeam);
                }
            }
        }
    }
    private void CreateHitImpuct(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Ground"))
        {
            GameObject impact = Instantiate(damageEffectGround, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impact, 3f);
        }
        else
        {
            GameObject impact = Instantiate(damageEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impact, 3f);
        }
    }

    public IEnumerator WaitAttack()
    {
        canShoot = false;

        attackAudioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(3f);

        canShoot = true;
    }

    void MoveTower()
    {
        if (player.isDestroyed) return;
        bool isMovingNow = false;

        towerAudioSource.volume = 20f;
        // Управление вращением башни
        if (Input.GetKey(KeyCode.K))
        {
            tower.Rotate(Vector3.up, -speedTower * Time.deltaTime);
            isMovingNow = true;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            tower.Rotate(Vector3.up, speedTower * Time.deltaTime);
            isMovingNow = true;
        }

        // Синхронизация вращения башни через RPC
        if (isMovingNow && !isTowerMoving)
        {
            photonView.RPC("StartMovingTower", RpcTarget.All);
        }
        else if (!isMovingNow && isTowerMoving)
        {
            photonView.RPC("StopMovingTower", RpcTarget.All);
        }

        if (isMovingNow)
        {
            // Отправляем поворот башни всем другим игрокам
            photonView.RPC("SyncTowerRotation", RpcTarget.Others, tower.rotation);
        }

        isTowerMoving = isMovingNow;
    }
    [PunRPC]
    void SyncTowerRotation(Quaternion newRotation)
    {
        // Обновляем поворот башни на других клиентах
        tower.rotation = newRotation;
    }
    [PunRPC]
    void StartMovingTower()
    {
        towerAudioSource.clip = towerSound;
        towerAudioSource.loop = true;
        towerAudioSource.Play();
        isTowerMoving = true;
    }

    [PunRPC]
    void StopMovingTower()
    {
        towerAudioSource.Stop();
        isTowerMoving = false;
    }
}