using System.Collections.Generic;
using UnityEngine;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace Projectiles
{
	public static class SceneExtensions
	{
		// PUBLIC METHODS

		// gets the first component of a given type in the specified scene
		public static T GetComponent<T>(this UnityScene scene, bool includeInactive = false) where T : class
		{
			List<GameObject> roots = new List<GameObject>();
			scene.GetRootGameObjects(roots);

			for (int i = 0, count = roots.Count; i < count; ++i)
			{
				T component = roots[i].GetComponentInChildren<T>(includeInactive);
				if (component != null)
					return component;
			}

			return default;
		}

		// gets all components of chosen type in the specified scene
		public static List<T> GetComponents<T>(this UnityScene scene, bool includeInactive = false) where T : class
		{
			List<T>          allComponents    = new List<T>();
			List<T>          objectComponents = new List<T>();
			List<GameObject> sceneRootObjects = new List<GameObject>();

			scene.GetRootGameObjects(sceneRootObjects);

			for (int i = 0, count = sceneRootObjects.Count; i < count; ++i)
			{
				sceneRootObjects[i].GetComponentsInChildren(includeInactive, objectComponents);
				allComponents.AddRange(objectComponents);
				objectComponents.Clear();
			}

			return allComponents;
		}

		// not sure how to explain this
		public static void GetComponents<T>(this UnityScene scene, List<T> components, bool includeInactive = false) where T : class
		{
			List<T>          objectComponents = new List<T>();
			List<GameObject> sceneRootObjects = new List<GameObject>();

			scene.GetRootGameObjects(sceneRootObjects);
			components.Clear();

			for (int i = 0, count = sceneRootObjects.Count; i < count; ++i)
			{
				sceneRootObjects[i].GetComponentsInChildren(includeInactive, objectComponents);
				components.AddRange(objectComponents);
				objectComponents.Clear();
			}
		}
	}
}
