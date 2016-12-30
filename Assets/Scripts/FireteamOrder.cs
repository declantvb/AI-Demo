using System;
using UnityEngine;

public class FireteamOrder
{
	public string Type;
	public Vector3 Target;
	public Func<bool> IsDone;

	public class Types
	{
		public const string Assault = "assault";
		public const string Defend = "defend";
		public const string Scout = "scout";
	}
}