using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sound : MonoBehaviour
{
    public GameObject soundSpeaker;
    private AudioSource audioSrc1;
    public GameObject[] objs11;
    void Awake()
    {
        objs11 = GameObject.FindGameObjectsWithTag("Sound");
        if (objs11.Length == 0)
        {
            soundSpeaker = Instantiate(soundSpeaker);
            soundSpeaker.name = "SoundSpeaker";
            DontDestroyOnLoad(soundSpeaker.gameObject);
        }
        else
        {
            soundSpeaker = GameObject.Find("SoundSpeaker");
        }
    }
    void Start()
    {
        audioSrc1 = soundSpeaker.GetComponent<AudioSource>();
    }
    void Update()
    {
        // Проверяем текущую сцену
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "Game")
        {
            if (audioSrc1.isPlaying) // Проверяем, играет ли музыка
            {
                audioSrc1.Stop(); // Ставим на паузу
            }
        }
        else
        {
            if (!audioSrc1.isPlaying) // Проверяем, не играет ли музыка
            {
                audioSrc1.Play(); // Возобновляем воспроизведение
            }
        }
    }
}
