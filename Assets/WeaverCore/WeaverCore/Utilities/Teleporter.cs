﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Assets;
using WeaverCore.Assets.Components;

namespace WeaverCore.Utilities
{
	public static class Teleporter
	{
		static ObjectPool WhiteFlashPool;
		static ObjectPool GlowPool;
		static ObjectPool TeleLinePool;


		[Serializable]
		public class TeleportException : Exception
		{
			public TeleportException() { }
			public TeleportException(string message) : base(message) { }
			public TeleportException(string message, Exception inner) : base(message, inner) { }
			protected TeleportException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}


		const float WARP_TIME = 20f / 60f;

		public enum TeleType
		{
			Quick,
			Delayed
		}

		static void LookAt(GameObject source, Vector3 destination)
		{
			source.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(destination.y - source.transform.position.y, destination.x - source.transform.position.x) * Mathf.Rad2Deg);
		}


		/// <summary>
		/// Teleports an entity to a specified destination
		/// </summary>
		/// <param name="entity">The entity to teleport</param>
		/// <param name="Destination">The destination of the entity</param>
		/// <param name="teleType">How fast the teleportation should take</param>
		/// <param name="teleportColor">The color of the teleportation effects</param>
		/// <param name="flashSprite">Whether the sprite on the entity should flash or not. This only works if the entity has a <see cref="SpriteRenderer"/> and a <see cref="WeaverCore.Components.SpriteFlasher"/> If a <see cref="WeaverCore.Components.SpriteFlasher"/> is not already on the entity, one will be created</param>
		/// <param name="playEffects">Whether the teleportation effects should be played.</param>
		/// <returns>Returns the amount of time the teleportation will take. You can use this if you want to wait until the teleportation is done</returns>
		public static float TeleportEntity(GameObject entity, Vector3 Destination, TeleType teleType = TeleType.Quick, Color teleportColor = default(Color), bool flashSprite = true, bool playEffects = true, float audioPitch = 1f)
		{
			float inTime = 0.0f;
			float outTime = 0.0f;

			switch (teleType)
			{
				case TeleType.Quick:
					inTime = 0.0f;
					outTime = 0.1f;
					break;
				case TeleType.Delayed:
					inTime = 0.05f;
					outTime = 0.1f;
					break;
				default:
					break;
			}
			return TeleportEntity(entity, Destination, inTime, outTime, teleportColor, flashSprite, playEffects,audioPitch);
		}

		/// <summary>
		/// Teleports an entity to a specified destination
		/// </summary>
		/// <param name="entity">The entity to teleport</param>
		/// <param name="Destination">The destination of the entity</param>
		/// <param name="teleInTime">How long the entity will wait before it will teleport</param>
		/// <param name="teleOutTime">How long the entity will wait after it has teleported</param>
		/// <param name="teleportColor">The color of the teleportation effects. If left at the default, the teleport color will be white</param>
		/// <param name="flashSprite">Whether the sprite on the entity should flash or not. This only works if the entity has a <see cref="SpriteRenderer"/> and a <see cref="WeaverCore.Components.SpriteFlasher"/> If a <see cref="WeaverCore.Components.SpriteFlasher"/> is not already on the entity, one will be created</param>
		/// <param name="playEffects">Whether the teleportation effects should be played.</param>
		/// <returns>Returns the amount of time the teleportation will take. You can use this if you want to wait until the teleportation is done</returns>
		public static float TeleportEntity(GameObject entity, Vector3 Destination, float teleInTime, float teleOutTime, Color teleportColor = default(Color), bool flashSprite = true, bool playEffects = true, float audioPitch = 1f)
		{
			if (teleportColor == default(Color))
			{
				teleportColor = Color.white;
			}

			if (teleInTime < 0f)
			{
				teleInTime = 0f;
			}

			if (teleOutTime < 0f)
			{
				teleOutTime = 0f;
			}

			var sprite = entity.GetComponent<SpriteRenderer>();
			var flasher = entity.GetComponent<SpriteFlasher>();

			if (flashSprite)
			{
				if (sprite == null)
				{
					throw new TeleportException("The entity to be teleported does not have a SpriteRenderer. Either add a sprite renderer to the entity, or set flashSprite to false");
				}
				if (flasher == null)
				{
					flasher = entity.AddComponent<SpriteFlasher>();
				}
			}

			float storedAlpha = sprite.color.a;

			UnboundCoroutine.Start(TeleportRoutine(entity, Destination, teleInTime, teleOutTime, teleportColor, flashSprite, playEffects, audioPitch, sprite, flasher, storedAlpha));

			return teleInTime + teleOutTime;
		}

