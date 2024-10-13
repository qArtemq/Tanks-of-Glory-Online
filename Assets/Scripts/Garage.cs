using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Garage : MonoBehaviour
{
    // ����� ��� �������� �� ����� �����
    public void GoToGarage()
    {
        if (SceneManager.GetActiveScene().name != "Garage")
        {
            // ���������, ��������� �� ������ � MasterServer ����� ��������� �����
            if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.ConnectedToMasterServer)
            {
                PhotonNetwork.LoadLevel("Garage");
            }
            else
            {
                PhotonNetwork.ReconnectAndRejoin(); // ��������������� � MasterServer
            }
        }
    }

    // ����� ��� �������� �� ����� ���
    public void GoToBattle()
    {
        if (SceneManager.GetActiveScene().name != "Battle")
        {
            // ���������, ��������� �� ������ � MasterServer ����� ��������� �����
            if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.ConnectedToMasterServer)
            {
                PhotonNetwork.LoadLevel("Battle");
            }
            else
            {
                PhotonNetwork.ReconnectAndRejoin(); // ��������������� � MasterServer
            }
        }
    }
}
