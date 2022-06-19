using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script helps you test out train horns to make sure they are clipped correctly.
/// NOTE: Your horns will sound different in-game due to how audio is processed and the various filters applied within Derail Valley. But the overall sound should sound the same.
/// </summary>
public class CustomHornTester : MonoBehaviour
{

	public AudioClip hornStartClip;
	public AudioClip hornLoopClip;
	private AudioSource _startSource, _loopSource;

	/// <summary>
	/// The button used to test the horn.
	/// </summary>
	public KeyCode hornLoopButton = KeyCode.H;

	/// <summary>
	/// Simulated input value from DV.
	/// </summary>
	private float input = 0.0f;

	/// <summary>
	/// Internal value.
	/// </summary>
	private float inputInternal = 0.0f;

	private bool hitPlaying = false;
	private bool hitPlayed = false;

	private Coroutine currentCoroutine;

	void OnEnable()
	{

		if (hornStartClip == null || hornLoopClip == null)
		{
			enabled = false;
			Debug.LogError("HornStartClip or HornLoopClip is null, disabling.");
			return;
		}

		_startSource = gameObject.AddComponent<AudioSource>();
		_startSource.clip = hornStartClip;

		_startSource.volume = 0f;
		_startSource.loop = false;

		_loopSource = gameObject.AddComponent<AudioSource>();
		_loopSource.clip = hornLoopClip;


		_loopSource.volume = 0f;
		_loopSource.loop = true;
		_loopSource.Play();

		input = 0.0f;
		inputInternal = 0.0f;
		hitPlaying = false;
		hitPlayed = false;
	}

	void Update()
	{

		#region Simulate Horn Lever Input

		inputInternal = Mathf.Lerp(inputInternal, Input.GetKey(hornLoopButton) ? 1 : 0, Time.deltaTime * 2f);

		input = inputInternal;

		if (inputInternal < 0.25f)
		{
			//If the horn is looping right now, we need to either fade it out or stop it altogether based on the current volume.
			if (_loopSource.isPlaying)
			{

				//Continue fading out if the volume is above 0.1f
				if (_loopSource.volume > 0.1f)
				{
					_loopSource.volume = Mathf.Lerp(_loopSource.volume, 0, Time.deltaTime * 2);
				}
				//Stop the loop altogether of its below.
				else
				{
					//If the hit is playing but we lose input, stop the coroutine.
					if (currentCoroutine != null)
					{
						StopCoroutine(currentCoroutine);
						currentCoroutine = null;
					}

					_startSource.volume = 0f;
					_startSource.Stop();

					_loopSource.volume = 0f;
					_loopSource.Stop();
				}
			}

			hitPlaying = false;
			hitPlayed = false;
		}

		#endregion

		if (!hitPlayed && !hitPlaying && input >= 0.25f)
		{
			if (currentCoroutine != null)
				StopCoroutine(currentCoroutine);

			currentCoroutine = StartCoroutine(PlayHornStartThenLoop());
		}
	}

	private IEnumerator PlayHornStartThenLoop()
	{
		hitPlaying = true;
		hitPlayed = false;

		//Get the horn exact duration
		double startDuration = (double) hornStartClip.samples / hornStartClip.frequency;
		double startTime = AudioSettings.dspTime + 0.1;
		
		_loopSource.Stop();

		_startSource.volume = 1f;
		_loopSource.volume = 1f;
		
		_startSource.PlayScheduled(startTime);
		_loopSource.PlayScheduled(startTime + startDuration);

		yield return new WaitForSeconds((float) startTime);
		
		hitPlaying = false;
		hitPlayed = true;

		_startSource.volume = 0f;
		_startSource.Stop();

		currentCoroutine = null;
	}
}