using Cinemachine;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject redTankPrefab; // ������ ��� ������� "�������"
    public GameObject blueTankPrefab; // ������ ��� ������� "�����"

    public Transform[] redTeamSpawnPoints; // ����� ������ ��� ������� "�������"
    public Transform[] blueTeamSpawnPoints; // ����� ������ ��� ������� "�����"

    public float speed = 15f;       // �������� ����������� �����
    public GameObject spherePrefab;

    private GameObject currentTank; // ������� ���� �� �����
    private GameObject spawnedSphere; // ���, ������� ���������� ����� ������
    private CinemachineVirtualCamera tankCamera; // ����������� ������ �����
    public GameObject cinemachineCameraPrefab; // ������ ������ Cinemachine
    private bool isMovingSphere = false;

    private Destroy destroy;
    Player player;
    private Transform lastRedSpawn;
    private Transform lastBlueSpawn;

    public float arcHeight = 5f; // ������ ����
    public float zoomSpeed = 0.1f; // �������� ����
    private float t = 0.5f; // ������� �� ���� (�� 0 �� 1), 0.5 - ��������
    private CinemachineTransposer transposer; // ��� ������ � offset-�� ������

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ���������, ��� `TeamManager.selectedTeam` �� null � ��������� ���������
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
        // ��������� ������� ������ Z � X
        if (Input.GetKey(KeyCode.O))
        {
            // ��������� t, ���������� ������ ����� � ������ ����� (pointA)
            t = Mathf.Clamp01(t - zoomSpeed / 40);
        }
        else if (Input.GetKey(KeyCode.I))
        {
            // ����������� t, ���������� ������ ����� �� ������ ����� (pointB)
            t = Mathf.Clamp01(t + zoomSpeed / 40);
        }

        // ��������� �������� ������, ���� Transposer ���������������
        if (transposer != null)
        {
            Vector3 newOffset = CalculateArcOffset(t);
            transposer.m_FollowOffset = newOffset;
        }
    }

    // ����� ��� ���������� �������� ������ �� ����
    private Vector3 CalculateArcOffset(float t)
    {
        // ������� offset ����� ����� ������� A � B
        Vector3 startOffset = new Vector3(0f, 1.7f, -4.5f); // ��������� ����� ��������
        Vector3 endOffset = new Vector3(0f, 7f, -5.25f);     // �������� ����� ��������

        // ������������ �� X � Z (�������������� ���������)
        Vector3 position = Vector3.Lerp(startOffset, endOffset, t);

        // ��������� ������ ���� �� Y
        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        position.y += arc;

        return position;
    }

    [PunRPC]
    private void SetTankVisibility(int tankViewID, bool isInvisible)
    {
        // �������� ������ ����� �� ��� PhotonView ID
        GameObject tank = PhotonView.Find(tankViewID).gameObject;

        Renderer[] renderers = tank.GetComponentsInChildren<Renderer>();
        Material[] materials = new Material[renderers.Length];

        // ������ ��������� ��� ���������/�����������
        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;

            if (isInvisible)
            {
                materials[i].SetInt("_Cull", 1); // ������ ������ ���������
            }
            else
            {
                materials[i].SetInt("_Cull", 2); // ������ ������ �������
            }
        }
    }

    private IEnumerator MakeTankInvisible(GameObject tank, float waitTime, float blinkDuration, int blinkCount)
    {
        player = FindObjectOfType<Player>();
        player.isInvisible = true;
        PhotonView photonView = PhotonView.Get(this);

        // ������ ���� ���������
        photonView.RPC("SetTankVisibility", RpcTarget.All, tank.GetComponent<PhotonView>().ViewID, true);

        yield return new WaitForSeconds(waitTime);

        // ��������� ������� ����� ���, ��� ������� ���� �������
        for (int j = 0; j < blinkCount; j++)
        {
            // ������ ���� ���������
            photonView.RPC("SetTankVisibility", RpcTarget.All, tank.GetComponent<PhotonView>().ViewID, true);

            yield return new WaitForSeconds(blinkDuration / 2); // �������� �����

            // ������ ���� �������
            photonView.RPC("SetTankVisibility", RpcTarget.All, tank.GetComponent<PhotonView>().ViewID, false);

            yield return new WaitForSeconds(blinkDuration / 2); // �������� �����
        }

        // ������ ���� ����� �������
        photonView.RPC("SetTankVisibility", RpcTarget.All, tank.GetComponent<PhotonView>().ViewID, false);
        player.isInvisible = false;
    }

    // ����� ��� ������ �����
    public void SpawnTank(GameObject tankPrefab, Transform[] spawnPoints)
    {
        int randomIndex = Random.Range(0, spawnPoints.Length); // ��������� ����� �����-�����
        currentTank = PhotonNetwork.Instantiate(tankPrefab.name, spawnPoints[randomIndex].position, spawnPoints[randomIndex].rotation);

        // �������� �������� �������� ������������
        StartCoroutine(MakeTankInvisible(currentTank, 5f, 0.5f, 5));

        // ��������� ������ Cinemachine �� ����
        AttachCinemachineCameraToTank();
    }
    private void AttachCinemachineCameraToTank()
    {
        // ���������, ���� �� ��� ������
        if (tankCamera == null)
        {
            // ���� ������ ��� ���, ���� ������������ ������ �� �����
            tankCamera = FindObjectOfType<CinemachineVirtualCamera>();

            // ���� ������ �� �������, ������� �����
            if (tankCamera == null)
            {
                GameObject cameraInstance = Instantiate(cinemachineCameraPrefab);
                tankCamera = cameraInstance.GetComponent<CinemachineVirtualCamera>();
            }
        }

        // ����������� ������ � �����
        if (currentTank != null)
        {
            Transform tankTower = currentTank.transform.Find("Tank_Tower");
            if (tankTower != null)
            {
                tankCamera.Follow = tankTower;
                tankCamera.LookAt = tankTower;
                Debug.Log("Cinemachine camera attached to the tank.");

                // ����������� Transposer ��� ���������� ��������� ������
                transposer = tankCamera.GetCinemachineComponent<CinemachineTransposer>();

                if (transposer != null)
                {
                    // �������������� �������� ������ (��������� ������� �� ����)
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
        // ���� ��� ������������ ������
        if (tankCamera == null)
        {
            // ���� ������ �� ����� ��� ��������� �������
            tankCamera = FindObjectOfType<CinemachineVirtualCamera>();

            if (tankCamera == null)
            {
                Debug.LogError("CinemachineVirtualCamera not found.");
                return;
            }
        }

        // ����������� ������ � �����
        tankCamera.Follow = sphere.transform;
        tankCamera.LookAt = sphere.transform;

        Debug.Log("Cinemachine camera attached to the sphere.");
    }

    // ����� ��� ����������� �����
    public void TankToSpawn(Transform tank)
    {
        if (isMovingSphere) return;

        isMovingSphere = true;  // ��������� ��������� ������ ��������

        // �������� ��������� ����� ������
        Transform spawnPointForTank = RandomSpawn();

        DetachCameraFromTank();

        spawnedSphere = Instantiate(spherePrefab, tank.transform.position, Quaternion.identity);

        AttachCameraToSphere(spawnedSphere);

        // �������� �������� ���� � ����� ��������
        StartCoroutine(MoveSphere(spawnedSphere, spawnPointForTank.position, spawnPointForTank));
    }

    private void DetachCameraFromTank()
    {
        // ���� ������ �� ����� ��� ��������� �������
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
            // ����� ��������� ����� ������ ��� �������� �������
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
            // ����� ��������� ����� ������ ��� ����� �������
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, blueTeamSpawnPoints.Length);
            } while (blueTeamSpawnPoints[randomIndex] == lastBlueSpawn);

            lastBlueSpawn = blueTeamSpawnPoints[randomIndex];
            return blueTeamSpawnPoints[randomIndex];
        }

        return null; // ���������� null, ���� ������� �� �������
    }

    // ������� ��� ����������� �����
    private IEnumerator MoveSphere(GameObject sphere, Vector3 targetPos, Transform spawnPointForTank)
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
        Destroy(sphere);  // ���������� ��� ����� ��� ��������

        isMovingSphere = false; // ���������� ����������

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

        // ������� ����� ���� � ���������� �������� � ��������
        currentTank = PhotonNetwork.Instantiate(tankPrefab.name, spawnPos, spawnRotation);

        // ������� ��������� Destroy � ����� �����
        destroy = currentTank.GetComponent<Destroy>();

        // �������� �������� ��������� ����� � ������������
        StartCoroutine(MakeTankInvisible(currentTank, 5f, 0.5f, 5));

        // ������� ������ � ������ �����
        tankCamera = currentTank.GetComponentInChildren<CinemachineVirtualCamera>();

        // ���� ������ ����������, ������ ����������� � � ������ �����
        AttachCinemachineCameraToTank();

        // ���������� ���� �����������
        destroy.isDestroyed = false;
    }

}