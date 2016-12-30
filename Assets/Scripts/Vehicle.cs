using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
	public string Faction;

	public float AutoAttackRange;
	public Fireteam Fireteam;

	public VehicleOrder currentOrder;

	private Mover mover;
	private Shooter shooter;
	private Sensor sensor;

	private void Start()
	{
		mover = GetComponent<Mover>();
		shooter = GetComponent<Shooter>();
		sensor = GetComponent<Sensor>();
	}

	private void Update()
	{
		if (shooter.AttackOnSight && 
			sensor.ClosestEnemy != null && 
			sensor.ClosestEnemy.Transform != null && 
			Vector3.Distance(transform.position, sensor.ClosestEnemy.Transform.position) < AutoAttackRange)
		{
			//todo detect already distracted

			DoOrder(new VehicleOrder
			{
				Type = VehicleOrder.Types.Attack,
				Target = sensor.ClosestEnemy.Transform.position,
				IsDone = x => sensor.ClosestEnemy.Transform == null
			});
		}

		// update order
		if (currentOrder != null && currentOrder.IsDone(this))
		{
			currentOrder = null;
			ResetChildComponents();
		}

		// update Fireteam
		if (Fireteam != null)
		{
			foreach (var enemy in sensor.Enemies)
			{
				Fireteam.blackboard.Store(Blackboard.Keys.Enemy, enemy.Transform, Time.time + Fireteam.blackboard.ExpiryTime);
			} 
		}
	}

	public List<Vehicle> EnemiesWithin(float distance)
	{
		return sensor.Enemies
			.Select(x => x.Transform.GetComponent<Vehicle>())
			.Where(x => x.transform != null && Vector3.Distance(transform.position, x.transform.position) < distance)
			.ToList();
	}

	public void UpdateOrder(VehicleOrder order)
	{
		currentOrder = order;

		DoOrder(currentOrder);
	}

	public void DoOrder(VehicleOrder order)
	{
		switch (order.Type)
		{
			case VehicleOrder.Types.Attack:
				Move(order.Target, attack: true, range : shooter.Range);
				break;

			case VehicleOrder.Types.Move:
				Move(order.Target);
				break;

			case VehicleOrder.Types.Wander:
				Move(order.Target, slowMove: true);
				break;

			default:
				Debug.LogWarning("bad order type");
				break;
		}
	}

	public bool OrderComplete()
	{
		return currentOrder.IsDone(this);
	}

	private void Move(Vector3 target, bool? attack = null, float range = 0f, bool slowMove = false)
	{
		Transform closestToTarget = null;
		if (attack.HasValue && attack.Value)
		{
			float closestDistance = float.MaxValue;
			foreach (var enemy in sensor.Enemies)
			{
				if (enemy.Transform == null || this == null)
				{
					continue;
				}

				var dist = Vector3.Distance(transform.position, enemy.Transform.position);
				if (dist < closestDistance)
				{
					closestToTarget = enemy.Transform;
					closestDistance = dist;
				}
			}
		}

		shooter.OrderTarget = closestToTarget;
		shooter.AttackOnSight = !attack.HasValue || attack.Value;
		mover.OrderTarget = target;
		mover.OrderTargetRange = range;
		mover.MoveType = slowMove ? MoveType.SlowMove : MoveType.Move;
	}

	private void ResetChildComponents()
	{
		shooter.OrderTarget = null;
		shooter.AttackOnSight = true;
		mover.OrderTarget = Vector3.zero;
		mover.OrderTargetRange = 0f;
		mover.MoveType = MoveType.None;
	}
}