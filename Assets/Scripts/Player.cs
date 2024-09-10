using System.Collections;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Header("Wheels Setting")]
    public Transform[] rightWheels;
    public Transform[] leftWheels;
    public float speed = 5f;
    public float speedWheels = 60f;
    public float speedRotation = 40f;

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
    }
    void Update()
    {
        Move();
        MoveTower();
        Attack();

        // Привязываем корпус как Follow и башню как LookAt
        CameraShake.Instance.UpdateCamera(tower);
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
        if (isDestroyed) return;
        float forward_back = Input.GetAxis("Vertical") * Time.deltaTime * speed;
        float left_right = Input.GetAxis("Horizontal") * Time.deltaTime * speedRotation;

        isMoveSound = Mathf.Abs(forward_back) > 0.01f || Mathf.Abs(left_right) > 0.01f;

        if (isMoveSound && !wasMovingSound)
        {
            // Плавное увеличение звука движения и уменьшение звука ожидания
            moveAudioSource.DOFade(0.5f, 0.5f); // Увеличиваем громкость звука движения
            waitAudioSource.DOFade(0f, 0.5f); // Уменьшаем громкость звука ожидания
            isAttack = true;
        }
        else if (!isMoveSound && wasMovingSound)
        {
            // Плавное уменьшение звука движения и увеличение звука ожидания
            moveAudioSource.DOFade(0f, 0.5f); // Уменьшаем громкость звука движения
            waitAudioSource.DOFade(0.5f, 0.5f); // Увеличиваем громкость звука ожидания
            isAttack = false;
        }

        wasMovingSound = isMoveSound;

        transform.Translate(Vector3.forward * forward_back);
        transform.Rotate(Vector3.up, left_right);

        // Определяем целевой угол в зависимости от движения вперед или назад
        if (forward_back > 0)
        {
            targetTiltAngle = -2.0f; // Увеличиваем угол наклона вперед
        }
        else if (forward_back < 0)
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


        WheelRotation(forward_back, left_right);
    }
    void WheelRotation(float forward_back, float left_right)
    {
        foreach (Transform wheel in rightWheels)
        {
            wheel.Rotate(Vector3.right, forward_back * speedWheels);
        }
        foreach (Transform wheel in leftWheels)
        {
            wheel.Rotate(Vector3.right, forward_back * speedWheels);
        }

        if (left_right != 0)
        {
            foreach (Transform wheel in rightWheels)
            {
                wheel.Rotate(Vector3.right, -left_right);
            }
            foreach (Transform wheel in leftWheels)
            {
                wheel.Rotate(Vector3.right, left_right);
            }
        }
    }
}
