using System.Collections;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Wheels Setting")]
    public WheelCollider[] rightWheelColliders;  // Коллайдеры правых колес
    public WheelCollider[] leftWheelColliders;   // Коллайдеры левых колес
    public Transform[] rightWheels;
    public Transform[] leftWheels;
    public float motorTorque = 1500f;   // Сила на колеса для движения вперед

    [Header("Tower Setting")]
    public Transform tower;
    public Transform muzzle;
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

    public AudioSource[] AudioSources { get { return audioSources; } set { audioSources = value; } }


    private bool isAttack = false;
    [Header("Tank States")]
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

        // Добавляем Rigidbody для взаимодействия с физикой
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -1f, 0); // Центр массы для большей устойчивости
    }
    void Update()
    {
        Move();
        MoveTower();
        Attack();
    }
    void Attack()
    {
        if (isDestroyed) return;
        if (Input.GetKeyDown(KeyCode.Space) && canShoot)
        {
            attackAudioSource.volume = 3f;
            StartCoroutine(WaitAttack());

            if (isAttack)
            {
                attackAudioSource.PlayOneShot(shotSound);
            }
            else
            {
                attackAudioSource.PlayOneShot(shotSound);
            }

            Vector3 recoilPosition = defaultPosition - new Vector3(0, 0, recoilDistance);

            CameraShake.Instance.ShakeCamera(5f, 1f);

            muzzle.DOLocalMove(recoilPosition, 0.2f).OnComplete(() =>
            {
                muzzle.DOLocalMove(defaultPosition, 0.2f);
            });
        }
    }

    public IEnumerator WaitAttack()
    {
        canShoot = false;
        if (isAttack)
        {
            attackAudioSource.PlayOneShot(reloadSound);
        }
        else
        {
            attackAudioSource.PlayOneShot(reloadSound);
        }

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
        if (isDestroyed)
        {
            // Останавливаем звук движения и ожидания
            moveAudioSource.volume = 0f;
            waitAudioSource.volume = 0f;
            return; // Прекращаем выполнение метода Move
        }
        float motorInput = Input.GetAxis("Vertical") * motorTorque;
        float steerInput = Input.GetAxis("Horizontal");

        // Применение трения для колес
        ApplyWheelFriction(steerInput);


        isMoveSound = Mathf.Abs(motorInput) > 0.01f || Mathf.Abs(steerInput) > 0.01f;

        if (isMoveSound && !wasMovingSound)
        {
            // Плавное увеличение звука движения и уменьшение звука ожидания
            moveAudioSource.DOFade(0.5f, 1f); // Увеличиваем громкость звука движения
            waitAudioSource.DOFade(0f, 1f); // Уменьшаем громкость звука ожидания
            isAttack = true;
        }
        else if (!isMoveSound && wasMovingSound)
        {
            // Плавное уменьшение звука движения и увеличение звука ожидания
            moveAudioSource.DOFade(0f, 1f); // Уменьшаем громкость звука движения
            waitAudioSource.DOFade(0.5f, 1f); // Увеличиваем громкость звука ожидания
            isAttack = false;
        }

        wasMovingSound = isMoveSound;

        // Если нет ввода (отпущены клавиши движения), применяем торможение
        if (Mathf.Abs(motorInput) < 0.1f && Mathf.Abs(steerInput) < 0.1f)
        {
            foreach (WheelCollider wheel in rightWheelColliders)
            {
                wheel.brakeTorque = 3000f; // Применяем торможение на правые колеса
                wheel.motorTorque = 0f;
            }
            foreach (WheelCollider wheel in leftWheelColliders)
            {
                wheel.brakeTorque = 3000f; // Применяем торможение на левые колеса
                wheel.motorTorque = 0f;
            }
        }
        else
        {
            // Если есть ввод, убираем торможение и применяем силу на колеса
            foreach (WheelCollider wheel in rightWheelColliders)
            {
                wheel.motorTorque = motorInput;
                wheel.brakeTorque = 0f;  // Отключаем торможение
            }
            foreach (WheelCollider wheel in leftWheelColliders)
            {
                wheel.motorTorque = motorInput;
                wheel.brakeTorque = 0f;  // Отключаем торможение
            }
        }

        // Определяем целевой угол в зависимости от движения вперед или назад
        if (motorInput > 0)
        {
            targetTiltAngle = -2.0f; // Увеличиваем угол наклона вперед
        }
        else if (motorInput < 0)
        {
            targetTiltAngle = 1.0f; // Уменьшаем угол наклона назад
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

        // Применение ограничения на поворот
        if (steerInput != 0)
        {
            float turnTorque = steerInput * motorTorque; // Используем rotateTorque для поворота
            foreach (WheelCollider wheel in rightWheelColliders)
            {
                wheel.motorTorque = -turnTorque; // Правые колеса двигаются в обратную сторону
            }
            foreach (WheelCollider wheel in leftWheelColliders)
            {
                wheel.motorTorque = turnTorque;  // Левые колеса двигаются вперед
            }


            UpdateWheelVisuals();
        }
        void UpdateWheelVisuals()
        {
            for (int i = 0; i < rightWheels.Length; i++)
            {
                UpdateWheelPosition(rightWheelColliders[i], rightWheels[i]);
            }

            for (int i = 0; i < leftWheels.Length; i++)
            {
                UpdateWheelPosition(leftWheelColliders[i], leftWheels[i]);
            }
        }
        void UpdateWheelPosition(WheelCollider collider, Transform wheelTransform)
        {
            Vector3 pos;
            Quaternion rot;
            collider.GetWorldPose(out pos, out rot);
            wheelTransform.position = pos;
            wheelTransform.rotation = rot;
        }
        void ApplyWheelFriction(float steerInput)
        {
            float targetStiffness = steerInput != 0 ? 0.5f : 1.0f;

            foreach (WheelCollider wheel in rightWheelColliders)
            {
                WheelFrictionCurve friction = wheel.sidewaysFriction;
                friction.stiffness = Mathf.Lerp(friction.stiffness, targetStiffness, Time.deltaTime * 2f);
                wheel.sidewaysFriction = friction;
            }

            foreach (WheelCollider wheel in leftWheelColliders)
            {
                WheelFrictionCurve friction = wheel.sidewaysFriction;
                friction.stiffness = Mathf.Lerp(friction.stiffness, targetStiffness, Time.deltaTime * 2f);
                wheel.sidewaysFriction = friction;
            }
        }
    }
}
