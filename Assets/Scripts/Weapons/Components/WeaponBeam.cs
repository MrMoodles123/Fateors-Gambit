using System;
using Fusion;
using UnityEngine;
using System.Collections;

namespace Projectiles
{
    /// <summary>
    ///  Weapon component that fires a laser beam with attack cooldown to prevent spamming.
    /// </summary>
    [DefaultExecutionOrder(15)]
    public class WeaponBeam : WeaponComponent
    {
        // EXISTING SERIALIZED FIELDS
        [SerializeField]
        private float _damage = 10f;
        [SerializeField]
        private EHitType _hitType = EHitType.Projectile;
        [SerializeField]
        private LayerMask _hitMask;
        [SerializeField]
        private float _maxDistance = 50f;
        [SerializeField]
        private float _beamRadius = 0.2f;
        [SerializeField, Tooltip("Number of raycast rays fired. First is always in center, other are spread around in the radius distance.")]
        private int _raycastAmount = 5;
        [SerializeField]
        private WeaponTrigger _weaponTrigger;

        [Header("Beam Visuals")]
        [SerializeField]
        private GameObject _beamStart;
        [SerializeField]
        private GameObject _beamEnd;
        [SerializeField]
        private LineRenderer _beam;
        [SerializeField]
        private float _beamEndOffset = 0.5f;
        [SerializeField]
        private bool _updateBeamMaterial;
        [SerializeField]
        private float _textureScale = 3f;
        [SerializeField]
        private float _textureScrollSpeed = -8f;

        [Header("Camera Effect")]
        [SerializeField]
        private ShakeSetup _cameraShakePosition;
        [SerializeField]
        private ShakeSetup _cameraShakeRotation;

        [Header("Animation")]
        [SerializeField]
        private Animator _transitionAnimator;
        [SerializeField]
        private float _transitionDuration = 1f; // Duration of the transition animation

        // NEW COOLDOWN FIELDS
        [Header("Attack Cooldown")]
        [SerializeField]
        private float _attackCooldown = 2f; // Time between attacks

        // NEW FIELDS FOR SOUND
        [Header("Sound Effects")]
        [SerializeField]
        private AudioSource _audioSource;  // Attach this in the Inspector
        [SerializeField]
        private AudioClip _attackSound;     // The attack sound clip
        [SerializeField]
        private float _minPitch = 0.8f;     // Minimum pitch range
        [SerializeField]
        private float _maxPitch = 1.2f;     // Maximum pitch range

        // PRIVATE STATE VARIABLES
        private bool _isAttacking = false;
        private float _lastAttackTime = -1f;

        [Networked]
        private float _beamDistance { get; set; }

        [Networked]
        private float LastAttackEndTime { get; set; }

        [Networked]
        private bool IsAnimationPlaying { get; set; }

        public override void Fire()
        {
            // If animation is already playing, do nothing
            if (IsAnimationPlaying)
            {
                return;
            }

            // Check if attack is possible based on cooldown
            if (!CanAttack())
            {
                return;
            }

            var hit = ProcessBeamHit();
            if (hit.Distance > 0f)
            {
                HitUtility.ProcessHit(Object.InputAuthority, FireTransform.forward, hit, _damage, _hitType);

                // Play attack sound only if damage is dealt
                PlayAttackSound();
            }

            // Mark animation as playing
            IsAnimationPlaying = true;

            // Update the last attack end time
            LastAttackEndTime = Runner.SimulationTime + _transitionDuration;

            // Trigger animation
            if (_transitionAnimator != null)
            {
                if (_transitionAnimator.name == "Axes")
                {
                    float randomValue = UnityEngine.Random.value; // Generate random value between 0 and 1
                    if (randomValue <= 0.25f && randomValue >= 0)
                    {
                        _transitionAnimator.SetBool("Attack", true);
                    }
                    else if (randomValue <= 0.50f && randomValue > 0.25f)
                    {
                        _transitionAnimator.SetBool("Attack1", true);
                    }
                    else if (randomValue <= 0.75f && randomValue > 0.5f)
                    {
                        _transitionAnimator.SetBool("Attack2", true);
                    }
                    else
                    {
                        _transitionAnimator.SetBool("Attack3", true);
                    }
                }
                else
                {
                    float randomValue = UnityEngine.Random.value; // Generate random value between 0 and 1
                    if (randomValue < 0.5f)
                    {
                        _transitionAnimator.SetBool("Attack", true);
                    }
                    else
                    {
                        _transitionAnimator.SetBool("Attack1", true);
                    }
                }
                // Start coroutine to reset animation and state
                StartCoroutine(ResetAnimationCoroutine());
            }


        }

