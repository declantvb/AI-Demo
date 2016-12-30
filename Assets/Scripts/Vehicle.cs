using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
	public string Faction;

	public float AutoAttackRange;
	public Fireteam Fireteam;

	public VehicleOrder currentOrder;

	private Blackboard blackboard;
	private Mover mover;
	private Shooter shooter;
	private Sensor sensor;
	private bool distracted;

	private void Start()
	{
		blackboard = GetComponent<Blackboard>();
		mover = GetComponent<Mover>();
		shooter = GetComponent<Shooter>();
		sensor = GetComponent<Sensor>();
	}

	private void Update()
	{
		var closestEnemy = ClosestEnemy();
		if (shooter.AttackOnSight &&
			closestEnemy != null &&
			Vector3.Distance(transform.position, closestEnemy.transform.position) < AutoAttackRange)
		{
			//todo detect already distracted

			DoOrder(new VehicleOrder
			{
				Type = VehicleOrder.Types.Attack,
				Target = closestEnemy.transform.position,
				IsDone = x => closestEnemy == null
			});

			distracted = true;
		}
		else if (distracted && currentOrder != null)
		{
			// get back on track

			DoOrder(currentOrder);
			distracted = false;
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
			var fireteamBlackboard = Fireteam.GetComponent<Blackboard>();
			foreach (var enemy in blackboard.Read("enemy"))
			{
				fireteamBlackboard.Write(enemy);
			}
		}
	}

	public Transform ClosestEnemy()
	{
		return blackboard.Read<Transform>("enemy")
			.Where(x => x != null)
			.OrderBy(x => Vector3.Distance(transform.position, x.position))
			.FirstOrDefault();
	}

	public List<Transform> EnemiesWithin(float distance)
	{
		return blackboard.Read<Transform>("enemy")
			.Where(x => x != null && Vector3.Distance(transform.position, x.position) < distance)
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
				Move(order.Target, attack: true, range: shooter.Range);
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
			foreach (var enemy in blackboard.Read<Transform>("enemy"))
			{
				if (enemy == null || this == null)
				{
					continue;
				}

				var dist = Vector3.Distance(transform.position, enemy.position);
				if (dist < closestDistance)
				{
					closestToTarget = enemy;
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