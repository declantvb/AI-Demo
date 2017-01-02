using System;
using UnityEngine;

public class Mover : MonoBehaviour
{
	public MoveType MoveType;
	public Transform Target;
	public Vector3 TargetOffset;
	public Vector3 TargetPosition;
	public float OrderTargetRange;
	public Func<float> FormationCorrection;

	public float MoveSpeed = 5;
	public float RepelDistance = 3;
	public float SlowMoveFactor = 0.6f;

	private Vector3 targetPos
	{
		get
		{
			if (Target != null)
			{
				return Target.position + (Target.rotation * TargetOffset);
			}

			return TargetPosition;
		}
	}


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

		foreach (var ally in blackboard.Read<Transform>(Blackboard.Keys.Ally))
		{
			if (ally == null)
			{
				continue;
			}

			var diff = ally.position - transform.position;
			var close = RepelDistance - diff.magnitude;

			var allyVehicle = ally.GetComponent<Vehicle>();
			if (close > 0 && allyVehicle.Fireteam == vehicle.Fireteam)
			{
				transform.position -= diff.normalized * close;
			}
		}
	}

	private void Move(float moveFactor = 1f)
	{
		Vector3 target = targetPos;

		var diff = target - transform.position;
		var magnitude = diff.magnitude;

		if (FormationCorrection != null)
		{
			moveFactor = moveFactor * FormationCorrection();
		}

		if (magnitude < OrderTargetRange)
		{
			return;
		}

		if (diff != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(diff.normalized);
		}

		var maxMoveThisFrame = Time.deltaTime * MoveSpeed * moveFactor;
		if (magnitude < maxMoveThisFrame)
		{
			transform.position += transform.forward * magnitude;
		}
		else
		{
			transform.position += transform.forward * maxMoveThisFrame;
		}
	}

	public void OnDrawGizmos()
	{
		if (MoveType != MoveType.None)
		{
			Gizmos.DrawLine(transform.position, targetPos);
		}
	}

	public void Reset()
	{
		Target = null;
		TargetOffset = Vector3.zero;
		TargetPosition = Vector3.zero;
		OrderTargetRange = 0f;
		MoveType = MoveType.None;
		FormationCorrection = null;
	}
}

public enum MoveType
{
	None,
	Move,
	SlowMove
}