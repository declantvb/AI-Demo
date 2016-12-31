using System;
using UnityEngine;

public class VehicleOrder
{
	public string Type;
	public Vector3 Target;
	public Func<Vehicle, bool> IsDone;

	public class Types
	{
		public const string Attack = "attack";
		public const string Hold = "hold";
		public const string Move = "move";
	}
}