using UnityEngine;
using Fusion;

namespace Projectiles
{
    /// <summary>
    /// Handles visual and audio effects for melee weapons.
    /// </summary>
    public class MeleeWeaponEffects : WeaponComponent
    {
        // PRIVATE MEMBERS

        [SerializeField]
        private Animator _weaponAnimator;

        [SerializeField]
        private string _attackTriggerName = "Attack";

        [SerializeField]
        private string _comboCounterParam = "ComboCounter";

        [SerializeField]
        private AudioClip[] _swingSounds;

        [SerializeField]
        private AudioClip[] _hitSounds;

        [SerializeField]
        private ParticleSystem _swingEffect;

        [SerializeField]
        private AudioSource _audioSource;

        private MeleeWeaponTrigger _meleeTrigger;

        // WeaponComponent INTERFACE

        public override void FireRender()
        {
            // Play animation
            if (_weaponAnimator != null)
            {
                // Get combo counter if available
                if (_meleeTrigger != null && _weaponAnimator.HasParameter(_comboCounterParam))
                {
                    // We need to use reflection to access the networked property
                    var counterField = _meleeTrigger.GetType().GetProperty("_comboCounter",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

                    if (counterField != null)
                    {
                        int comboCounter = (int)counterField.GetValue(_meleeTrigger);
                        _weaponAnimator.SetInteger(_comboCounterParam, comboCounter);
                    }
                }

                _weaponAnimator.SetTrigger(_attackTriggerName);
            }

            // Play swing effect
            if (_swingEffect != null)
            {
                _swingEffect.Play();
            }

            // Play swing sound
            if (_audioSource != null && _swingSounds != null && _swingSounds.Length > 0)
            {
                int soundIndex = Random.Range(0, _swingSounds.Length);
                _audioSource.PlayOneShot(_swingSounds[soundIndex]);
            }
        }

        // MONOBEHAVIOUR

        protected void Awake()
        {
            _meleeTrigger = GetComponentInParent<MeleeWeaponTrigger>();

            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
        }

        // PUBLIC METHODS

        /// <summary>
        /// Called by animation events when a hit connects
        /// </summary>
        public void OnHitConnect()
        {
            if (_audioSource != null && _hitSounds != null && _hitSounds.Length > 0)
            {
                int soundIndex = Random.Range(0, _hitSounds.Length);
                _audioSource.PlayOneShot(_hitSounds[soundIndex]);
            }
        }
    }

    public static class AnimatorExtensions
    {
        public static bool HasParameter(this Animator animator, string paramName)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}