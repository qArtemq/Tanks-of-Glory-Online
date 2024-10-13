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
    private const int maxPlayers = 10; // Максимальное количество игроков в комнате
    private const int minPlayers = 2; // Минимальное количество игроков в комнате

    public GameObject battleInfoPanel; // Панель "Информация о бое"
    public GameObject roomButtonPrefab; // Префаб кнопки для комнаты
    public Transform roomListContent; // Контейнер для кнопок комнат

    private int blueTeamCount = 0; // Количество игроков в синей команде
    private int redTeamCount = 0;  // Количество игроков в красной команде

    public TextMeshProUGUI[] blueTeamText;
    public TextMeshProUGUI[] redTeamText;

    private string playerTeam = "";

    public void Start()
    {
        battleInfoPanel.SetActive(false);
        string enteredNickname = GenerateUniquePlayerId();
        PhotonNetwork.NickName = enteredNickname; // Устанавливаем никнейм
        TeamManager.selectedTeam = ""; // Изначально команда не выбрана
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Подключен к мастер-серверу");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby(); // Подключаемся к лобби, если не подключены
        }
    }
    private string GenerateUniquePlayerId()
    {
        // Генерируем уникальный ID, например, используя Guid
        return "Player_" + Guid.NewGuid().ToString("").Substring(0, 8); // Берем первые 8 символов GUID
    }

    // Метод для создания комнаты
    public void CreateRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.ConnectedToMasterServer)
        {
            string roomId = GenerateRandomRoomId(); // Генерация случайного ID комнаты
            RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };
            PhotonNetwork.CreateRoom(roomId, options);
        }
        else
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby(); // Подключаемся к лобби, если не подключены
            }
        }
    }
    // Этот метод будет вызван, когда комната создана успешно
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
        // Принудительно вызываем обновление списка комнат
        PhotonNetwork.LeaveLobby();  // Сначала выйдем из лобби
        PhotonNetwork.JoinLobby();   // Затем снова зайдем, чтобы обновить список
    }

    // Метод для добавления комнаты в список битв
    private void AddRoomToBattleList()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        GameObject newRoomButton = Instantiate(roomButtonPrefab, roomListContent);
        newRoomButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Игроков: {playerCount}из{maxPlayers}";
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
        // Очищаем старый список комнат
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }

        // Добавляем актуальные комнаты
        foreach (RoomInfo room in roomList)
        {
            // Создаем кнопку для каждой комнаты
            GameObject newRoomButton = Instantiate(roomButtonPrefab, roomListContent);
            newRoomButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Игроков: {room.PlayerCount}из{maxPlayers}";
            newRoomButton.GetComponent<Button>().onClick.AddListener(() => JoinRoom(room.Name));
        }
    }
    public void SelectTeamRed()
    {
        if (playerTeam != "Red") // Проверяем, выбрал ли игрок уже красную команду
        {
            TeamManager.selectedTeam = "Red";
            playerTeam = "Red"; // Обновляем переменную
            photonView.RPC("UpdateTeamSelection", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName, "Red");
        }
    }

    public void SelectTeamBlue()
    {
        if (playerTeam != "Blue") // Проверяем, выбрал ли игрок уже синюю команду
        {
            TeamManager.selectedTeam = "Blue";
            playerTeam = "Blue"; // Обновляем переменную
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

    // Метод для генерации случайного ID комнаты
    private string GenerateRandomRoomId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random random = new System.Random();
        char[] stringChars = new char[8]; // Длина ID, например, 8 символов

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }
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

    public void GoToBattle()
    {
        if (SceneManager.GetActiveScene().name != "Battle")
        {
            // Проверяем, что клиент на мастер-сервере
            if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.ConnectedToMasterServer)
            {
                PhotonNetwork.LoadLevel("Battle");
            }
            else
            {
                Debug.Log("Переподключение к MasterServer перед переходом на сцену Battle.");
                PhotonNetwork.ReconnectAndRejoin(); // Переподключаемся к MasterServer
            }
        }
    }
}