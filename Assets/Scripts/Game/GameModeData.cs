
using UnityEngine;


[CreateAssetMenu(fileName = "GameModeData", menuName = "GameModeData")]
public class GameModeData : ScriptableObject
{
#region Pacman

	[Space()]
	[Header("Pacman")]
	public int m_iInitialPlayerLives = 3;
	public int m_iPointsToGainOneLife = 1000;

	#endregion

#region Ghosts

	[Space()]
	[Header("Ghosts")]
	public float m_fSecondsBetweenGhostsActivation = 10.0f;

	#endregion

#region Map

	[Space()]
	[Header("Map")]
	public string m_sDefaultMapName = "Map1";

	#endregion
}
