﻿using UnityEngine;

namespace Projectiles
{
	/// <summary>
	/// Hitscan projectile with immediate effect and optional trail path.
	/// </summary>
	public class InstantHitscanProjectile : HitscanProjectile
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private float _visibleTime = 2f;

		[Header("Trail")]
		[SerializeField]
		private LineRenderer _trail;
		[SerializeField]
		private Gradient _fadeoutTrailGradient;
		[SerializeField]
		private float _fadeoutTrailWidthMultiplier = 1f;

		private Gradient _startTrailGradient;
		private float _startTrailWidthMultiplier;

		private Gradient _trailGradient;

		private float _time;

		// HitscanProjectile INTERFACE

		// activates projectile with provided data
		public override void Activate(ref HitscanData data)
		{
			base.Activate(ref data);

			var startPosition = Context.BarrelTransforms[data.BarrelIndex].position;

			transform.position = startPosition;
			transform.rotation = Quaternion.LookRotation(data.FireDirection);

			SpawnImpactVisual(data.ImpactPosition, data.ImpactNormal);

			if (_trail != null)
			{
				_trail.SetPosition(0, startPosition);
				_trail.SetPosition(1, (Vector3)data.ImpactPosition != Vector3.zero ? data.ImpactPosition : startPosition + (Vector3)data.FireDirection * _maxDistance);
			}

			_time = 0f;
		}

        // renders projectile over time, by updating trail and visibility
		public override void Render(ref HitscanData data, ref HitscanData fromData, float alpha)
		{
			base.Render(ref data, ref fromData, alpha);

			_time += Time.deltaTime;

			IsFinished = _time >= _visibleTime;

			if (_trail != null)
			{
				float progress = _time / _visibleTime;

				_trail.colorGradient = LerpGradient(_startTrailGradient, _fadeoutTrailGradient, _trailGradient, progress);
				_trail.widthMultiplier = Mathf.Lerp(_startTrailWidthMultiplier, _fadeoutTrailWidthMultiplier, progress);
			}
		}

		// MONOBEHAVIOUR
		// initialises projectile's settings during startup
		protected override void Awake()
		{
			base.Awake();

			if (_trail != null)
			{
				_startTrailGradient = _trail.colorGradient;
				_startTrailWidthMultiplier = _trail.widthMultiplier;

				_trailGradient = new Gradient();
				_trailGradient.SetKeys(new GradientColorKey[_startTrailGradient.colorKeys.Length], new GradientAlphaKey[_startTrailGradient.alphaKeys.Length]);

				if (_trailGradient.colorKeys.Length != _fadeoutTrailGradient.colorKeys.Length || _trailGradient.alphaKeys.Length != _fadeoutTrailGradient.alphaKeys.Length)
				{
					Debug.LogError($"{gameObject.name} - Trail gradient and fadeout gradient must have identical number of color and alpha keys");
				}
			}
		}

		// PRIVATE METHODS
        // changes between two gradients based on provided value
		private Gradient LerpGradient(Gradient from, Gradient to, Gradient result, float alpha)
		{
			// TODO: This allocates a lot (array copies)
			var fromColorKeys = from.colorKeys;
			var toColorKeys = to.colorKeys;
			var resultColorKeys = result.colorKeys;

			for (int i = 0; i < resultColorKeys.Length; i++)
			{
				GradientColorKey key = default;

				key.color = Color.Lerp(fromColorKeys[i].color, toColorKeys[i].color, alpha);
				key.time = Mathf.Lerp(fromColorKeys[i].time, toColorKeys[i].time, alpha);

				resultColorKeys[i] = key;
			}

			var fromAlphaKeys = from.alphaKeys;
			var toAlphaKeys = to.alphaKeys;
			var resultAlphaKeys = result.alphaKeys;

			for (int i = 0; i < resultAlphaKeys.Length; i++)
			{
				GradientAlphaKey key = default;

				key.alpha = Mathf.Lerp(fromAlphaKeys[i].alpha, toAlphaKeys[i].alpha, alpha);
				key.time = Mathf.Lerp(fromAlphaKeys[i].time, toColorKeys[i].time, alpha);

				resultAlphaKeys[i] = key;
			}

			result.SetKeys(resultColorKeys, resultAlphaKeys);

			return result;
		}
	}
}
