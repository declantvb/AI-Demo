using System;
using UnityEngine;

public class Mover : MonoBehaviour
{
	public MoveType MoveType;
	public Vector3 OrderTarget;
	public float OrderTargetRange;
	public float MoveSpeed = 5;
	public float RepelDistance = 3;
	public float SlowMoveFactor = 0.6f;

	private Vehicle vehicle;
	private Blackboard blackboard;

	private void Start()
	{
		vehicle = GetComponent<Vehicle>();
		blackboard = GetComponent<Blackboard>();
	}

	private void Update()
	{
		switch (MoveType)
		{
			case MoveType.None:
				break;
			case MoveType.Move:
				Move();
				break;
			case MoveType.SlowMove:
				Move(SlowMoveFactor);
				break;
			default:
				break;
		}

		foreach (var ally in blackboard.Read<Transform>("ally"))
		{
			if (ally == null)
			{
				continue;
			}

			var dir = ally.position - transform.position;
			var close = RepelDistance - dir.magnitude;

			var allyVehicle = ally.GetComponent<Vehicle>();
			if (close > 0 && allyVehicle.Fireteam == vehicle.Fireteam)
			{
				transform.position -= dir.normalized * close;
			}
		}
	}

	private void Move(float moveFactor = 1f)
	{
		var diff = OrderTarget - transform.position;

		if (diff.magnitude < OrderTargetRange)
		{
			return;
		}
		else if (diff.magnitude < Time.deltaTime * MoveSpeed * moveFactor)
		{
			transform.position += diff;
		}
		else
		{
			transform.position += diff.normalized * Time.deltaTime * MoveSpeed * moveFactor;
		}
	}

	public void OnDrawGizmos()
	{
		if (OrderTarget != null && MoveType != MoveType.None)
		{
			Gizmos.DrawLine(transform.position, OrderTarget);
		}
	}
}

public enum MoveType
{
	None,
	Move,
	SlowMove
}