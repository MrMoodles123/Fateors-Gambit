namespace Projectiles
{
	public static class AudioEffectExtensions
	{
		// attempt to play a sound effect from an array that holds all audioeffect objects
		public static bool PlaySound(this AudioEffect[] effects, AudioSetup setup, EForceBehaviour force = EForceBehaviour.None)
		{
			if (effects == null)
				return false;

			if (setup.Clips.SafeCount() == 0)
				return false;

			AudioEffect bestPlayingEffect = null;
			float bestTime = 0.5f;

			// loop through array of audios
			for (int i = 0; i < effects.Length; i++)
			{
				var audioEffect = effects[i];

				// if the effect is not playing, play it
				if (audioEffect.IsPlaying == false)
				{
					audioEffect.Play(setup);
					return true;
				}

				bool chooseAudioEffect = false;

				// based on behaviour, choose effect to play
				switch (force)
				{
					case EForceBehaviour.ForceDifferentSetup:
						chooseAudioEffect = audioEffect.AudioSource.time > bestTime && audioEffect.CurrentSetup != setup;
						break;
					case EForceBehaviour.ForceSameSetup:
						chooseAudioEffect = audioEffect.AudioSource.time > bestTime && audioEffect.CurrentSetup == setup;
						break;
					case EForceBehaviour.ForceAny:
						chooseAudioEffect = audioEffect.AudioSource.time > bestTime;
						break;
				}

				if (chooseAudioEffect == true)
				{
					bestPlayingEffect = audioEffect;
					bestTime = audioEffect.AudioSource.time;
				}
			}

			if (force == EForceBehaviour.None)
				return false; // No free audio effect

			// if the best effect is found, play it
			if (bestPlayingEffect != null)
			{
				bestPlayingEffect.Play(setup, force);
				return true;
			}

			return false;
		}
	}
}