        // MODIFIED METHOD TO CHECK IF ATTACK IS ALLOWED
        private bool CanAttack()
        {
            // Check if enough time has passed since the last attack ended
            return Runner.SimulationTime >= LastAttackEndTime + _attackCooldown;
        }

        private IEnumerator ResetAnimationCoroutine()
        {
            // Wait for the transition duration
            yield return new WaitForSeconds(_transitionDuration);

            // Reset the Attack boolean
            if (_transitionAnimator != null)
            {
                if (_transitionAnimator.name == "Axes")
                {
                    _transitionAnimator.SetBool("Attack", false);
                    _transitionAnimator.SetBool("Attack1", false);
                    _transitionAnimator.SetBool("Attack2", false);
                    _transitionAnimator.SetBool("Attack3", false);
                }
                else
                {
                    _transitionAnimator.SetBool("Attack", false);
                    _transitionAnimator.SetBool("Attack1", false);
                }
            }

            // Mark animation as completed
            IsAnimationPlaying = false;
        }

        // NEW METHOD TO PLAY ATTACK SOUND WITH PITCH VARIATION
        private void PlayAttackSound()
        {
            if (_audioSource != null && _attackSound != null)
            {
                _audioSource.clip = _attackSound;
                _audioSource.pitch = UnityEngine.Random.Range(_minPitch, _maxPitch);  // Random pitch
                _audioSource.Play();
            }
        }

        // NetworkBehaviour INTERFACE
        public override void FixedUpdateNetwork()
        {
            // Update beam distance only when trigger is firing
            if (_weaponTrigger.IsBusy == true)
            {
                ProcessBeamHit();
            }
            else
            {
                _beamDistance = -1f;
            }
        }

        private void LateUpdate()
        {
            if (Object == null || Object.IsValid == false)
                return;

            // Beam needs to be updated after camera pivot change
            // - after PlayerAgent.LateUpdate
            UpdateBeam();

            if (_beamDistance > 0f && HasInputAuthority == true)
            {
                var cameraShake = Context.Camera.ShakeEffect;
                cameraShake.Play(_cameraShakePosition, EShakeForce.ReplaceSame);
                cameraShake.Play(_cameraShakeRotation, EShakeForce.ReplaceSame);
            }
        }

        // PRIVATE MEMBERS
        private LagCompensatedHit ProcessBeamHit()
        {
            _beamDistance = _maxDistance;

            if (ProjectileUtility.CircleCast(Runner, Object.InputAuthority, FireTransform.position, FireTransform.forward, _maxDistance, _beamRadius, _raycastAmount, _hitMask, out LagCompensatedHit hit) == true)
            {
                _beamDistance = hit.Distance;
                return hit;
            }

            return default;
        }

        private void UpdateBeam()
        {
            bool beamActive = _beamDistance > 0f;

            if (_beamStart != null) _beamStart.SetActiveSafe(beamActive);
            if (_beamEnd != null) _beamEnd.SetActiveSafe(beamActive);
            if (_beam != null) _beam.gameObject.SetActiveSafe(beamActive);

            if (beamActive == false || _beamStart == null || _beam == null)
                return;

            var startPosition = _beamStart.transform.position;
            var targetPosition = FireTransform.position + FireTransform.forward * _beamDistance;

            var visualDirection = targetPosition - startPosition;
            float visualDistance = visualDirection.magnitude;

            visualDirection /= visualDistance; // Normalize

            if (_beamEndOffset > 0f)
            {
                visualDistance = visualDistance > _beamEndOffset ? visualDistance - _beamEndOffset : 0f;
                targetPosition = startPosition + visualDirection * visualDistance;
            }

            if (_beamEnd != null)
            {
                _beamEnd.transform.SetPositionAndRotation(targetPosition, Quaternion.LookRotation(-visualDirection));
            }

            _beam.SetPosition(0, startPosition);
            _beam.SetPosition(1, targetPosition);

            if (_updateBeamMaterial == true && _beam.material != null)
            {
                var beamMaterial = _beam.material;
                beamMaterial.mainTextureScale = new Vector2(visualDistance / _textureScale, 1f);
                beamMaterial.mainTextureOffset += new Vector2(Time.deltaTime * _textureScrollSpeed, 0f);
            }
        }
    }
}