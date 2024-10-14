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
        // ��������� ������� �����
        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "Game")
        {
            if (audioSrc1.isPlaying) // ���������, ������ �� ������
            {
                audioSrc1.Stop(); // ������ �� �����
            }
        }
        else
        {
            if (!audioSrc1.isPlaying) // ���������, �� ������ �� ������
            {
                audioSrc1.Play(); // ������������ ���������������
            }
        }
    }
}
