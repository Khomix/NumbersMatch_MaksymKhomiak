using UnityEngine;
using Unity.Cinemachine;

public class FeedbackManager : MonoBehaviour
{
    [SerializeField] private FeedbackSettings _settings;
    
    private AudioSource _audioSource;
    private CinemachineImpulseSource _impulseSource;

    public void Initialize(CinemachineImpulseSource impulseSource)
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _impulseSource = impulseSource;
    }

    public void Play(FeedbackType type)
    {
        if (_settings == null) return;

        if (_settings.TryGetFeedback(type, out FeedbackData data))
        {
            if (data.AudioClips != null && _audioSource != null)
            {
                _audioSource.pitch = Random.Range(0.9f, 1.1f);
                _audioSource.PlayOneShot(data.AudioClips);
            }

            if (data.UseCameraShake && _impulseSource != null)
            {
                _impulseSource.GenerateImpulse(data.ShakeAmplitude);
            }
        }
    }
}