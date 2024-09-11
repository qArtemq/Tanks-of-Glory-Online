using System.Collections;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class Player : MonoBehaviour
{

    [Header("Wheels Setting")]
    public Transform[] rightWheels;
    public Transform[] leftWheels;
    public float moveSpeed = 5f;     // Скорость движения танка
    public float rotationSpeed = 120f;    // Скорость поворота танка
    public float speedWheels = 200f;

    [Header("Tower Setting")]
    public Transform tower;
    public Transform muzzle;
    public float recoilForce = 10f;  // Сила отдачи танка
    public float speedTower = 2;
    public float recoilDistance = 0.5f;
    public float recoilSpeed = 5f;
    private Vector3 defaultPosition;
    private bool canShoot = true;
    private bool isTowerMoving = false;

    [Header("Tilt Simulation")]
    public Transform body;
    public float tiltAngle = 5f;
    public float tiltSpeed = 2f;
    private float targetTiltAngle = 0.0f;

    [Header("Audio Setting")]
    [SerializeField] public AudioClip waitSound;
    public AudioClip moveSound;
    public AudioClip reloadSound;
    public AudioClip shotSound;
    public AudioClip towerSound;

    private Rigidbody rb;

    private AudioSource waitAudioSource;
    private AudioSource moveAudioSource;
    private AudioSource towerAudioSource;
    private AudioSource attackAudioSource;
    private AudioSource[] audioSources;
    private bool isMoveSound = false;
    private bool wasMovingSound = false;

    [Header("Effect Setting")]
    public float range = 100f;
    public ParticleSystem fireEffect;
    public ParticleSystem smokeEffect;
    public GameObject damageEffect;

    public AudioSource[] AudioSources { get { return audioSources; } set { audioSources = value; } }



    [Header("Tank States")]
    public bool isFlipped = false;  // Новый флаг для отслеживания переворота танка
    public bool isDestroyed = false;


    void Start()
    {
        defaultPosition = muzzle.transform.localPosition;

        audioSources = GetComponents<AudioSource>();
        waitAudioSource = audioSources[0];
        moveAudioSource = audioSources[1];
        towerAudioSource = audioSources[2];
        attackAudioSource = audioSources[3];

        waitAudioSource.clip = waitSound;
        waitAudioSource.loop = true;
        waitAudioSource.volume = 0.5f;
        waitAudioSource.Play();

        moveAudioSource.clip = moveSound;
        moveAudioSource.loop = true;
        moveAudioSource.volume = 0f;
        moveAudioSource.Play();

        rb = GetComponent<Rigidbody>();  // Получаем компонент Rigidbody
        rb.constraints = RigidbodyConstraints.None;  // Убираем ограничения вращения
        rb.centerOfMass = new Vector3(0, -1, 0);
        rb.mass = 5000f; // Увеличьте массу танка для устойчивости.
        Physics.gravity = new Vector3(0, -30f, 0); // Снизьте влияние гравитации.

    }
    void Update()
    {
        CheckIfFlipped();  // Проверка, перевернулся ли танк
        if (!isFlipped)
        {
            Move();
        }
        MoveTower();
        RotateWheels();
        Attack();
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
    private void FixedUpdate()
    {
        StabilizeTank();
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

    void Attack()
    {
        if (isDestroyed) return;
        if (Input.GetKeyDown(KeyCode.Space) && canShoot)
        {
            Shoot();
            attackAudioSource.volume = 3f;
            StartCoroutine(WaitAttack());

            attackAudioSource.PlayOneShot(shotSound);

            Vector3 recoilPosition = defaultPosition - new Vector3(0, 0, recoilDistance);

            CameraShake.Instance.ShakeCamera(5f, 1f);

            muzzle.DOLocalMove(recoilPosition, 0.2f).OnComplete(() =>
            {
                muzzle.DOLocalMove(defaultPosition, 0.2f);
            });

            Vector3 recoilForce = -transform.forward * recoilSpeed;  // Увеличиваем силу отдачи
            rb.AddForce(recoilForce, ForceMode.VelocityChange);
        }
    }
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
        RaycastHit hit;
        // Используем направление дула для вычисления выстрела
        if (Physics.Raycast(muzzle.position, muzzle.forward, out hit, range))
        {
            CreateHitImpuct(hit);
            // Здесь можно добавить логику для урона врагам
            return;
        }
    }
    private void CreateHitImpuct(RaycastHit hit)
    {
        GameObject impact = Instantiate(damageEffect, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(impact, 3f);
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
        if (isDestroyed) return;
        bool isMovingNow = false;

        towerAudioSource.volume = 20f;
        // Управление вращением башни
        if (Input.GetKey(KeyCode.K))
        {
            // Поворот по оси Y на основе времени
            tower.Rotate(Vector3.up, -speedTower * Time.deltaTime);
            isMovingNow = true;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            // Поворот в противоположную сторону по оси Y на основе времени
            tower.Rotate(Vector3.up, speedTower * Time.deltaTime);
            isMovingNow = true;
        }

        // Логика воспроизведения звуков для башни
        if (isMovingNow && !isTowerMoving)
        {
            // Начало движения башни
            towerAudioSource.clip = towerSound;
            towerAudioSource.loop = true;
            towerAudioSource.Play();
        }
        else if (!isMovingNow && isTowerMoving)
        {
            // Остановка звука после прекращения движения башни
            towerAudioSource.Stop();
        }

        isTowerMoving = isMovingNow;
    }
    void Move()
    {
        if (isFlipped) return;
        if (isDestroyed)
        {
            // Останавливаем звук движения и ожидания
            moveAudioSource.volume = 0f;
            waitAudioSource.volume = 0f;
            return; // Прекращаем выполнение метода Move
        }


        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");


        // Двигаем танк вперед/назад с постоянной скоростью
        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);

        // Вращаем танк вокруг вертикальной оси Y (поворот вокруг своей оси)
        transform.Rotate(Vector3.up, turnInput * rotationSpeed * Time.deltaTime);



        isMoveSound = Mathf.Abs(moveInput) > 0.01f || Mathf.Abs(turnInput) > 0.01f;

        if (isMoveSound && !wasMovingSound)
        {
            // Плавное увеличение звука движения и уменьшение звука ожидания
            moveAudioSource.DOFade(0.5f, 1f); // Увеличиваем громкость звука движения
            waitAudioSource.DOFade(0f, 1f); // Уменьшаем громкость звука ожидания
        }
        else if (!isMoveSound && wasMovingSound)
        {
            // Плавное уменьшение звука движения и увеличение звука ожидания
            moveAudioSource.DOFade(0f, 1f); // Уменьшаем громкость звука движения
            waitAudioSource.DOFade(0.5f, 1f); // Увеличиваем громкость звука ожидания
        }

        wasMovingSound = isMoveSound;

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


        // Плавно изменяем текущий угол к целевому
        tiltAngle = Mathf.Lerp(tiltAngle, targetTiltAngle, Time.deltaTime * tiltSpeed);

        // Применяем текущий угол ко всем частям тела

        // Поворот по оси X для наклона
        body.transform.localRotation = Quaternion.Lerp(
            body.transform.localRotation,
            Quaternion.Euler(tiltAngle, 0, 0),
            Time.deltaTime * tiltSpeed);

        // Применяем наклон к башне
        tower.localRotation = Quaternion.Lerp(
            tower.localRotation,
            Quaternion.Euler(tiltAngle, tower.localRotation.eulerAngles.y, 0),
            Time.deltaTime * tiltSpeed);
        WheelRotation(moveInput, turnInput);
    }
    void RotateWheels()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        // Вращение колес при движении вперед/назад и повороте
        float forwardRotation = moveInput * speedWheels * Time.deltaTime;

        foreach (Transform wheel in rightWheels)
        {
            wheel.Rotate(Vector3.right, forwardRotation);  // Вращение правых колес
        }
        foreach (Transform wheel in leftWheels)
        {
            wheel.Rotate(Vector3.right, forwardRotation);  // Вращение левых колес
        }

        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float turnRotation = turnInput * rotationSpeed * Time.deltaTime;
            foreach (Transform wheel in rightWheels)
            {
                wheel.Rotate(Vector3.right, -turnRotation);  // Противоположное вращение правых колес
            }
            foreach (Transform wheel in leftWheels)
            {
                wheel.Rotate(Vector3.right, turnRotation);  // Вращение левых колес
            }
        }
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
}
