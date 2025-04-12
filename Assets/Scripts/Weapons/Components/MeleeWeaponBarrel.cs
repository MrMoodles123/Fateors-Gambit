using UnityEngine;
using System.Collections.Generic;
using Fusion;

namespace Projectiles
{
    /// <summary>
    /// Weapon component responsible for performing melee attacks.
    /// </summary>
    public class MeleeWeaponBarrel : WeaponComponent
    {
        // PRIVATE MEMBERS

        [SerializeField]
        private float _damage = 25f;

        [SerializeField]
        private float _range = 2f;

        [SerializeField]
        private float _angle = 60f;

        [SerializeField]
        private LayerMask _hitMask;

        [SerializeField]
        private bool _showDebugRays = false;

        [SerializeField]
        private Transform _meleeOrigin;

        // WeaponComponent INTERFACE

        public override void Fire()
        {
            // We'll perform a cone-shaped melee attack
            Vector3 origin = _meleeOrigin != null ? _meleeOrigin.position : FireTransform.position;
            Vector3 forward = FireTransform.forward;

            // Use a Physics.SphereCast for more accurate hit detection
            RaycastHit[] hits = Physics.SphereCastAll(origin, _range * 0.5f, forward, _range, _hitMask);

            // Process each hit
            List<Collider> processedColliders = new List<Collider>();

            foreach (var hit in hits)
            {
                // Check if we've already processed this collider
                if (processedColliders.Contains(hit.collider))
                    continue;

                processedColliders.Add(hit.collider);

                // Check if the hit is within our attack angle
                Vector3 directionToHit = (hit.point - origin).normalized;
                float angleToHit = Vector3.Angle(forward, directionToHit);

                if (angleToHit <= _angle * 0.5f)
                {
                    if (_showDebugRays)
                    {
                        Debug.DrawLine(origin, hit.point, Color.red, 1.0f);
                    }

                    // Look for a hitbox component
                    var hitbox = hit.collider.GetComponent<Hitbox>();
                    if (hitbox != null)
                    {
                        // Create hit data
                        var hitData = new HitData
                        {
                            Action = EHitAction.Damage,
                            Amount = _damage,
                            Position = hit.point,
                            Direction = forward,
                            Normal = hit.normal,
                            InstigatorRef = Object.InputAuthority
                        };

                        // Get the HitboxRoot that contains this hitbox
                        HitboxRoot hitboxRoot = hitbox.Root;
                        if (hitboxRoot != null)
                        {
                            // Get the NetworkObject from the HitboxRoot
                            NetworkObject targetObject = hitboxRoot.GetComponent<NetworkObject>();

                            // Try to find a health component on the target
                            var healthHandler = hitboxRoot.GetComponent<IHealthHandler>();
                            if (healthHandler != null)
                            {
                                // Apply damage to the health component
                                healthHandler.TakeDamage(hitData);
                            }
                            else
                            {
                                // Alternatively, try to find a damage handler component
                                var damageHandler = hitboxRoot.GetComponent<IDamageHandler>();
                                if (damageHandler != null)
                                {
                                    damageHandler.OnDamage(hitData);
                                }
                                else
                                {
                                    Debug.LogWarning($"Hit object with hitbox but no health or damage handler found");
                                }
                            }
                        }
                    }
                }
            }

            // Debug visualization
            if (_showDebugRays)
            {
                Debug.DrawLine(origin, origin + forward * _range, Color.blue, 1.0f);

                // Draw cone outline
                int segments = 8;
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = (_angle * 0.5f) * Mathf.Deg2Rad * ((float)i / segments - 0.5f);
                    float angle2 = (_angle * 0.5f) * Mathf.Deg2Rad * ((float)(i + 1) / segments - 0.5f);

                    Vector3 dir1 = Quaternion.AngleAxis(angle1 * Mathf.Rad2Deg, FireTransform.up) * forward;
                    Vector3 dir2 = Quaternion.AngleAxis(angle2 * Mathf.Rad2Deg, FireTransform.up) * forward;

                    Debug.DrawLine(origin, origin + dir1 * _range, Color.green, 1.0f);
                    Debug.DrawLine(origin + dir1 * _range, origin + dir2 * _range, Color.green, 1.0f);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying && _showDebugRays)
            {
                Vector3 origin = _meleeOrigin != null ? _meleeOrigin.position : transform.position;
                Vector3 forward = transform.forward;

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(origin, origin + forward * _range);

                // Draw cone outline
                int segments = 16;
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = (_angle * 0.5f) * Mathf.Deg2Rad * ((float)i / segments - 0.5f);
                    float angle2 = (_angle * 0.5f) * Mathf.Deg2Rad * ((float)(i + 1) / segments - 0.5f);

                    Vector3 dir1 = Quaternion.AngleAxis(angle1 * Mathf.Rad2Deg, transform.up) * forward;
                    Vector3 dir2 = Quaternion.AngleAxis(angle2 * Mathf.Rad2Deg, transform.up) * forward;

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(origin, origin + dir1 * _range);
                    Gizmos.DrawLine(origin + dir1 * _range, origin + dir2 * _range);
                }
            }
        }
    }

    // Interface definitions that need to be implemented by your health components
    public interface IHealthHandler
    {
        void TakeDamage(HitData hitData);
    }

    public interface IDamageHandler
    {
        void OnDamage(HitData hitData);
    }
}