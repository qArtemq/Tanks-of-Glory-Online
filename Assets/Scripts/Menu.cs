using Photon.Realtime;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.SceneManagement;
public class Menu : MonoBehaviourPunCallbacks
{
    private const int maxPlayers = 10; // ������������ ���������� ������� � �������
    private const int minPlayers = 2; // ����������� ���������� ������� � �������

    public GameObject battleInfoPanel; // ������ "���������� � ���"
    public GameObject roomButtonPrefab; // ������ ������ ��� �������
    public Transform roomListContent; // ��������� ��� ������ ������

    private int blueTeamCount = 0; // ���������� ������� � ����� �������
    private int redTeamCount = 0;  // ���������� ������� � ������� �������

    public TextMeshProUGUI[] blueTeamText;
    public TextMeshProUGUI[] redTeamText;

    private string playerTeam = "";

    public void Start()
    {
        battleInfoPanel.SetActive(false);
        string enteredNickname = GenerateUniquePlayerId();
        PhotonNetwork.NickName = enteredNickname; // ������������� �������
        TeamManager.selectedTeam = ""; // ���������� ������� �� �������
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("��������� � ������-�������");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby(); // ������������ � �����, ���� �� ����������
        }
    }
    private string GenerateUniquePlayerId()
    {
        // ���������� ���������� ID, ��������, ��������� Guid
        return "Player_" + Guid.NewGuid().ToString("").Substring(0, 8); // ����� ������ 8 �������� GUID
    }

    // ����� ��� �������� �������
    public void CreateRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.ConnectedToMasterServer)
        {
            string roomId = GenerateRandomRoomId(); // ��������� ���������� ID �������
            RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };
            PhotonNetwork.CreateRoom(roomId, options);
        }
        else
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby(); // ������������ � �����, ���� �� ����������
            }
        }
    }
    // ���� ����� ����� ������, ����� ������� ������� �������
    public override void OnCreatedRoom()
    {
        AddRoomToBattleList();
        SwitchToBattleInfoTab();
    }
    public override void OnJoinedRoom()
    {
        SwitchToBattleInfoTab();
    }

    public void RefreshRoomList()
    {
        // ������������� �������� ���������� ������ ������
        PhotonNetwork.LeaveLobby();  // ������� ������ �� �����
        PhotonNetwork.JoinLobby();   // ����� ����� ������, ����� �������� ������
    }

    // ����� ��� ���������� ������� � ������ ����
    private void AddRoomToBattleList()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        GameObject newRoomButton = Instantiate(roomButtonPrefab, roomListContent);
        newRoomButton.GetComponentInChildren<TextMeshProUGUI>().text = $"�������: {playerCount}��{maxPlayers}";
    }
    public void JoinRoom(string roomId)
    {
        PhotonNetwork.JoinRoom(roomId);
    }
    private void SwitchToBattleInfoTab()
    {
        battleInfoPanel.SetActive(true);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // ������� ������ ������ ������
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }

        // ��������� ���������� �������
        foreach (RoomInfo room in roomList)
        {
            // ������� ������ ��� ������ �������
            GameObject newRoomButton = Instantiate(roomButtonPrefab, roomListContent);
            newRoomButton.GetComponentInChildren<TextMeshProUGUI>().text = $"�������: {room.PlayerCount}��{maxPlayers}";
            newRoomButton.GetComponent<Button>().onClick.AddListener(() => JoinRoom(room.Name));
        }
    }
    public void SelectTeamRed()
    {
        if (playerTeam != "Red") // ���������, ������ �� ����� ��� ������� �������
        {
            TeamManager.selectedTeam = "Red";
            playerTeam = "Red"; // ��������� ����������
            photonView.RPC("UpdateTeamSelection", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName, "Red");
        }
    }

    public void SelectTeamBlue()
    {
        if (playerTeam != "Blue") // ���������, ������ �� ����� ��� ����� �������
        {
            TeamManager.selectedTeam = "Blue";
            playerTeam = "Blue"; // ��������� ����������
            photonView.RPC("UpdateTeamSelection", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName, "Blue");
        }
    }

    [PunRPC]
    void UpdateTeamSelection(string playerName, string team)
    {
        if (team == "Red")
        {
            redTeamCount++;
            UpdateTeamDisplay(redTeamText, playerName, redTeamCount);
        }
        else if (team == "Blue")
        {
            blueTeamCount++;
            UpdateTeamDisplay(blueTeamText, playerName, blueTeamCount);
        }

        if (blueTeamCount + redTeamCount >= minPlayers)
        {
            StartGameForAll();
        }
    }
    private void UpdateTeamDisplay(TextMeshProUGUI[] teamTextArray, string nickname, int teamCount)
    {
        if (teamCount - 1 < teamTextArray.Length)
        {
            teamTextArray[teamCount - 1].text = nickname;
        }
    }
    private void StartGameForAll()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    // ����� ��� ��������� ���������� ID �������
    private string GenerateRandomRoomId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random random = new System.Random();
        char[] stringChars = new char[8]; // ����� ID, ��������, 8 ��������

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }
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

    public void GoToBattle()
    {
        if (SceneManager.GetActiveScene().name != "Battle")
        {
            // ���������, ��� ������ �� ������-�������
            if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.ConnectedToMasterServer)
            {
                PhotonNetwork.LoadLevel("Battle");
            }
            else
            {
                Debug.Log("��������������� � MasterServer ����� ��������� �� ����� Battle.");
                PhotonNetwork.ReconnectAndRejoin(); // ���������������� � MasterServer
            }
        }
    }
}