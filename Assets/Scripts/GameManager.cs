using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject tankPrefab;  // Префаб танка
    public GameObject spherePrefab;  // Префаб сферы (или другой объект)
    public Transform spawnPoint;   // Точка появления нового танка
    public float speed = 0.01f;       // Скорость перемещения

    private GameObject currentTank; // Текущий танк на сцене

    void Start()
    {
        SpawnTank();
    }

    // Метод для спавна танка
    public void SpawnTank()
    {
        // Создаем новый танк на точке появления
        currentTank = Instantiate(tankPrefab, spawnPoint.position, spawnPoint.rotation);

        // Спавним невидимый шар и начинаем его движение
        GameObject spawnedSphere = Instantiate(spherePrefab, spawnPoint.position, Quaternion.identity);
        StartCoroutine(MoveSphere(spawnedSphere, spawnPoint.position));
    }

    // Корутин для перемещения сферы
    private IEnumerator MoveSphere(GameObject sphere, Vector3 targetPos)
    {
        // Пока объект не достигнет целевой позиции
        while (Vector3.Distance(sphere.transform.position, targetPos) > 0.1f)
        {
            // Перемещаем шар к целевой позиции
            sphere.transform.position = Vector3.Lerp(sphere.transform.position, targetPos, speed * Time.deltaTime);
            // Ждем до следующего кадра
            yield return null;
        }

        // Уничтожаем объект (если это нужно)
        Destroy(sphere);
    }

    // Метод для уничтожения танка
    public void DestroyTank(GameObject tank)
    {
        // Уничтожаем танк через 10 секунд
        Destroy(tank, 10);

        // Запускаем корутину для респауна через некоторое время
        StartCoroutine(RespawnTank());
    }

    // Корутин для респауна танка через 10 секунд
    IEnumerator RespawnTank()
    {
        // Ждём 10 секунд перед респауном
        yield return new WaitForSeconds(12f);

        // Спауним новый танк
        SpawnTank();
    }
}
