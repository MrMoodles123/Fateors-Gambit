namespace Projectiles
{
	using UnityEngine;

	public static partial class ComponentExtensions
	{
		// PUBLIC METHODS

		// gets a component without making a new list
		public static T GetComponentNoAlloc<T>(this Component component) where T : class
		{
			return GameObjectExtensions<T>.GetComponentNoAlloc(component.gameObject);
		}

		// enables or disables selected component
		public static void SetActive(this Component component, bool value)
		{
			if (component == null)
				return;

			if (component.gameObject.activeSelf == value)
				return;

			component.gameObject.SetActive(value);
		}
	}
}
