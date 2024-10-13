using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Collections;

public class Boost : MonoBehaviourPunCallbacks
{
    public Image boostFillImage;  // Полоса заполнения для буста
    public TextMeshProUGUI boostCooldownText; // Текст для отображения времени перезарядки
    private Player player; // Ссылка на Player скрипт
    public Canvas canvas;

    private void Start()
    {
        // Находим Player скрипт только для локального игрока
        if (photonView.IsMine)
        {
            player = GetComponent<Player>();
            canvas.gameObject.SetActive(true);  // Включаем Canvas для локального игрока
        }
        else
        {
            canvas.gameObject.SetActive(false);  // Отключаем Canvas для других игроков
        }
    }

    private void Update()
    {
        // Обновляем UI только для локального игрока
        if (photonView.IsMine && player != null)
        {
            UpdateBoostUI();
        }
    }

    private void UpdateBoostUI()
    {
        if (player.isBoosting)
        {
            // Обновляем заполнение полосы в зависимости от времени буста
            boostFillImage.fillAmount = player.boostCooldownTimer / player.boostDuration;
            boostCooldownText.text = Mathf.Ceil(player.boostCooldownTimer).ToString(); // Обновляем текст
            boostCooldownText.gameObject.SetActive(true);  // Включаем текст
            boostFillImage.gameObject.SetActive(true); // Включаем полосу
        }
        else if (player.boostCooldownTimer > 0)
        {
            // Если буст на перезарядке, показываем оставшееся время до конца
            boostFillImage.fillAmount = player.boostCooldownTimer / player.boostCooldown;
            boostCooldownText.text = Mathf.Ceil(player.boostCooldownTimer).ToString(); // Обновляем текст
            boostCooldownText.gameObject.SetActive(true);  // Включаем текст
            boostFillImage.gameObject.SetActive(true); // Включаем полосу
        }
        else
        {
            // Если буст доступен и таймер на 0, отключаем UI
            boostFillImage.fillAmount = 0f;
            boostCooldownText.gameObject.SetActive(false);  // Отключаем текст
            boostFillImage.gameObject.SetActive(false); // Отключаем полосу
        }
    }
}
