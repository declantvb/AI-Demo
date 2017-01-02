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
	public float MaxDistanceFromLeader;
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
		var oldState = currentState;

		if (fleeTimeout > Time.time)
		{
			currentState = State.Flee;
			return false;
		}

		if (closestEnemy != null &&
		 Vector3.Distance(closestEnemy.position, transform.position) < FleeDistance &&
		 health.Percent < FleeHealthPercent)
		{
			currentState = State.Flee;
			return true;
		}

		if (Vector3.Distance(transform.position, Fireteam.AveragePosition()) < MaxDistanceFromLeader)
		{
			if (shooter.Target != null &&
				Vector3.Distance(transform.position, shooter.Target.position) < AutoAttackRange)
			{
				currentState = State.Fight;
				return false;
			}

			if (shooter.AttackOnSight &&
				closestEnemy != null &&
				Vector3.Distance(transform.position, closestEnemy.position) < AutoAttackRange)
			{
				currentState = State.Fight;
				return true;
			}
		}

		if (currentOrder != null)
		{
			currentState = State.FollowOrders;
			return oldState != currentState;
		}

		currentState = State.Idle;
		return oldState != currentState;
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

			Move(null, transform.position - diff.normalized * FleeDistance, false);

			fleeTimeout = Time.time + FleeTime;
		}
	}

	private void DoFight(Transform closestEnemy)
	{
		if (closestEnemy != null)
		{
			// only chanse fleeing enemy so far

			Move(closestEnemy, withTeam: false, range: shooter.Range);
			Attack(closestEnemy);
		}
	}

	private void DoIdle()
	{
		if (wanderWaitTime > Time.time)
		{
			// already wandering
			return;
		}

		var middlePos = Fireteam.AveragePosition();

		var angleBetween = Mathf.PI * 2 / Fireteam.Members.Length;
		var i = Fireteam.GetIndex(this);

		var offset = Quaternion.AngleAxis(angleBetween * i * Mathf.Rad2Deg, Vector3.up) * Vector3.forward * WanderDistance;
		var rand = UnityEngine.Random.insideUnitCircle * WanderError;
		var fudge = new Vector3(rand.x, 0, rand.y);

		// change middle to target pos?
		// if other team mates are still on their way the idle point is still in middle of team

		Move(null, middlePos + offset + fudge, slowMove: true);

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

		currentState = State.Idle;
	}

	public void DoOrder(VehicleOrder order)
	{
		switch (order.Type)
		{
			case VehicleOrderType.Attack:
				if (order.Target != null)
				{
					Move(order.Target, range: shooter.Range);
					Attack(order.Target);
				}
				else
				{
					Move(null, order.TargetPosition, range: shooter.Range);
					Attack(null, order.TargetPosition);
				}
				break;

			case VehicleOrderType.Move:
				Move(order.Target, order.TargetPosition);
				Attack(null);
				break;

			default:
				Debug.LogWarning("bad order type");
				break;
		}
	}

	private void Attack(Transform target, Vector3? targetPos = null, bool attackOnSight = true)
	{
		if (target == null && targetPos.HasValue)
		{
			float closestDistance = float.MaxValue;
			foreach (var enemy in blackboard.Read<Transform>(Blackboard.Keys.Enemy))
			{
				if (enemy == null || this == null)
				{
					continue;
				}

				var dist = Vector3.Distance(targetPos.Value, enemy.position);
				if (dist < closestDistance)
				{
					target = enemy;
					closestDistance = dist;
				}
			}
		}

		shooter.Reset();
		shooter.Target = target;
		shooter.AttackOnSight = attackOnSight;
	}

	private void Move(Transform target, Vector3? targetPos = null, bool withTeam = true, float range = 0f, bool slowMove = false)
	{
		mover.Reset();
		mover.MoveType = slowMove ? MoveType.SlowMove : MoveType.Move;

		var isLeader = Fireteam.GetLeader() == this;
		if (isLeader || !withTeam)
		{
			if (target != null)
			{
				mover.Target = target;
			}
			else if (targetPos.HasValue)
			{
				mover.TargetPosition = targetPos.Value;
			}
			else
			{
				Debug.LogError("move with no target");
			}

			mover.OrderTargetRange = range;

			if (isLeader)
			{
				var formation = Fireteam.Formation;
				mover.FormationCorrection = () =>
				{
					var points = Fireteam.Members
					.Where(x => x != null)
					.Select(x => transform.InverseTransformPoint(x.transform.position))
					.ToArray();

					return formation.SeparationCorrection(points);
				};
			}
		}
		else
		{
			var pos = Fireteam.Formation.Get(Fireteam.GetIndex(this));

			mover.Target = Fireteam.GetLeader().transform;
			mover.TargetOffset = pos;
		}
	}

	public void OnDrawGizmos()
	{
		if (Fireteam != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(transform.position, Fireteam.AveragePosition());
		}
	}

	private void ResetChildComponents()
	{
		shooter.Reset();
		mover.Reset();
	}

	public enum State
	{
		Idle,
		Fight,
		Flee,
		FollowOrders
	}
}