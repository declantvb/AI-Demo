using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
	public string Faction;
	public float FleeHealthPercent;
	public float FleeDistance;
	public float FleeTime;
	public float WanderDistance;
	public float WanderError;
	public float WanderWaitTimeMin;
	public float WanderWaitTimeMax;
	public float AutoAttackRange;
	public float NeedRepairThreshold;
	public Fireteam Fireteam;

	public VehicleOrder currentOrder;

	public State currentState;

	private Blackboard blackboard;
	private Mover mover;
	private Shooter shooter;
	private Sensor sensor;
	private Health health;
	private float wanderWaitTime;
	private float fleeTimeout = 0;

	private void Start()
	{
		blackboard = GetComponent<Blackboard>();
		mover = GetComponent<Mover>();
		shooter = GetComponent<Shooter>();
		sensor = GetComponent<Sensor>();
		health = GetComponent<Health>();
	}

	private void Update()
	{
		var closestEnemy = ClosestEnemy();

		if (UpdateState(closestEnemy))
		{
			DoState(currentState, closestEnemy);
		}

		// reset order when complete
		if (currentOrder != null && currentOrder.IsDone(this))
		{
			//notify?
			currentOrder = null;
			ResetChildComponents();
		}

		// update Fireteam with data
		if (Fireteam != null)
		{
			var fireteamBlackboard = Fireteam.GetComponent<Blackboard>();

			foreach (var enemy in blackboard.Read(Blackboard.Keys.Enemy))
			{
				fireteamBlackboard.Write(enemy);
			}

			foreach (var repairStation in blackboard.Read(Blackboard.Keys.RepairStation))
			{
				fireteamBlackboard.Write(repairStation);
			}

			if (health.Percent < NeedRepairThreshold)
			{
				fireteamBlackboard.Write(Blackboard.Keys.NeedRepair, this, Time.time + 1);
			}
		}
	}

	private bool UpdateState(Transform closestEnemy)
	{
		var newState = State.Idle;

		if (fleeTimeout > Time.time || 
			(closestEnemy != null &&
			Vector3.Distance(closestEnemy.position, transform.position) < FleeDistance &&
			health.Percent < FleeHealthPercent))
		{
			newState = State.Flee;
		}
		else if (closestEnemy != null &&
			shooter.AttackOnSight &&
			Vector3.Distance(transform.position, closestEnemy.position) < AutoAttackRange)
		{
			newState = State.Fight;
		}
		else if (currentOrder != null)
		{
			newState = State.FollowOrders;
		}

		if (currentState == State.FollowOrders && newState == State.FollowOrders)
		{
			return false;
		}
		currentState = newState;
		return true;
	}

	private void DoState(State state, Transform closestEnemy)
	{
		switch (state)
		{
			case State.Idle:
				DoIdle();
				break;

			case State.Fight:
				DoFight(closestEnemy);
				break;

			case State.Flee:
				DoFlee(closestEnemy);
				break;

			case State.FollowOrders:
				DoOrder(currentOrder);
				break;

			default:
				Debug.LogError("invalid state");
				break;
		}
	}

	private void DoFlee(Transform closestEnemy)
	{
		if (closestEnemy != null)
		{
			// flee faster than chasers

			var diff = closestEnemy.position - transform.position;

			Move(transform.position - diff.normalized * FleeDistance, false);

			fleeTimeout = Time.time + FleeTime;
		}
	}

	private void DoFight(Transform closestEnemy)
	{
		if (closestEnemy != null)
		{
			// only chanse fleeing enemy so far

			Move(closestEnemy.position, attack: true, range: shooter.Range);
		}
	}

	private void DoIdle()
	{
		if (wanderWaitTime > Time.time)
		{
			// already wandering
			return;
		}

		var middlePos = Fireteam.AveragePosition;

		var angleBetween = Mathf.PI * 2 / Fireteam.Members.Length;
		var i = Array.IndexOf(Fireteam.Members, this);

		var offset = Quaternion.AngleAxis(angleBetween * i * Mathf.Rad2Deg, Vector3.up) * Vector3.forward * WanderDistance;
		var rand = UnityEngine.Random.insideUnitCircle * WanderError;
		var fudge = new Vector3(rand.x, 0, rand.y);

		// change middle to target pos?
		// if other team mates are still on their way the idle point is still in middle of team

		Move(middlePos + offset + fudge, slowMove: true);

		wanderWaitTime = Time.time + UnityEngine.Random.value * (WanderWaitTimeMax - WanderWaitTimeMin) + WanderWaitTimeMin;
	}

	public Transform ClosestEnemy()
	{
		return blackboard.Read<Transform>(Blackboard.Keys.Enemy)
			.Where(x => x != null)
			.OrderBy(x => Vector3.Distance(transform.position, x.position))
			.FirstOrDefault();
	}

	public List<Transform> EnemiesWithin(float distance)
	{
		return blackboard.Read<Transform>(Blackboard.Keys.Enemy)
			.Where(x => x != null && Vector3.Distance(transform.position, x.position) < distance)
			.ToList();
	}

	public void UpdateOrder(VehicleOrder order)
	{
		currentOrder = order;

		DoOrder(currentOrder);
	}

	public bool OrderComplete()
	{
		return currentOrder.IsDone(this);
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

			default:
				Debug.LogWarning("bad order type");
				break;
		}
	}

	private void Move(Vector3 target, bool? attack = null, float range = 0f, bool slowMove = false)
	{
		Transform closestToTarget = null;
		if (attack.HasValue && attack.Value)
		{
			float closestDistance = float.MaxValue;
			foreach (var enemy in blackboard.Read<Transform>(Blackboard.Keys.Enemy))
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

	public enum State
	{
		Idle,
		Fight,
		Flee,
		FollowOrders
	}
}