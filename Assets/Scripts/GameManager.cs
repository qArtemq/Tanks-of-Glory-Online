using Cinemachine;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject tankPrefab;  // ������ �����
    public GameObject spherePrefab;  // ������ �����
    public Transform[] spawnPoint;   // ����� ��������� ������ �����
    public float speed = 20f;       // �������� ����������� �����

    private GameObject currentTank; // ������� ���� �� �����
    private GameObject spawnedSphere; // ���, ������� ���������� ����� ������
    private CinemachineVirtualCamera tankCamera; // ����������� ������ �����

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

    // ����� ��� ������ �����
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
        // ��������� ����������� ������ �� �����
        tankCamera.gameObject.transform.SetParent(null);
    }
    private void AttachCameraToSphere(GameObject sphere)
    {
        // ����������� ������ ��� �������� �� ������
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

    // ����� ��� ����������� �����
    public void TankToSpawn(Transform tank)
    {
        Transform spawnPointForTank = RandomSpawn();

        DetachCameraFromTank();

        spawnedSphere = Instantiate(spherePrefab, tank.transform.position, Quaternion.identity);
        AttachCameraToSphere(spawnedSphere);

        // �������� �������� ���� � ����� ��������
        StartCoroutine(MoveSphere(spawnedSphere, spawnPointForTank.position));
    }


    // ������� ��� ����������� �����
    private IEnumerator MoveSphere(GameObject sphere, Vector3 targetPos)
    {
        Vector3 startPos = sphere.transform.position;
        float journey = 0f;
        float duration = Vector3.Distance(startPos, targetPos) / speed;  // ����� ���� � ����������� �� ��������
        float arcHeight = 10f;  // ������ ����

        while (journey < 1f)
        {
            journey += Time.deltaTime / duration;

            // ������������ �� XZ (�������������� ���������)
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, journey);

            // ��������� �������� �� Y ��� �������� ����
            float height = Mathf.Sin(Mathf.PI * journey) * arcHeight;
            currentPos.y += height;

            // ��������� ������� ����
            sphere.transform.position = currentPos;
            yield return null;
        }
        Destroy(tankCamera.gameObject);
        Destroy(sphere);  // ���������� ��� ����� ��� ��������

        StartCoroutine(RespawnTankWithDelay(2f, targetPos));
    }

    // ������� ��� �������� ����� � �������� ������ �� ����
    private IEnumerator RespawnTankWithDelay(float delay, Vector3 spawnPos)
    {
        yield return new WaitForSeconds(delay);

        // ������� ����� ����
        currentTank = Instantiate(tankPrefab, spawnPos, Quaternion.identity);
        // ������� ������ � ������ �����
        tankCamera = currentTank.GetComponentInChildren<CinemachineVirtualCamera>();
        // ����������� ������ � ����� ������ �����
        AttachCameraToTank();
        destroy.isDestroyed = false;  // ���������� ���� �����������
    }
}
