using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MusicSettings
{
	public AudioClip menuMusic;
	public List<AudioClip> regularPlaylist;
	public List<AudioClip> combatPlaylist;
}

public class AudioControllerScript : MonoBehaviour
{
	public MusicSettings musicSettings;

	private AudioSource audioSource;
	private GameControllerScript gc;

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
			// Non-menu scenes should always have a game controller.
			gc = GameControllerScript.GetController();

			PlayNextTrack();
		}
	}

	public void PlayNextTrack()
	{
		AudioClip clip;
		float fadeDuration = 3;
		if (gc.IsInCombat())
		{
			int i;
			do
			{
				i = Random.Range(0, musicSettings.combatPlaylist.Count);
			} while (musicSettings.combatPlaylist[i] == audioSource.clip);
			clip = musicSettings.combatPlaylist[i];
			fadeDuration = 5;
		}
		else
		{
			int i;
			do
			{
				i = Random.Range(0, musicSettings.regularPlaylist.Count);
			} while (musicSettings.regularPlaylist[i] == audioSource.clip);
			clip = musicSettings.regularPlaylist[i];
		}

		StartCoroutine(FadeToTrack(clip, fadeDuration));
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
