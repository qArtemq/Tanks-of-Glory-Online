using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public loadingbar loadingBar; // Ссылка на скрипт индикатора

    void FixedUpdate()
    {
        // Проверяем, если индикатор заполнен и еще не подключены к серверу
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
