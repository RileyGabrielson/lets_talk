using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource sfx;

    public void PlaySoundEffect(AudioClip clip, float minPitch, float maxPitch) {
        sfx.pitch = Random.Range(minPitch, maxPitch);
        sfx.PlayOneShot(clip);
    }

    public void PlaySoundEffect(AudioClip clip) {
        sfx.pitch = 1f;
        sfx.PlayOneShot(clip);
    }
}
