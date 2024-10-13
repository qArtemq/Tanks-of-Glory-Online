using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public loadingbar loadingBar; // ������ �� ������ ����������

    void FixedUpdate()
    {
        // ���������, ���� ��������� �������� � ��� �� ���������� � �������
        if (loadingBar.isFilled && !PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Battle");
    }
}
