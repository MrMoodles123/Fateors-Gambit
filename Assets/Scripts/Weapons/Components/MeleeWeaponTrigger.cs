using Fusion;
using UnityEngine;

namespace Projectiles
{
    /// <summary>
    /// Weapon component that handles melee attack timing and combos.
    /// </summary>
    public class MeleeWeaponTrigger : WeaponComponent
    {
        // PRIVATE MEMBERS

        [SerializeField]
        private int _attacksPerMinute = 90; // This would be a fairly fast swing

        [SerializeField]
        private EInputButton _attackButton = EInputButton.Fire;

        [SerializeField]
        private bool _enableCombo = true;

        [SerializeField]
        private int _maxComboCount = 3;

        [Networked]
        private TickTimer _attackCooldown { get; set; }

        [Networked]
        private int _comboCounter { get; set; }

        [Networked]
        private TickTimer _comboTimer { get; set; }

        private int _attackTicks;

        // WeaponComponent INTERFACE

        public override bool IsBusy => _attackCooldown.ExpiredOrNotRunning(Runner) == false;

        public override bool CanFire()
        {
            if (Weapon.IsBusy() == true)
                return false;

            if (_attackCooldown.ExpiredOrNotRunning(Runner) == false)
                return false;

            return PressedButtons.IsSet(_attackButton);
        }

        public override void Fire()
        {
            // Start cooldown
            _attackCooldown = TickTimer.CreateFromTicks(Runner, _attackTicks);

            // Handle combo system
            if (_enableCombo)
            {
                _comboCounter = (_comboCounter + 1) % (_maxComboCount + 1);
                if (_comboCounter == 0)
                    _comboCounter = 1;

                // Set combo timer - player has a short window to continue the combo
                _comboTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);
            }
        }

        // NetworkBehaviour INTERFACE

        public override void Spawned()
        {
            base.Spawned();

            float attackTime = 60f / _attacksPerMinute;
            _attackTicks = (int)System.Math.Ceiling(attackTime / (double)Runner.DeltaTime);
        }

        public override void FixedUpdateNetwork()
        {
            // Reset combo if timer expires
            if (_enableCombo && _comboTimer.Expired(Runner) && _comboCounter > 0 && _attackCooldown.ExpiredOrNotRunning(Runner))
            {
                _comboCounter = 0;
            }
        }
    }
}