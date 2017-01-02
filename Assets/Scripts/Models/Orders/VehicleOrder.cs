using System;
using UnityEngine;

public class VehicleOrder
{
	public VehicleOrderType Type;
	public Transform Target;
	public Vector3 TargetPosition;
	public Func<Vehicle, bool> IsDone;
}

public enum VehicleOrderType
{
	Attack,
	Move
}