using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject tankPrefab;  // ������ �����
    public GameObject spherePrefab;  // ������ ����� (��� ������ ������)
    public Transform spawnPoint;   // ����� ��������� ������ �����
    public float speed = 0.01f;       // �������� �����������

    private GameObject currentTank; // ������� ���� �� �����

    void Start()
    {
        SpawnTank();
    }

    // ����� ��� ������ �����
    public void SpawnTank()
    {
        // ������� ����� ���� �� ����� ���������
        currentTank = Instantiate(tankPrefab, spawnPoint.position, spawnPoint.rotation);

        // ������� ��������� ��� � �������� ��� ��������
        GameObject spawnedSphere = Instantiate(spherePrefab, spawnPoint.position, Quaternion.identity);
        StartCoroutine(MoveSphere(spawnedSphere, spawnPoint.position));
    }

    // ������� ��� ����������� �����
    private IEnumerator MoveSphere(GameObject sphere, Vector3 targetPos)
    {
        // ���� ������ �� ��������� ������� �������
        while (Vector3.Distance(sphere.transform.position, targetPos) > 0.1f)
        {
            // ���������� ��� � ������� �������
            sphere.transform.position = Vector3.Lerp(sphere.transform.position, targetPos, speed * Time.deltaTime);
            // ���� �� ���������� �����
            yield return null;
        }

        // ���������� ������ (���� ��� �����)
        Destroy(sphere);
    }

    // ����� ��� ����������� �����
    public void DestroyTank(GameObject tank)
    {
        // ���������� ���� ����� 10 ������
        Destroy(tank, 10);

        // ��������� �������� ��� �������� ����� ��������� �����
        StartCoroutine(RespawnTank());
    }

    // ������� ��� �������� ����� ����� 10 ������
    IEnumerator RespawnTank()
    {
        // ��� 10 ������ ����� ���������
        yield return new WaitForSeconds(12f);

        // ������� ����� ����
        SpawnTank();
    }
}
