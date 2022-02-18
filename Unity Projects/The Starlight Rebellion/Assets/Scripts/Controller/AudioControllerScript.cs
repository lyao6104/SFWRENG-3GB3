using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MusicSettings
{
	public AudioClip menuMusic;
	public List<AudioClip> playlist;
}

public class AudioControllerScript : MonoBehaviour
{
	public MusicSettings musicSettings;

	private AudioSource audioSource;

	public float maxVolume = 0.5f;

	private void Start()
	{
		DontDestroyOnLoad(this);

		audioSource = GetComponent<AudioSource>();
		SceneManager.activeSceneChanged += OnSceneChange;

		audioSource.clip = musicSettings.menuMusic;
		audioSource.volume = maxVolume;
		audioSource.Play();
	}

	private void Update()
	{
		if (!audioSource.isPlaying)
		{
			PlayNextTrack();
		}
	}

	private void OnSceneChange(Scene current, Scene next)
	{
		if (next.name == "MenuScene")
		{
			StartCoroutine(FadeToTrack(musicSettings.menuMusic, 3));
		}
		else
		{
			PlayNextTrack();
		}
	}

	private void PlayNextTrack()
	{
		int i = 0;
		do
		{
			i = Random.Range(0, musicSettings.playlist.Count);
		} while (musicSettings.playlist[i] == audioSource.clip);

		StartCoroutine(FadeToTrack(musicSettings.playlist[i], 3));
	}

	private IEnumerator FadeToTrack(AudioClip nextTrack, float duration)
	{
		float t = 1;

		// Fade out and stop.
		while (t > 0)
		{
			audioSource.volume = Mathf.Clamp01(Mathf.Lerp(0, maxVolume, t));
			t -= Time.deltaTime / duration;
			yield return new WaitForEndOfFrame();
		}
		audioSource.Stop();

		// Swap to next track and fade in.
		audioSource.clip = nextTrack;
		audioSource.Play();
		while (t < 1)
		{
			audioSource.volume = Mathf.Clamp01(Mathf.Lerp(0, maxVolume, t));
			t += Time.deltaTime / duration;
			yield return new WaitForEndOfFrame();
		}
	}
}
