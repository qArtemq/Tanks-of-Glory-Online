using Cinemachine;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject redTankPrefab; // Префаб для команды "Красные"
    public GameObject blueTankPrefab; // Префаб для команды "Синие"

    public Transform[] redTeamSpawnPoints; // Точки спавна для команды "Красные"
    public Transform[] blueTeamSpawnPoints; // Точки спавна для команды "Синие"

    public float speed = 15f;       // Скорость перемещения сферы
    public GameObject spherePrefab;

    private GameObject currentTank; // Текущий танк на сцене
    private GameObject spawnedSphere; // Шар, который появляется после взрыва
    private CinemachineVirtualCamera tankCamera; // Виртуальная камера танка
    public GameObject cinemachineCameraPrefab; // Префаб камеры Cinemachine
    private bool isMovingSphere = false;

    private Destroy destroy;
    Player player;
    private Transform lastRedSpawn;
    private Transform lastBlueSpawn;

    public float arcHeight = 5f; // Высота дуги
    public float zoomSpeed = 0.1f; // Скорость зума
    private float t = 0.5f; // Позиция на дуге (от 0 до 1), 0.5 - середина
    private CinemachineTransposer transposer; // Для работы с offset-ом камеры

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Проверьте, что `TeamManager.selectedTeam` не null и правильно назначено
        if (TeamManager.selectedTeam == "Red")
        {
            SpawnTank(redTankPrefab, redTeamSpawnPoints);
        }
        else if (TeamManager.selectedTeam == "Blue")
        {
            SpawnTank(blueTankPrefab, blueTeamSpawnPoints);
        }
    }
    void Update()
    {
        // Проверяем нажатие клавиш Z и X
        if (Input.GetKey(KeyCode.O))
        {
            // Уменьшаем t, перемещаем камеру ближе к первой точке (pointA)
            t = Mathf.Clamp01(t - zoomSpeed / 40);
        }
        else if (Input.GetKey(KeyCode.I))
        {
            // Увеличиваем t, перемещаем камеру ближе ко второй точке (pointB)
            t = Mathf.Clamp01(t + zoomSpeed / 40);
        }

        // Обновляем смещение камеры, если Transposer инициализирован
        if (transposer != null)
        {
            Vector3 newOffset = CalculateArcOffset(t);
            transposer.m_FollowOffset = newOffset;
        }
    }

    // Метод для вычисления смещения камеры по дуге
    private Vector3 CalculateArcOffset(float t)
    {
        // Базовый offset между двумя точками A и B
        Vector3 startOffset = new Vector3(0f, 1.7f, -4.5f); // Начальная точка смещения
        Vector3 endOffset = new Vector3(0f, 7f, -5.25f);     // Конечная точка смещения

        // Интерполяция по X и Z (горизонтальная плоскость)
        Vector3 position = Vector3.Lerp(startOffset, endOffset, t);

        // Добавляем высоту дуги по Y
        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        position.y += arc;

        return position;
    }

    [PunRPC]
    private void SetTankVisibility(int tankViewID, bool isInvisible)
    {
        // Получаем объект танка по его PhotonView ID
        GameObject tank = PhotonView.Find(tankViewID).gameObject;

        Renderer[] renderers = tank.GetComponentsInChildren<Renderer>();
        Material[] materials = new Material[renderers.Length];

        // Меняем материалы для видимости/невидимости
        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;

            if (isInvisible)
            {
                materials[i].SetInt("_Cull", 1); // Делаем объект невидимым
            }
            else
            {
                materials[i].SetInt("_Cull", 2); // Делаем объект видимым
            }
        }
    }

    private IEnumerator MakeTankInvisible(GameObject tank, float waitTime, float blinkDuration, int blinkCount)
    {
        player = FindObjectOfType<Player>();
        player.isInvisible = true;
        PhotonView photonView = PhotonView.Get(this);

        // Делаем танк невидимым
        photonView.RPC("SetTankVisibility", RpcTarget.All, tank.GetComponent<PhotonView>().ViewID, true);

        yield return new WaitForSeconds(waitTime);

        // Выполняем мигание перед тем, как сделать танк видимым
        for (int j = 0; j < blinkCount; j++)
        {
            // Делаем танк невидимым
            photonView.RPC("SetTankVisibility", RpcTarget.All, tank.GetComponent<PhotonView>().ViewID, true);

            yield return new WaitForSeconds(blinkDuration / 2); // Короткая пауза

            // Делаем танк видимым
            photonView.RPC("SetTankVisibility", RpcTarget.All, tank.GetComponent<PhotonView>().ViewID, false);

            yield return new WaitForSeconds(blinkDuration / 2); // Короткая пауза
        }

        // Делаем танк снова видимым
        photonView.RPC("SetTankVisibility", RpcTarget.All, tank.GetComponent<PhotonView>().ViewID, false);
        player.isInvisible = false;
    }

    // Метод для спавна танка
    public void SpawnTank(GameObject tankPrefab, Transform[] spawnPoints)
    {
        int randomIndex = Random.Range(0, spawnPoints.Length); // Случайный выбор спавн-точки
        currentTank = PhotonNetwork.Instantiate(tankPrefab.name, spawnPoints[randomIndex].position, spawnPoints[randomIndex].rotation);

        // Начинаем корутину анимации прозрачности
        StartCoroutine(MakeTankInvisible(currentTank, 5f, 0.5f, 5));

        // Добавляем камеру Cinemachine на танк
        AttachCinemachineCameraToTank();
    }
    private void AttachCinemachineCameraToTank()
    {
        // Проверяем, есть ли уже камера
        if (tankCamera == null)
        {
            // Если камеры еще нет, ищем существующую камеру на сцене
            tankCamera = FindObjectOfType<CinemachineVirtualCamera>();

            // Если камера не найдена, создаем новую
            if (tankCamera == null)
            {
                GameObject cameraInstance = Instantiate(cinemachineCameraPrefab);
                tankCamera = cameraInstance.GetComponent<CinemachineVirtualCamera>();
            }
        }

        // Привязываем камеру к танку
        if (currentTank != null)
        {
            Transform tankTower = currentTank.transform.Find("Tank_Tower");
            if (tankTower != null)
            {
                tankCamera.Follow = tankTower;
                tankCamera.LookAt = tankTower;
                Debug.Log("Cinemachine camera attached to the tank.");

                // Настраиваем Transposer для управления смещением камеры
                transposer = tankCamera.GetCinemachineComponent<CinemachineTransposer>();

                if (transposer != null)
                {
                    // Инициализируем смещение камеры (начальная позиция на дуге)
                    transposer.m_FollowOffset = CalculateArcOffset(t);
                }
            }
            else
            {
                Debug.LogError("Tank tower not found.");
            }
        }
    }

    private void AttachCameraToSphere(GameObject sphere)
    {
        // Ищем уже существующую камеру
        if (tankCamera == null)
        {
            // Ищем камеру на сцене или добавляем вручную
            tankCamera = FindObjectOfType<CinemachineVirtualCamera>();

            if (tankCamera == null)
            {
                Debug.LogError("CinemachineVirtualCamera not found.");
                return;
            }
        }

        // Привязываем камеру к сфере
        tankCamera.Follow = sphere.transform;
        tankCamera.LookAt = sphere.transform;

        Debug.Log("Cinemachine camera attached to the sphere.");
    }

    // Метод для уничтожения танка
    public void TankToSpawn(Transform tank)
    {
        if (isMovingSphere) return;

        isMovingSphere = true;  // Блокируем повторный запуск корутины

        // Выбираем случайную точку спавна
        Transform spawnPointForTank = RandomSpawn();

        DetachCameraFromTank();

        spawnedSphere = Instantiate(spherePrefab, tank.transform.position, Quaternion.identity);

        AttachCameraToSphere(spawnedSphere);

        // Начинаем движение шара к точке респауна
        StartCoroutine(MoveSphere(spawnedSphere, spawnPointForTank.position, spawnPointForTank));
    }

    private void DetachCameraFromTank()
    {
        // Ищем камеру на сцене или добавляем вручную
        tankCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (tankCamera != null)
        {
            tankCamera.Follow = null;
            tankCamera.LookAt = null;
        }
    }

    private Transform RandomSpawn()
    {

        if (TeamManager.selectedTeam == "Red")
        {
            // Выбор случайной точки спавна для лкрасной команды
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, redTeamSpawnPoints.Length);
            } while (redTeamSpawnPoints[randomIndex] == lastRedSpawn);

            lastRedSpawn = redTeamSpawnPoints[randomIndex];
            return redTeamSpawnPoints[randomIndex];
        }
        else if (TeamManager.selectedTeam == "Blue")
        {
            // Выбор случайной точки спавна для синей команды
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, blueTeamSpawnPoints.Length);
            } while (blueTeamSpawnPoints[randomIndex] == lastBlueSpawn);

            lastBlueSpawn = blueTeamSpawnPoints[randomIndex];
            return blueTeamSpawnPoints[randomIndex];
        }

        return null; // Возвращаем null, если команда не выбрана
    }

    // Корутин для перемещения сферы
    private IEnumerator MoveSphere(GameObject sphere, Vector3 targetPos, Transform spawnPointForTank)
    {
        Vector3 startPos = sphere.transform.position;
        float journey = 0f;
        float duration = Vector3.Distance(startPos, targetPos) / speed;  // Время пути в зависимости от скорости
        float arcHeight = 10f;  // Высота дуги

        while (journey < 1f)
        {
            journey += Time.deltaTime / duration;

            // Интерполяция по XZ (горизонтальная плоскость)
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, journey);

            // Добавляем смещение по Y для создания дуги
            float height = Mathf.Sin(Mathf.PI * journey) * arcHeight;
            currentPos.y += height;

            // Обновляем позицию шара
            sphere.transform.position = currentPos;
            yield return null;
        }
        Destroy(sphere);  // Уничтожаем шар после его движения

        isMovingSphere = false; // Сбрасываем блокировку

        if (TeamManager.selectedTeam == "Red")
        {
            StartCoroutine(RespawnTankWithDelay(1f, targetPos, spawnPointForTank.rotation, redTankPrefab));
        }
        else if (TeamManager.selectedTeam == "Blue")
        {
            StartCoroutine(RespawnTankWithDelay(1f, targetPos, spawnPointForTank.rotation, blueTankPrefab));
        }

    }

    private IEnumerator RespawnTankWithDelay(float delay, Vector3 spawnPos, Quaternion spawnRotation, GameObject tankPrefab)
    {
        yield return new WaitForSeconds(delay);

        // Спавним новый танк с переданной позицией и ротацией
        currentTank = PhotonNetwork.Instantiate(tankPrefab.name, spawnPos, spawnRotation);

        // Находим компонент Destroy в новом танке
        destroy = currentTank.GetComponent<Destroy>();

        // Начинаем анимацию появления танка с прозрачности
        StartCoroutine(MakeTankInvisible(currentTank, 5f, 0.5f, 5));

        // Находим камеру у нового танка
        tankCamera = currentTank.GetComponentInChildren<CinemachineVirtualCamera>();

        // Если камера существует, заново привязываем её к новому танку
        AttachCinemachineCameraToTank();

        // Сбрасываем флаг уничтожения
        destroy.isDestroyed = false;
    }

}