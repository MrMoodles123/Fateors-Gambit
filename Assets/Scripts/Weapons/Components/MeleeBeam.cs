using System.Collections;
using Fusion;
using UnityEngine;

namespace Projectiles
{
    [DefaultExecutionOrder(15)]
    public class MeleeBeam : WeaponComponent
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private float _damage = 10f;
        [SerializeField] private EHitType _hitType = EHitType.Projectile;
        [SerializeField] private LayerMask _hitMask;
        [SerializeField] private float _maxDistance = 50f;
        [SerializeField] private float _beamRadius = 0.2f;
        [SerializeField] private int _raycastAmount = 5;
        [SerializeField] private WeaponTrigger _weaponTrigger;

        [Header("Beam Visuals")]
        [SerializeField] private GameObject _beamStart;
        [SerializeField] private GameObject _beamEnd;
        [SerializeField] private LineRenderer _beam;

        [SerializeField] private float _beamEndOffset = 0.5f;
        [SerializeField] private bool _updateBeamMaterial;
        [SerializeField] private float _textureScale = 3f;
        [SerializeField] private float _textureScrollSpeed = -8f;

        private float _beamDistance;
        private bool _isAttacking = false;

        public override void Fire()
        {
            if (_isAttacking) return; // Prevent attack spamming

            _isAttacking = true;
            _animator.SetBool("Attack", true);

            Debug.Log("[WeaponBeam] Attack started");

            StartCoroutine(WaitForAnimationAndApplyDamage());
        }

        private IEnumerator WaitForAnimationAndApplyDamage()
        {
            if (_animator == null) yield break;

            // Wait for the attack animation to start
            while (!_animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            {
                yield return null;
            }

            Debug.Log("[WeaponBeam] Attack animation started");

            // Wait for animation to reach completion
            while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }

            Debug.Log("[WeaponBeam] Attack animation finished, applying damage...");

            ApplyDamage();

            _animator.SetBool("Attack", false);
            _isAttacking = false;
        }

        private void ApplyDamage()
        {
            if (_weaponTrigger.IsBusy)
            {
                Debug.Log("[WeaponBeam] Weapon trigger is busy, checking hit...");

                var hit = ProcessBeamHit();
                if (hit.Distance > 0f)
                {
                    Debug.Log($"[WeaponBeam] Hit detected at distance: {hit.Distance}, applying {_damage} damage");
                    HitUtility.ProcessHit(Object.InputAuthority, FireTransform.forward, hit, _damage, _hitType);
                }
                else
                {
                    Debug.Log("[WeaponBeam] No valid hit detected!");
                }
            }
            else
            {
                Debug.Log("[WeaponBeam] WeaponTrigger is NOT busy, damage not applied.");
            }
        }

        public override void FixedUpdateNetwork()
        {
            _beamDistance = -1f; // Reset beam distance every frame
        }

        private LagCompensatedHit ProcessBeamHit()
        {
            _beamDistance = _maxDistance;

            if (ProjectileUtility.CircleCast(Runner, Object.InputAuthority, FireTransform.position, FireTransform.forward, _maxDistance, _beamRadius, _raycastAmount, _hitMask, out LagCompensatedHit hit))
            {
                Debug.Log($"[WeaponBeam] Raycast hit at {hit.Distance}");
                _beamDistance = hit.Distance;
                return hit;
            }

            return default;
        }
    }
}
