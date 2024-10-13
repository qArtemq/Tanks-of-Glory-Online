using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Collections;

public class Boost : MonoBehaviourPunCallbacks
{
    public Image boostFillImage;  // ������ ���������� ��� �����
    public TextMeshProUGUI boostCooldownText; // ����� ��� ����������� ������� �����������
    private Player player; // ������ �� Player ������
    public Canvas canvas;

    private void Start()
    {
        // ������� Player ������ ������ ��� ���������� ������
        if (photonView.IsMine)
        {
            player = GetComponent<Player>();
            canvas.gameObject.SetActive(true);  // �������� Canvas ��� ���������� ������
        }
        else
        {
            canvas.gameObject.SetActive(false);  // ��������� Canvas ��� ������ �������
        }
    }

    private void Update()
    {
        // ��������� UI ������ ��� ���������� ������
        if (photonView.IsMine && player != null)
        {
            UpdateBoostUI();
        }
    }

    private void UpdateBoostUI()
    {
        if (player.isBoosting)
        {
            // ��������� ���������� ������ � ����������� �� ������� �����
            boostFillImage.fillAmount = player.boostCooldownTimer / player.boostDuration;
            boostCooldownText.text = Mathf.Ceil(player.boostCooldownTimer).ToString(); // ��������� �����
            boostCooldownText.gameObject.SetActive(true);  // �������� �����
            boostFillImage.gameObject.SetActive(true); // �������� ������
        }
        else if (player.boostCooldownTimer > 0)
        {
            // ���� ���� �� �����������, ���������� ���������� ����� �� �����
            boostFillImage.fillAmount = player.boostCooldownTimer / player.boostCooldown;
            boostCooldownText.text = Mathf.Ceil(player.boostCooldownTimer).ToString(); // ��������� �����
            boostCooldownText.gameObject.SetActive(true);  // �������� �����
            boostFillImage.gameObject.SetActive(true); // �������� ������
        }
        else
        {
            // ���� ���� �������� � ������ �� 0, ��������� UI
            boostFillImage.fillAmount = 0f;
            boostCooldownText.gameObject.SetActive(false);  // ��������� �����
            boostFillImage.gameObject.SetActive(false); // ��������� ������
        }
    }
}
