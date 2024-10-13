using System.Collections;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using Photon.Pun;

public class Player : MonoBehaviourPunCallbacks
{

    [Header("Wheels Setting")]
    public Transform[] rightWheels;
    public Transform[] leftWheels;
    public float moveSpeed = 5f;     // Скорость движения танка
    public float rotationSpeed = 120f;    // Скорость поворота танка
    public float speedWheels = 200f;

    [Header("Tilt Simulation")]
    public Transform body;
    public float tiltAngle = 5f;
    public float tiltSpeed = 2f;
    private float targetTiltAngle = 0.0f;

    [Header("Audio Setting")]
    public AudioClip waitSound;
    public AudioClip moveSound;
    private AudioSource waitAudioSource;
    private AudioSource moveAudioSource;
    private AudioSource[] audioSources;
    private bool isMoveSound = false;
    private bool wasMovingSound = false;
    private Rigidbody rb;
    public AudioSource[] AudioSources { get { return audioSources; } set { audioSources = value; } }

    [Header("Effect Setting")]
    public ParticleSystem[] waitEffect;
    public ParticleSystem[] goEffect;
    public ParticleSystem[] boostEffect;
    public ParticleSystem[] graundEffect;

    [Header("Boost Setting")]
    public float boostSpeed = 20f; // Скорость во время буста
    public float boostDuration = 3f; // Длительность буста
    public bool isBoosting = false; // Активирован ли буст
    public float boostCooldown = 5f; // Время до следующего буста
    public float boostCooldownTimer = 0f; // Таймер для буста

    [Header("Tank States")]
    public bool isFlipped = false;  // Новый флаг для отслеживания переворота танка
    public bool isDestroyed = false;
    public bool isInvisible = false;

    ManagerTower managerTower;

