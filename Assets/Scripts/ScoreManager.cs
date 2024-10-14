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
        // Обновляем UI при старте
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

        // Обновляем UI для всех игроков через RPC
        photonView.RPC("UpdateScoreUI_RPC", RpcTarget.All, redTeamScore, blueTeamScore);

        // Проверка на достижение максимального счета и завершение игры
        if (redTeamScore >= maxScore)
        {
            // Если красная команда выиграла, то только она получает сообщение о победе, а синяя — о проигрыше
            photonView.RPC("EndGame_RPC_Win", RpcTarget.All, "Red");
        }
        else if (blueTeamScore >= maxScore)
        {
            // Если синяя команда выиграла, то только она получает сообщение о победе, а красная — о проигрыше
            photonView.RPC("EndGame_RPC_Win", RpcTarget.All, "Blue");
        }
    }

    [PunRPC]
    public void UpdateScoreUI_RPC(int redScore, int blueScore)
    {
        // Форматируем текст для отображения счёта
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
        // Если команда выиграла, выводим только сообщение о победе, а проигравшую команду можно не упоминать
        gameOverMenu.SetActive(true);
        winnerText.text = $"{winningTeam} Team Wins!";

        // Запуск корутины, чтобы через 2 секунды остановить время у всех
        StartCoroutine(DelayBeforeTimeStop(2f));
    }

    IEnumerator DelayBeforeTimeStop(float delayTime)
    {
        // Ждем окончания задержки
        yield return new WaitForSeconds(delayTime);

        // Останавливаем время после того, как анимации взрыва прошли
        Time.timeScale = 0;
    }
}
