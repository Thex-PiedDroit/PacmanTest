
using UnityEngine;


abstract public class GhostBehaviour : ScriptableObject
{
	public Color m_tGhostColor = Color.red;


	abstract public void UpdateGhostDestination(Ghost pGhost);
}