		private static IEnumerator TeleportRoutine(GameObject entity, Vector3 Destination, float teleInTime, float teleOutTime, Color teleportColor, bool flashSprite, bool playEffects, float audioPitch, SpriteRenderer sprite, SpriteFlasher flasher, float storedAlpha)
		{
			if (teleInTime == 0f && teleOutTime == 0f)
			{
				var originalPosition = entity.transform.position;
				entity.transform.position = Destination;

				if (playEffects)
				{
					SpawnTeleportGlow(Destination, teleportColor);

					SpawnTeleportLine(originalPosition, Destination, teleportColor);

					SpawnWhiteFlash(teleportColor, originalPosition);
				}

				PlayTeleportSound(Destination, audioPitch);
			}
			else
			{
				if (flashSprite)
				{
					flasher.DoFlash(teleInTime, 0.0f, 0.8f, teleportColor, 10f);
				}
				WhiteFlash whiteFlash = null;

				if (playEffects)
				{
					whiteFlash = SpawnWhiteFlash(teleportColor, entity.transform.position);
					whiteFlash.transform.parent = entity.transform;
				}
				if (teleInTime > 0f)
				{
					//yield return new Awaiters.WaitForSeconds(teleInTime);
					yield return new WaitForSeconds(teleInTime);
				}
				if (playEffects)
				{
					whiteFlash.transform.parent = null;
					whiteFlash.transform.position = entity.transform.position;
				}
				if (teleOutTime == 0f)
				{
					entity.transform.position = Destination;
					if (flashSprite)
					{
						flasher.StopFlashing();
						flasher.FlashIntensity = 0f;
					}

					var originalPosition = entity.transform.position;

					if (playEffects)
					{
						SpawnTeleportGlow(Destination, teleportColor);

						SpawnTeleportLine(originalPosition, Destination, teleportColor);
					}

					PlayTeleportSound(Destination, audioPitch);
					//var teleportSound = HollowPlayer.Play(AudioAssets.Teleport, Destination, 1f, AudioChannel.Sound);

					//teleportSound.AudioSource.pitch = audioPitch;
				}
				else
				{
					if (flashSprite)
					{
						flasher.StopFlashing();
						flasher.FlashIntensity = 0.8f;
						flasher.FlashColor = teleportColor;
					}

					float currentWarpTime = WARP_TIME;

					if (teleOutTime < WARP_TIME)
					{
						currentWarpTime = teleOutTime;
					}

					float fadeOutTime = currentWarpTime / 2f;

					float fadeOutTimer = 0f;

					var originalPosition = entity.transform.position;

					if (playEffects)
					{
						SpawnTeleportGlow(Destination, teleportColor);
						/*var glow = GameObject.Instantiate(EffectAssets.TeleportGlowPrefab, new Vector3(Destination.x, Destination.y, Destination.z - 0.1f), Quaternion.identity);
						glow.GetComponent<SpriteRenderer>().color = Color.Lerp(teleportColor,Color.white,0.5f);*/

						SpawnTeleportLine(originalPosition, Destination, teleportColor);
						/*var teleLine = GameObject.Instantiate(EffectAssets.TeleLinePrefab, Vector3.Lerp(originalPosition, Destination, 0.5f), Quaternion.identity);
						LookAt(teleLine, Destination);
						teleLine.transform.localScale = new Vector3(Vector3.Distance(originalPosition, Destination), teleLine.transform.localScale.y, teleLine.transform.localScale.z);*/

						//var mainModule = teleLine.GetComponent<ParticleSystem>().main;
						//mainModule.startColor = Color.Lerp(teleportColor, Color.white, 0.5f);
					}
					PlayTeleportSound(Destination, audioPitch);
					//var teleportSound = HollowPlayer.Play(AudioAssets.Teleport, Destination, 1f, AudioChannel.Sound);
					//teleportSound.AudioSource.pitch = audioPitch;

					while (fadeOutTimer < fadeOutTime)
					{
						yield return null;
						fadeOutTimer += Time.deltaTime;
						if (fadeOutTimer > fadeOutTime)
						{
							fadeOutTimer = fadeOutTime;
						}
						if (playEffects)
						{
							sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, Mathf.Lerp(storedAlpha, 0f, fadeOutTimer / fadeOutTime));
						}
					}

					entity.transform.position = Destination;

					float fadeInTime = currentWarpTime - fadeOutTime;

					float fadeInTimer = 0f;

					if (playEffects)
					{
						flasher.DoFlash(0.0f, teleOutTime, 0.8f, teleportColor, 0f);
					}

					while (fadeInTimer < fadeInTime)
					{
						yield return null;
						fadeInTimer += Time.deltaTime;
						if (fadeInTimer > fadeInTime)
						{
							fadeInTimer = fadeInTime;
						}
						if (playEffects)
						{
							sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, Mathf.Lerp(0f, storedAlpha, fadeInTimer / fadeInTime));
						}
					}
				}
			}
		}

		private static void PlayTeleportSound(Vector3 position, float audioPitch)
		{
			var teleportSound = WeaverAudio.PlayAtPoint(AudioAssets.Teleport, position, 1f, AudioChannel.Sound);

			teleportSound.AudioSource.pitch = audioPitch;
		}

		static WhiteFlash SpawnWhiteFlash(Color color, Vector3 originalPosition)
		{
			if (WhiteFlashPool == null)
			{
				WhiteFlashPool = new ObjectPool(EffectAssets.WhiteFlashPrefab,PoolLoadType.Local);
			}
			var whiteFlash = WhiteFlashPool.Instantiate<WhiteFlash>(originalPosition, Quaternion.identity);
			//var whiteFlash = GameObject.Instantiate(EffectAssets.WhiteFlashPrefab, originalPosition, Quaternion.identity).GetComponent<WhiteFlash>();
			whiteFlash.FadeInTime = 0f;
			whiteFlash.FlashColor = Color.Lerp(color, Color.white, 0.5f);
			whiteFlash.transform.localScale = Vector3.one * 2f;

			return whiteFlash;
		}

		static SpriteRenderer SpawnTeleportGlow(Vector3 spawnPoint, Color color)
		{
			if (GlowPool == null)
			{
				GlowPool = new ObjectPool(EffectAssets.TeleportGlowPrefab, PoolLoadType.Local);
			}
			var sprite = GlowPool.Instantiate<SpriteRenderer>(new Vector3(spawnPoint.x, spawnPoint.y, spawnPoint.z - 0.1f), Quaternion.identity);
			//var glow = GameObject.Instantiate(EffectAssets.TeleportGlowPrefab, new Vector3(spawnPoint.x, spawnPoint.y, spawnPoint.z - 0.1f), Quaternion.identity);
			//var sprite = glow.GetComponent<SpriteRenderer>();
			sprite.color = Color.Lerp(color, Color.white, 0.5f);
			return sprite;
		}

		static ParticleSystem SpawnTeleportLine(Vector3 originalPosition, Vector3 destination, Color color)
		{
			if (TeleLinePool == null)
			{
				TeleLinePool = new ObjectPool(EffectAssets.TeleLinePrefab, PoolLoadType.Local);
			}
			//var teleLine = GameObject.Instantiate(EffectAssets.TeleLinePrefab, Vector3.Lerp(originalPosition, destination, 0.5f), Quaternion.identity);
			var teleLine = TeleLinePool.Instantiate(Vector3.Lerp(originalPosition, destination, 0.5f), Quaternion.identity);
			LookAt(teleLine, destination);
			teleLine.transform.localScale = new Vector3(Vector3.Distance(originalPosition, destination), teleLine.transform.localScale.y, teleLine.transform.localScale.z);

			var particleSystem = teleLine.GetComponent<ParticleSystem>();

			var mainModule = particleSystem.main;
			mainModule.startColor = Color.Lerp(color, Color.white, 0.5f);

			return particleSystem;
		}
	}
}
