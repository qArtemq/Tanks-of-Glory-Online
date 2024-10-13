using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviourPunCallbacks
{
    public RectTransform rectComponent; // ������ ������������
    public Image imageComp; // ������ ������������
    public Canvas canvasSource; // ������ ������ ������� ����������
    public float health = 1f;
    private Camera localPlayerCamera; // ������ ���������� ������
    private Player player;

    // ����������� ��� ����� �������������
    public Health(Canvas canvas)
    {
        // ��������� ������ ��� �������� ������
        canvasSource = canvas;

        // ���� ���������� ������ �������
        rectComponent = canvasSource.GetComponentInChildren<RectTransform>();
        imageComp = rectComponent.GetComponentInChildren<Image>();
    }

    void Start()
    {
        imageComp.fillAmount = health;  // ������������� � ������� ��������� ��������
        player = GetComponent<Player>();

        // ����� ������� ������ ������
        if (Camera.main != null)
        {
            localPlayerCamera = Camera.main;  // �������� �������� ������
        }
        else
        {
            // ���� ������ �� "Main", ������� � � �������� �������� ������� �����
            localPlayerCamera = FindObjectOfType<Camera>();
        }
    }
    void Update()
    {
        if (localPlayerCamera != null)
        {
            // ������������ ������ �������� � ������ ���������� ������
            rectComponent.transform.LookAt(localPlayerCamera.transform);
        }
    }
    public void TakeDamage(float damage)
    {
        if (!photonView.IsMine || player.isInvisible) return; // ������ ������ ������� ����� �������� ��������

        // �������� ��������� �������� ��� ���������
        health -= damage;
        health = Mathf.Clamp(health, 0, 1);
        imageComp.fillAmount = health;

        // ���������� ���������� ������ �������
        photonView.RPC("UpdateHealthUI", RpcTarget.Others, health); // ��������� UI ��� ������ �������
    }

    // ����� ��� ���������� UI �� ���� ��������, ����� ���������
    [PunRPC]
    public void UpdateHealthUI(float currentHealth)
    {
        health = currentHealth;
        imageComp.fillAmount = health;
    }

}
