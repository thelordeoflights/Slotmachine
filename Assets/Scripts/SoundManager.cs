using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip spinSound, winSound;

    public void PlaySpinSound()
    {
        audioSource.clip = spinSound;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopSpinSound()
    {
        audioSource.Stop();
        audioSource.loop = false;
        // audioSource.clip = stopSound;
    }
    public void PlayWinSound()
    {
        Debug.Log("Playing win sound!");
        audioSource.clip = winSound;
        audioSource.Play();
        // audioSource.loop = false;
    }
    
}
