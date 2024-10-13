using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviourPunCallbacks
{
    public RectTransform rectComponent; // Делаем неизменяемым
    public Image imageComp; // Делаем неизменяемым
    public Canvas canvasSource; // Канвас откуда берется информация
    public float health = 1f;
    private Camera localPlayerCamera; // Камера локального игрока
    private Player player;

    // Конструктор или метод инициализации
    public Health(Canvas canvas)
    {
        // Указываем канвас как источник данных
        canvasSource = canvas;

        // Ищем компоненты внутри канваса
        rectComponent = canvasSource.GetComponentInChildren<RectTransform>();
        imageComp = rectComponent.GetComponentInChildren<Image>();
    }

    void Start()
    {
        imageComp.fillAmount = health;  // Инициализация с текущим значением здоровья
        player = GetComponent<Player>();

        // Поиск главной камеры игрока
        if (Camera.main != null)
        {
            localPlayerCamera = Camera.main;  // Получаем основную камеру
        }
        else
        {
            // Если камера не "Main", находим её в дочерних объектах префаба танка
            localPlayerCamera = FindObjectOfType<Camera>();
        }
    }
    void Update()
    {
        if (localPlayerCamera != null)
        {
            // Поворачиваем панель здоровья к камере локального игрока
            rectComponent.transform.LookAt(localPlayerCamera.transform);
        }
    }
    public void TakeDamage(float damage)
    {
        if (!photonView.IsMine || player.isInvisible) return; // Только хозяин объекта может изменять здоровье

        // Локально обновляем здоровье для владельца
        health -= damage;
        health = Mathf.Clamp(health, 0, 1);
        imageComp.fillAmount = health;

        // Отправляем обновление другим игрокам
        photonView.RPC("UpdateHealthUI", RpcTarget.Others, health); // Обновляем UI для других игроков
    }

    // Метод для обновления UI на всех клиентах, кроме владельца
    [PunRPC]
    public void UpdateHealthUI(float currentHealth)
    {
        health = currentHealth;
        imageComp.fillAmount = health;
    }

}
