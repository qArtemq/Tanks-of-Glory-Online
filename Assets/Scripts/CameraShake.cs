using Cinemachine;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class CameraShake : MonoBehaviourPunCallbacks
{
    public static CameraShake Instance { get; private set; }

    private CinemachineVirtualCamera _virtualCamera;
    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;

    Player player;

    private void Awake()
    {
        Instance = this;
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
        player = FindObjectOfType<Player>();

        player.isDestroyed = false;
    }

    private void Update()
    {
        if (shakeTimer >= 0)
        {
            shakeTimer -= Time.deltaTime;
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain =
                Mathf.Lerp(startingIntensity, 0f, (1 - (shakeTimer / shakeTimerTotal)));
        }
    }
    public void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
            _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;

        startingIntensity = intensity;
        shakeTimerTotal = time;
        shakeTimer = time;
    }

    public void UpdateCamera(Transform target)
    {
        _virtualCamera.Follow = target;
        _virtualCamera.LookAt = target;

        // —глаживание движени€ камеры
        _virtualCamera.transform.position = Vector3.Lerp(
            _virtualCamera.transform.position,
            target.position,
            Time.deltaTime * 2f);
        _virtualCamera.transform.rotation = Quaternion.Lerp(
            _virtualCamera.transform.rotation,
            target.rotation,
            Time.deltaTime * 2f);
    }
}