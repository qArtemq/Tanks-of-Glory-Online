using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Quit : MonoBehaviourPunCallbacks
{
    // Метод для выхода из игры
    public void QuitGame()
    {
        Application.Quit();
    }

    // Метод для возврата на главную сцену
    public void GoToMainMenu()
    {
        // Проверяем, находится ли игрок в комнате
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();  // Выйти из комнаты, если в ней находимся
        }

        // Загрузка главной сцены
        SceneManager.LoadScene("Battle");
    }
}
