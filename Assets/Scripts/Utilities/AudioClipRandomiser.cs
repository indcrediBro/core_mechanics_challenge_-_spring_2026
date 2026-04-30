using IncredibleAttributes;
using UnityEngine;

public class AudioClipRandomiser : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private AudioSource audioSource;
    [SerializeField, MinMaxSlider(.5f,3f)] private Vector2 minMaxPitch;

    public void SetRandomPitch()
    {
        audioSource.pitch = Random.Range(minMaxPitch.x, minMaxPitch.y);
    }

    public void SetRandomAudioClip()
    {
        audioSource.clip = clips[Random.Range(0, clips.Length)];
    }
}
