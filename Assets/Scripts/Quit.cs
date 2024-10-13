using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Quit : MonoBehaviourPunCallbacks
{
    // ����� ��� ������ �� ����
    public void QuitGame()
    {
        Application.Quit();
    }

    // ����� ��� �������� �� ������� �����
    public void GoToMainMenu()
    {
        // ���������, ��������� �� ����� � �������
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();  // ����� �� �������, ���� � ��� ���������
        }

        // �������� ������� �����
        SceneManager.LoadScene("Battle");
    }
}
