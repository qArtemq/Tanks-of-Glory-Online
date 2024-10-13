using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class loadingbar : MonoBehaviour
{

    private RectTransform rectComponent;
    private Image imageComp;
    public float desiredTime = 2.0f; // Время в секундах, за которое индикатор заполнится
    private float speed;

    public bool isFilled = false; // Флаг, который указывает на завершение загрузки

    void Start()
    {
        rectComponent = GetComponent<RectTransform>();
        imageComp = rectComponent.GetComponent<Image>();
        imageComp.fillAmount = 0.0f;

        speed = 1f / desiredTime; // Рассчитываем скорость
    }

    void Update()
    {
        if (imageComp.fillAmount < 1f)
        {
            imageComp.fillAmount += Time.deltaTime * speed;
        }
        else
        {
            isFilled = true; // Устанавливаем флаг, когда индикатор полностью заполнен
        }
    }
}
