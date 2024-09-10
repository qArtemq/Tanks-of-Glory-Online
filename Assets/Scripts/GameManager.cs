using Cinemachine;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject tankPrefab;  // Префаб танка
    public GameObject spherePrefab;  // Префаб сферы
    public Transform[] spawnPoint;   // Точка появления нового танка
    public float speed = 20f;       // Скорость перемещения сферы

    private GameObject currentTank; // Текущий танк на сцене
    private GameObject spawnedSphere; // Шар, который появляется после взрыва
    private CinemachineVirtualCamera tankCamera; // Виртуальная камера танка

    private Destroy destroy;

    void Start()
    {
        SpawnTank();
    }
    public Transform RandomSpawn()
    {
        int randomIndex = Random.Range(0, spawnPoint.Length);
        return spawnPoint[randomIndex];
    }

    // Метод для спавна танка
    public void SpawnTank()
    {
        Transform randomSpawnPoint = RandomSpawn();
        currentTank = Instantiate(tankPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
        tankCamera = currentTank.GetComponentInChildren<CinemachineVirtualCamera>();
        CameraShake.Instance.UpdateCamera(currentTank.transform.Find("Tank_Tower"));
        destroy = currentTank.GetComponent<Destroy>();
    }
    private void DetachCameraFromTank()
    {
        // Отключаем виртуальную камеру от танка
        tankCamera.gameObject.transform.SetParent(null);
    }
    private void AttachCameraToSphere(GameObject sphere)
    {
        // Настраиваем камеру для слежения за сферой
        tankCamera.Follow = sphere.transform;
        tankCamera.LookAt = sphere.transform;
    }

    private void AttachCameraToTank()
    {
        if (tankCamera != null)
        {
            tankCamera.Follow = currentTank.transform.Find("Tank_Tower");
            tankCamera.LookAt = currentTank.transform.Find("Tank_Tower");
        }
    }

    // Метод для уничтожения танка
    public void TankToSpawn(Transform tank)
    {
        Transform spawnPointForTank = RandomSpawn();

        DetachCameraFromTank();

        spawnedSphere = Instantiate(spherePrefab, tank.transform.position, Quaternion.identity);
        AttachCameraToSphere(spawnedSphere);

        // Начинаем движение шара к точке респауна
        StartCoroutine(MoveSphere(spawnedSphere, spawnPointForTank.position));
    }


    // Корутин для перемещения сферы
    private IEnumerator MoveSphere(GameObject sphere, Vector3 targetPos)
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
        Destroy(tankCamera.gameObject);
        Destroy(sphere);  // Уничтожаем шар после его движения

        StartCoroutine(RespawnTankWithDelay(2f, targetPos));
    }

    // Корутин для респауна танка и возврата камеры на танк
    private IEnumerator RespawnTankWithDelay(float delay, Vector3 spawnPos)
    {
        yield return new WaitForSeconds(delay);

        // Спавним новый танк
        currentTank = Instantiate(tankPrefab, spawnPos, Quaternion.identity);
        // Находим камеру у нового танка
        tankCamera = currentTank.GetComponentInChildren<CinemachineVirtualCamera>();
        // Привязываем камеру к башне нового танка
        AttachCameraToTank();
        destroy.isDestroyed = false;  // Сбрасываем флаг уничтожения
    }
}
