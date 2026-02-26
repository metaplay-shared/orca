using System;
using System.Collections.Generic;
using Code.UI.Effects.Signals;
using Code.UI.HudBase;
using Code.UI.Island.Signals;
using Code.UI.Map.Signals;
using Code.UI.Merge.AddOns.MergeBoard.LockArea;
using Code.UI.MergeBase.Signals;
using Code.UI.Rewarding;
using Code.UI.Tutorial.TriggerActions;
using Code.UI.Utils;
using DG.Tweening;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace Code.UI.Sound {
	public class SoundPlayer : MonoBehaviour {
		[SerializeField] private SoundSettings SoundSettings;
		[SerializeField] private AudioSource IslandMusicSource;
		[SerializeField] private AudioSource MapMusicSource;
		[SerializeField] private AudioMixer AudioMixer;
		[SerializeField] private AudioMixerGroup SoundMixerGroup;
		[SerializeField] private AudioMixerGroup MergeMixerGroup;
		[SerializeField] private AudioMixerGroup MusicMixerGroup;
		[SerializeField] private AudioMixerGroup FlightHitMixerGroup;

		[Inject] private SignalBus signalBus;

		private readonly Dictionary<AudioClip, DateTime> soundPlayTimes = new();
		private Tween mapMusicSourceStopTween;

		private void Start() {
			// Effects
			// signalBus.Subscribe<ItemSelectedSignal>(() => PlaySound(SoundSettings.ItemSelectionSound));
			// signalBus.Subscribe<ItemMergedSignal>((signal) => PlayMergeSound(signal, SoundSettings.ItemMergeSound));
			// signalBus.Subscribe<ItemCreatedSignal>(
			// 	signal => {
			// 		if (signal.Spawned) {
			// 			PlaySound(SoundSettings.ItemSpawnSound);
			// 		}
			// 	}
			// );
			// signalBus.Subscribe<RewardShownSignal>(() => PlaySound(SoundSettings.RewardCelebration));
			// signalBus.Subscribe<ParticleDestroyedSignal>(() => PlaySound(SoundSettings.FlightHit, FlightHitMixerGroup));
			// signalBus.Subscribe<ItemFlightCompletedSignal>(() => PlaySound(SoundSettings.InventoryItemFlightHit));
			// signalBus.Subscribe<LockAreaOpenedSignal>(() => PlaySound(SoundSettings.MergeBoardCloudOpened));
			// signalBus.Subscribe<ButtonClickedSignal>(() => PlaySound(SoundSettings.ButtonClicked));
			// signalBus.Subscribe<EnteredIslandSignal>(() => PlaySound(SoundSettings.EnterIsland));
			// signalBus.Subscribe<DialogueOpenSignal>(() => PlaySound(SoundSettings.DialogueOpen));
			//
			// // Music
			// signalBus.Subscribe<EnteredMapSignal>(() => PlayMapMusic(SoundSettings.MapMusic));
			// signalBus.Subscribe<EnteredIslandSignal>(() => PlayIslandMusic(SoundSettings.IslandMusic));
			// PlayIslandMusic(SoundSettings.IslandMusic);
		}

		private void PlayMergeSound(ItemMergedSignal itemMergedSignal, AudioClip soundToPlay) {
			if (!MetaplayClient.PlayerModel.PrivateProfile.SoundSettings.SoundEnabled) {
				return;
			}

			if (soundToPlay == null) {
				return;
			}

			// Prevent sound stacking
			if (soundPlayTimes.ContainsKey(soundToPlay) &&
				(DateTime.Now - soundPlayTimes[soundToPlay]).TotalSeconds < 0.05f) {
				return;
			}

			soundPlayTimes[soundToPlay] = DateTime.Now;

			int itemLevel = itemMergedSignal.Item.Info.Level;

			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.clip = soundToPlay;
			audioSource.Play();
			audioSource.outputAudioMixerGroup = MergeMixerGroup;
			var mixer = audioSource.outputAudioMixerGroup.audioMixer;
			mixer.SetFloat("Pitch", (float)Math.Pow(1.05946, itemLevel - 1));

			Destroy(audioSource, soundToPlay.length);
		}

		private void PlaySound(AudioClip soundToPlay, AudioMixerGroup audioMixerGroup = null) {
			if (!MetaplayClient.PlayerModel.PrivateProfile.SoundSettings.SoundEnabled) {
				return;
			}

			if (soundToPlay == null) {
				return;
			}

			// Prevent sound stacking
			if (soundPlayTimes.ContainsKey(soundToPlay) &&
				(DateTime.Now - soundPlayTimes[soundToPlay]).TotalSeconds < 0.05f) {
				return;
			}

			soundPlayTimes[soundToPlay] = DateTime.Now;

			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.clip = soundToPlay;
			audioSource.Play();
			audioSource.outputAudioMixerGroup = audioMixerGroup != null ? audioMixerGroup : SoundMixerGroup;

			Destroy(audioSource, soundToPlay.length);
		}

		private void PlayMapMusic(AudioClip musicToPlay) {
			if (musicToPlay == null) {
				return;
			}

			IslandMusicSource.DOFade(0.3f, 1f);
			mapMusicSourceStopTween?.Kill();
			MapMusicSource.DOFade(1, 1);

			MapMusicSource.clip = musicToPlay;
			MapMusicSource.loop = true;
			MapMusicSource.outputAudioMixerGroup = MusicMixerGroup;
			if (!MapMusicSource.isPlaying) {
				MapMusicSource.Play();
			}
		}

		private void PlayIslandMusic(AudioClip musicToPlay) {
			if (musicToPlay == null) {
				return;
			}

			IslandMusicSource.DOFade(1, 1);
			mapMusicSourceStopTween = MapMusicSource.DOFade(0, 1).OnComplete(() => MapMusicSource.Stop());

			IslandMusicSource.clip = musicToPlay;
			IslandMusicSource.loop = true;
			IslandMusicSource.outputAudioMixerGroup = MusicMixerGroup;
			if (!IslandMusicSource.isPlaying) {
				IslandMusicSource.Play();
			}
		}

		private void Update() {
			IslandMusicSource.mute = !MetaplayClient.PlayerModel.PrivateProfile.SoundSettings.MusicEnabled;
			MapMusicSource.mute = !MetaplayClient.PlayerModel.PrivateProfile.SoundSettings.MusicEnabled;
		}
	}
}
