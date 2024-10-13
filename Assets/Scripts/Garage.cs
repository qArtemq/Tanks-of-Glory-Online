using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Garage : MonoBehaviour
{
    // Метод для перехода на сцену Гараж
    public void GoToGarage()
    {
        if (SceneManager.GetActiveScene().name != "Garage")
        {
            // Проверяем, подключен ли клиент к MasterServer перед загрузкой сцены
            if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.ConnectedToMasterServer)
            {
                PhotonNetwork.LoadLevel("Garage");
            }
            else
            {
                PhotonNetwork.ReconnectAndRejoin(); // Переподключение к MasterServer
            }
        }
    }

    // Метод для перехода на сцену Бой
    public void GoToBattle()
    {
        if (SceneManager.GetActiveScene().name != "Battle")
        {
            // Проверяем, подключен ли клиент к MasterServer перед загрузкой сцены
            if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.ConnectedToMasterServer)
            {
                PhotonNetwork.LoadLevel("Battle");
            }
            else
            {
                PhotonNetwork.ReconnectAndRejoin(); // Переподключение к MasterServer
            }
        }
    }
}
