using System;
using UnityEngine;

public class FireteamOrder
{
	public FireteamOrderType Type;
	public Transform Target;
	public Vector3 TargetPosition;
	public Func<bool> IsDone;
}

public enum FireteamOrderType
{
	Assault,
	Defend,
	Scout
}