using UnityEngine;

namespace Projectiles
{
    /// <summary>
    /// Example setup for a melee weapon.
    /// This is a helper class that contains guidance on how to set up a melee weapon in Unity.
    /// </summary>
    public class MeleeWeaponSetup : MonoBehaviour
    {
        /*
         * MELEE WEAPON SETUP GUIDE
         * 
         * 1. Create a new prefab for your melee weapon
         * 2. Add the Weapon component to the root
         * 3. Create a child GameObject named "PrimaryAction"
         * 4. Add WeaponAction component to "PrimaryAction"
         * 5. Add the following components to "PrimaryAction":
         *    - MeleeWeaponTrigger
         *    - MeleeWeaponBarrel
         *    - MeleeWeaponEffects
         *    - WeaponMagazine (optional, for stamina-based attacks)
         * 
         * HIERARCHY EXAMPLE:
         * 
         * MeleeWeapon (Weapon, NetworkObject)
         * ├── Model
         * │   ├── Sword (or other weapon model)
         * │   └── Animator (for weapon animations)
         * ├── PrimaryAction (WeaponAction)
         * │   ├── MeleeWeaponTrigger
         * │   ├── MeleeWeaponBarrel
         * │   ├── MeleeWeaponEffects
         * │   └── WeaponMagazine (optional)
         * └── FX
         *     ├── SwingParticles
         *     └── HitParticles
         * 
         * USAGE:
         * 
         * 1. Create this hierarchy for your melee weapon prefab
         * 2. Configure the parameters of each component
         * 3. Add the prefab to your _initialWeapons array in the Weapons component
         */

        // This class is just for documentation purposes and doesn't need implementation

        private void Awake()
        {
            // This script is just for guidance and should be removed from the actual prefab
            Debug.LogWarning("MeleeWeaponSetup is a documentation class and should be removed from the actual prefab.");
            Destroy(this);
        }
    }
}