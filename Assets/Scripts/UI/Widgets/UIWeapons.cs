using System.Collections.Generic;
using UnityEngine;

namespace Projectiles.UI
{
	public class UIWeapons : UIBehaviour
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private UIWeapon _weaponDescription;


		private int _currentWeaponSlot;
		private List<Weapon> _weapons = new(32);

		private int _lastVersion;

		// PUBLIC METHODS

		public void UpdateWeapons(Weapons weapons)
		{
			UpdateData(weapons);

			if (_currentWeaponSlot != weapons.CurrentWeaponSlot)
			{
				_weaponDescription.SetData(weapons.CurrentWeapon);
				_currentWeaponSlot = weapons.CurrentWeaponSlot;

			}
		}

		// PRIVATE METHODS

		private void UpdateData(Weapons weapons)
		{
			if (weapons.Version == _lastVersion)
				return; // Weapons did not change

			_weapons.Clear();
			weapons.GetAllWeapons(_weapons);
			_currentWeaponSlot = -1;

			_lastVersion = weapons.Version;
		}
	}
}
