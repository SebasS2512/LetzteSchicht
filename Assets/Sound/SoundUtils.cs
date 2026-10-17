using UnityEngine;

public static class SoundUtils
{
    public static void Play(AudioSource source, AudioClip clip, float pitchVariation = 0.06f)
    {
        if (source == null || clip == null) return;

        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

        source.PlayOneShot(clip, GameSettings.SfxVolume);
    }

    public static void PlayExclusive(AudioSource source, AudioClip clip, float pitchVariation = 0.06f)
    {
        if (source == null || clip == null) return;

        source.Stop();

        source.clip = clip;
        source.loop = false;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

        source.volume = GameSettings.SfxVolume;

        source.Play();
    }

    public static void PlayRandom(AudioSource source, AudioClip[] clips, float pitchVariation = 0.08f)
    {
        if (source == null || clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        Play(source, clip, pitchVariation);
    }

    public static void PlayAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, volume * GameSettings.SfxVolume);
    }

    public static void PlayLoop(AudioSource source, float baseVolume = 1f)
    {
        if (source == null) return;

        source.volume = baseVolume * GameSettings.SfxVolume;
        if (!source.isPlaying) source.Play();
    }
}