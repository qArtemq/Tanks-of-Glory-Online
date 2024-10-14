using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviourPun
{
    [Header("UI Elements")]
    public TextMeshProUGUI redTeamScoreText;
    public TextMeshProUGUI blueTeamScoreText;

    [Header("Game Settings")]
    private int maxScore = 8;

    public static int redTeamScore = 0;
    public static int blueTeamScore = 0;

    public GameObject gameOverMenu;
    public TextMeshProUGUI winnerText;
    void Start()
    {
        // ��������� UI ��� ������
        UpdateScoreUI();
    }

    [PunRPC]
    public void UpdateScore(string team)
    {
        if (team == "Red")
        {
            blueTeamScore++;
        }
        else if (team == "Blue")
        {
            redTeamScore++;
        }

        // ��������� UI ��� ���� ������� ����� RPC
        photonView.RPC("UpdateScoreUI_RPC", RpcTarget.All, redTeamScore, blueTeamScore);

        // �������� �� ���������� ������������� ����� � ���������� ����
        if (redTeamScore >= maxScore)
        {
            // ���� ������� ������� ��������, �� ������ ��� �������� ��������� � ������, � ����� � � ���������
            photonView.RPC("EndGame_RPC_Win", RpcTarget.All, "Red");
        }
        else if (blueTeamScore >= maxScore)
        {
            // ���� ����� ������� ��������, �� ������ ��� �������� ��������� � ������, � ������� � � ���������
            photonView.RPC("EndGame_RPC_Win", RpcTarget.All, "Blue");
        }
    }

    [PunRPC]
    public void UpdateScoreUI_RPC(int redScore, int blueScore)
    {
        // ����������� ����� ��� ����������� �����
        redTeamScore = redScore;
        blueTeamScore = blueScore;
        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        if (redTeamScoreText != null)
        {
            redTeamScoreText.text = redTeamScore.ToString("0");
        }
        if (blueTeamScoreText != null)
        {
            blueTeamScoreText.text = blueTeamScore.ToString("0");
        }
    }


    [PunRPC]
    void EndGame_RPC_Win(string winningTeam)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // ���� ������� ��������, ������� ������ ��������� � ������, � ����������� ������� ����� �� ���������
        gameOverMenu.SetActive(true);
        winnerText.text = $"{winningTeam} Team Wins!";

        // ������ ��������, ����� ����� 2 ������� ���������� ����� � ����
        StartCoroutine(DelayBeforeTimeStop(2f));
    }

    IEnumerator DelayBeforeTimeStop(float delayTime)
    {
        // ���� ��������� ��������
        yield return new WaitForSeconds(delayTime);

        // ������������� ����� ����� ����, ��� �������� ������ ������
        Time.timeScale = 0;
    }
}
