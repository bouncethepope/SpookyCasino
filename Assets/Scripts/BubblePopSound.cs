using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BubblePopSound : MonoBehaviour
{
    [SerializeField] private AudioClip popSound;
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void StartPop()
    {
        if (popSound == null || audioSource == null)
        {
            return;
        }

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(popSound);
    }
}