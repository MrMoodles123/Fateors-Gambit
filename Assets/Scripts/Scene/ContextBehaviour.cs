using Fusion;

namespace Projectiles
{
	// get and set all context behaviours
	public interface IContextBehaviour
	{
		SceneContext Context { get; set; }
	}

	public abstract class ContextBehaviour : NetworkBehaviour, IContextBehaviour
	{
		public SceneContext Context { get; set; }
	}

	public abstract class ContextSimulationBehaviour : SimulationBehaviour, IContextBehaviour
	{
		public SceneContext Context { get; set; }
	}
}