    void Start()
    {
        managerTower = GetComponent<ManagerTower>();
        rb = GetComponent<Rigidbody>();
        audioSources = GetComponents<AudioSource>();
        waitAudioSource = audioSources[0];
        moveAudioSource = audioSources[1];

        waitAudioSource.clip = waitSound;
        waitAudioSource.loop = true;
        waitAudioSource.volume = 0.2f;
        waitAudioSource.Play();

        moveAudioSource.clip = moveSound;
        moveAudioSource.loop = true;
        moveAudioSource.volume = 0f;
        moveAudioSource.Play();
        rb.constraints = RigidbodyConstraints.None;  // Убираем ограничения вращения
        rb.centerOfMass = new Vector3(0, -1, 0);
        rb.mass = 5000f; // Увеличьте массу танка для устойчивости.
        Physics.gravity = new Vector3(0, -30f, 0); // Снизьте влияние гравитации.

    }
    void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                BoostTank(); // Активируем буст
            }
            // Обновляем таймер перезарядки буста
            if (boostCooldownTimer > 0f)
            {
                boostCooldownTimer -= Time.deltaTime;
            }
            CheckIfFlipped();  // Проверка, перевернулся ли танк
            if (!isFlipped)
            {
                Move();
            }
        }
    }
    private void FixedUpdate()
    {
        StabilizeTank();
    }
    private void CheckIfFlipped()
    {
        // Проверяем угол наклона по оси X или Z, чтобы понять, перевернулся ли танк
        float angle = Vector3.Angle(Vector3.up, transform.up);
        if (angle > 45)  // Если наклон больше 45 градусов, считаем, что танк перевернулся
        {
            isFlipped = true;
        }
        else
        {
            isFlipped = false;
        }
    }

    private void StabilizeTank()
    {
        // Стабилизация танка при наклонах
        Vector3 predictedUp = Quaternion.AngleAxis(
            rb.angularVelocity.magnitude * Mathf.Rad2Deg * 0.5f / tiltSpeed,
            rb.angularVelocity) * transform.up;
        Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
        rb.AddTorque(torqueVector * tiltSpeed * tiltSpeed);
    }
    void BoostTank()
    {
        // Проверяем, доступен ли буст
        if (isBoosting || boostCooldownTimer > 0f)
        {
            return; // Если танк уже в состоянии буста или буст на перезарядке
        }

        // Увеличиваем скорость танка на время действия буста
        StartCoroutine(BoostCoroutine());
    }

    IEnumerator BoostCoroutine()
    {
        isBoosting = true;
        float originalSpeed = moveSpeed; // Сохраняем обычную скорость
        moveSpeed = boostSpeed; // Устанавливаем скорость буста

        // Устанавливаем таймер на полную длительность буста
        boostCooldownTimer = boostDuration;

        // Останавливаем обычные эффекты и включаем буст
        StopWaitEffect();
        StopGoEffect();
        PlayGroundEffect();
        // Вызываем буст для всех клиентов
        photonView.RPC("ActivateBoostEffect", RpcTarget.All);

        // Ждем окончания действия буста
        yield return new WaitForSeconds(boostDuration);

        // Возвращаем скорость обратно
        moveSpeed = originalSpeed;
        isBoosting = false;

        // Включаем перезарядку буста
        boostCooldownTimer = boostCooldown;

        // Останавливаем эффекты буста
        photonView.RPC("StopBoostEffect", RpcTarget.All);
        // Проверяем, если танк продолжает двигаться после окончания буста
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f)
        {
            PlayGoEffect(); // Включаем эффект движения, если танк продолжает движение
            PlayGroundEffect();
        }
        else
        {
            PlayWaitEffect(); // Включаем эффект ожидания, если танк остановился
        }
    }
    // Используем RPC для вызова буста у всех игроков
    [PunRPC]
    void ActivateBoostEffect()
    {
        // Включаем эффекты буста у всех клиентов
        PlayBoostEffect();
    }
    void PlayBoostEffect()
    {
        // Добавьте здесь код для визуальных эффектов буста, например огонь из выхлопной трубы
        foreach (var effect in boostEffect)
        {
            if (!effect.isPlaying)
            {
                effect.Play();
            }
        }
    }
    [PunRPC]
    void StopBoostEffect()
    {
        // Останавливаем эффекты буста
        foreach (var effect in boostEffect)
        {
            if (effect.isPlaying)
            {
                effect.Stop();
            }
        }
    }
    void Move()
    {
        if (isFlipped) return;
        if (isDestroyed)
        {
            // Останавливаем звук движения и ожидания
            moveAudioSource.volume = 0f;
            waitAudioSource.volume = 0f;
            StopAllEffects();
            return; // Прекращаем выполнение метода Move
        }
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        if (photonView.IsMine)  // Только свой танк должен воспроизводить звуки движения
        {
            // Двигаем танк вперед/назад с постоянной скоростью
            transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);

            // Вращаем танк вокруг вертикальной оси Y (поворот вокруг своей оси)
            transform.Rotate(Vector3.up, turnInput * rotationSpeed * Time.deltaTime);

            isMoveSound = Mathf.Abs(moveInput) > 0.01f || Mathf.Abs(turnInput) > 0.01f;

            if (isMoveSound && !wasMovingSound)
            {
                moveAudioSource.DOFade(0.2f, 0.2f);  // Увеличиваем громкость звука движения
                waitAudioSource.DOFade(0f, 0.2f);    // Уменьшаем громкость звука ожидания

                if (!isBoosting)
                {
                    PlayGoEffect();
                    PlayGroundEffect();
                    StopWaitEffect();
                }
            }
            else if (!isMoveSound && wasMovingSound)
            {
                moveAudioSource.DOFade(0f, 0.2f);    // Уменьшаем громкость звука движения
                waitAudioSource.DOFade(0.2f, 0.2f);  // Увеличиваем громкость звука ожидания

                if (!isBoosting)
                {
                    PlayWaitEffect();
                    StopGoEffect();
                    StopGroundEffect();
                }
            }

            wasMovingSound = isMoveSound;
        }
        // Определяем целевой угол в зависимости от движения вперед или назад
        if (moveInput > 0)
        {
            targetTiltAngle = -5.0f; // Увеличиваем угол наклона вперед
        }
        else if (moveInput < 0)
        {
            targetTiltAngle = 5.0f; // Уменьшаем угол наклона назад
        }
        else
        {
            targetTiltAngle = 0.0f; // Возвращаемся в исходное положение
        }

        photonView.RPC("SyncTiltAngle", RpcTarget.All, targetTiltAngle);

        WheelRotation(moveInput, turnInput);

        // Синхронизируем вращение колес через RPC
        photonView.RPC("SyncWheelRotation", RpcTarget.Others, moveInput, turnInput);
    }
    [PunRPC]
    void SyncTiltAngle(float targetTiltAngle)
    {
        // Плавно изменяем текущий угол к целевому
        tiltAngle = Mathf.Lerp(tiltAngle, targetTiltAngle, Time.deltaTime * tiltSpeed);

        // Применяем текущий угол ко всем частям тела

        // Поворот по оси X для наклона
        body.transform.localRotation = Quaternion.Lerp(
            body.transform.localRotation,
            Quaternion.Euler(tiltAngle, 0, 0),
            Time.deltaTime * tiltSpeed);

        // Применяем наклон к башне с учётом её текущего поворота
        float towerRotationY = managerTower.tower.localRotation.eulerAngles.y;
        float correctedTiltAngle = tiltAngle;

        // Если башня повёрнута на 90-270 градусов, инвертируем наклон
        if (towerRotationY > 90f && towerRotationY < 270f)
        {
            correctedTiltAngle = -tiltAngle;
        }
        // Применяем наклон к башне
        managerTower.tower.localRotation = Quaternion.Lerp(
        managerTower.tower.localRotation,
        Quaternion.Euler(correctedTiltAngle, towerRotationY, 0),
        Time.deltaTime * tiltSpeed);
    }
    [PunRPC]
    void SyncWheelRotation(float forward_back, float left_right)
    {
        WheelRotation(forward_back, left_right);
    }
    void WheelRotation(float forward_back, float left_right)
    {
        // Вращение колес при движении вперед/назад
        float forwardRotation = forward_back * speedWheels * Time.deltaTime;
        // Вращаем правые и левые колеса при движении вперед/назад
        foreach (Transform wheel in rightWheels)
        {
            wheel.Rotate(Vector3.right, forwardRotation);  // Вращаем колеса по оси X (вперед-назад)
        }
        foreach (Transform wheel in leftWheels)
        {
            wheel.Rotate(Vector3.right, forwardRotation);  // Вращаем колеса по оси X (вперед-назад)
        }

        // Дополнительное вращение колес при повороте (если танк поворачивает)
        if (Mathf.Abs(left_right) > 0.01f)
        {
            float turnRotation = left_right * rotationSpeed * Time.deltaTime;
            foreach (Transform wheel in rightWheels)
            {
                wheel.Rotate(Vector3.right, -turnRotation); // Вращаем правые колеса в противоположную сторону при повороте
            }
            foreach (Transform wheel in leftWheels)
            {
                wheel.Rotate(Vector3.right, turnRotation); // Вращаем левые колеса при повороте
            }
        }
    }
    // Включаем эффект движения
    void PlayGoEffect()
    {
        foreach (var effect in goEffect)
        {
            if (!effect.isPlaying)
            {
                effect.Play();
            }
        }
    }
    // Останавливаем эффект движения
    void StopGoEffect()
    {
        foreach (var effect in goEffect)
        {
            if (effect.isPlaying)
            {
                effect.Stop();
            }
        }
    }
    void PlayGroundEffect()
    {
        foreach (var effect in graundEffect)
        {
            if (!effect.isPlaying)
            {
                effect.Play();
            }
        }
    }

    // Останавливаем эффект движения
    void StopGroundEffect()
    {
        foreach (var effect in graundEffect)
        {
            if (effect.isPlaying)
            {
                effect.Stop();
            }
        }
    }

    // Включаем эффект ожидания
    void PlayWaitEffect()
    {
        foreach (var effect in waitEffect)
        {
            if (!effect.isPlaying)
            {
                effect.Play();
            }
        }
    }

    // Останавливаем эффект ожидания
    void StopWaitEffect()
    {
        foreach (var effect in waitEffect)
        {
            if (effect.isPlaying)
            {
                effect.Stop();
            }
        }
    }

    // Останавливаем все эффекты (если танк разрушен)
    void StopAllEffects()
    {
        StopGroundEffect();
        StopGoEffect();
        StopWaitEffect();
    }
}