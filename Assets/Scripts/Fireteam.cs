using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Fireteam : MonoBehaviour, IGoap
{
	public float MaxScoutDistance;
	public float MaxActionDistance;
	public float MaxAttackEnemyDistance;
	public float MaxChaseDistance;
	public float RetreatDistance;
	public float FormationSeparationDistance;
	public float FormationCorrection;
	public float NeedRepairAbortPercent;
	public Formation Formation;
	
	public FireteamOrder currentOrder;

	private bool moving = false;
	private HashSet<KeyValuePair<string, object>> currentGoal;
	private bool needToAbortPlan = false;

	private Blackboard blackboard;
	public Vehicle Leader;
	public Vehicle[] Members;

	public Vehicle GetLeader()
	{
		if (Leader == null)
		{
			Leader = Members.FirstOrDefault();
			if (Leader == null)
			{
				Debug.LogError("no new leader");
			}
		}

		return Leader;
	}

	public int GetIndex(Vehicle vehicle)
	{
		return Array.IndexOf(Members, vehicle);
	}

	private Vector3 averagePosition()
	{
		if (!Members.Any())
		{
			return Vector3.zero;
		}

		return Members
			.Where(x => x != null)
			.Select(x => x.transform.position)
			.Aggregate((acc, val) => acc + val) / Members.Count();
	}

	private void Start()
	{
		ChangeFormation(new LineFormation());

		blackboard = GetComponent<Blackboard>();

		foreach (var member in Members)
		{
			member.Fireteam = this;
		}

		//add goap actions
		gameObject.AddComponent<ExploreAction>();
		gameObject.AddComponent<AttackAction>();
		gameObject.AddComponent<DefendAction>();
		gameObject.AddComponent<RepairAction>();
		gameObject.AddComponent<RetreatAction>();

		GetComponent<AttackAction>().MaxChaseDistance = MaxChaseDistance;
		GetComponent<RetreatAction>().RetreatDistance = RetreatDistance;
	}

	private void Update()
	{
		if (Members.Any(x => x == null))
		{
			Members = Members.Where(x => x != null).ToArray();

			// fix current plans
			needToAbortPlan = true;
		}

		if (!Members.Any())
		{
			Destroy(gameObject);
			return;
		}

		transform.position = averagePosition();

		// update order
		if (currentOrder != null && currentOrder.IsDone())
		{
			//notify?
			currentOrder = null;
		}

		CheckForAbortConditions();
	}

	private void CheckForAbortConditions()
	{
		var needRepairEntry = blackboard.Read(Blackboard.Keys.NeedRepair);
		var alliesDown = blackboard.Read(Blackboard.Keys.AllyDown);
		if (currentGoal != null && currentGoal.Any(x => x.Key == GoapKeys.Damaged))
		{
			return;
		}
		else if ((needRepairEntry.Count / (float)(Members.Length + alliesDown.Count) * 100) >= NeedRepairAbortPercent)
		{
			needToAbortPlan = true;
			return;
		}

		var attacking = blackboard.Read<Transform>(Blackboard.Keys.Attacking);
		if (currentGoal != null && currentGoal.Any(x => x.Key == GoapKeys.Attack))
		{
			return;
		}
		else if (attacking.Any())
		{
			needToAbortPlan = true;
			return;
		}
	}

	public void UpdateOrder(FireteamOrder order)
	{
		currentOrder = order;

		//abort current plan
		needToAbortPlan = true;
	}

	public void ChangeFormation(Formation newFormation)
	{
		Formation = newFormation;
		Formation.SeparationDistance = FormationSeparationDistance;
		Formation.CorrectionSpeed = FormationCorrection;
	}

	#region IGoap members

	public HashSet<KeyValuePair<string, object>> getWorldState()
	{
		var state = new HashSet<KeyValuePair<string, object>> { };

		state.Add(new KeyValuePair<string, object>(GoapKeys.AlliesDown, blackboard.Read(Blackboard.Keys.AllyDown).Count));
		state.Add(new KeyValuePair<string, object>(GoapKeys.Damaged, blackboard.Read(Blackboard.Keys.NeedRepair).Any()));
		state.Add(new KeyValuePair<string, object>(GoapKeys.SeenEnemy, blackboard.Read(Blackboard.Keys.Enemy).Any()));
		state.Add(new KeyValuePair<string, object>(GoapKeys.InCombat, blackboard.Read(Blackboard.Keys.Attacking).Any()));

		return state;
	}

	public HashSet<KeyValuePair<string, object>> createGoalState()
	{
		var goal = new HashSet<KeyValuePair<string, object>> { };

		if (blackboard.Read(Blackboard.Keys.NeedRepair).Any())
		{
			goal.Add(new KeyValuePair<string, object>(GoapKeys.Damaged, false));
		}
		else if (blackboard.Read(Blackboard.Keys.Attacking).Any())
		{
			goal.Add(new KeyValuePair<string, object>(GoapKeys.Attack, true));
		}
		else if (currentOrder != null)
		{
			if (currentOrder.Type == FireteamOrderType.Assault)
			{
				goal.Add(new KeyValuePair<string, object>(GoapKeys.Attack, true));
			}
			else if (currentOrder.Type == FireteamOrderType.Defend)
			{
				goal.Add(new KeyValuePair<string, object>(GoapKeys.Defend, true));
			}
		}

		return goal;
	}

	public void planFailed(HashSet<KeyValuePair<string, object>> failedGoal)
	{
		Debug.LogWarning("plan failed");
	}

	public void planFound(HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions)
	{
		currentGoal = goal;
		var actionNames = actions.Select(x => x.GetType().Name);
		Debug.Log("plan found: " + string.Join(" -> ", actionNames.ToArray()));
	}

	public void actionsFinished()
	{
		goapReset();
		//Debug.Log("plan finished");
	}

	public void planAborted(GoapAction aborter)
	{
		goapReset();
		Debug.LogWarning("plan aborted");
	}

	public bool moveAgent(GoapAction nextAction)
	{
		Vector3 targetPos;
		if (nextAction.target != null)
		{
			targetPos = nextAction.target.position;
		}
		else if (nextAction.targetPosition.HasValue)
		{
			targetPos = nextAction.targetPosition.Value;
		}
		else
		{
			Debug.LogError("goap move with no target");
			return true;
		}

		var imperative = nextAction is RetreatAction;

		if (!moving)
		{
			VehicleOrder memberOrder;
			VehicleOrderType orderType = nextAction is AttackAction ? VehicleOrderType.Attack : VehicleOrderType.Move;

			if (nextAction.target != null)
			{
				memberOrder = new VehicleOrder
				{
					Type = orderType,
					Target = nextAction.target,
					IsDone = x => nextAction.target != null && Vector3.Distance(x.transform.position, nextAction.target.position) < MaxScoutDistance,
					IsImperative = imperative
				};
			}
			else
			{
				memberOrder = new VehicleOrder
				{
					Type = orderType,
					TargetPosition = targetPos,
					IsDone = x => Vector3.Distance(x.transform.position, targetPos) < MaxScoutDistance,
					IsImperative = imperative
				};
			}

			foreach (var member in Members)
			{
				member.UpdateOrder(memberOrder);
			}

			moving = true;
		}
		else if (GetLeader() != null && Vector3.Distance(GetLeader().transform.position, targetPos) < MaxActionDistance)
		{
			goapReset();
			// hold position
			Leader.UpdateOrder(new VehicleOrder
			{
				Type = VehicleOrderType.Move,
				TargetPosition = Leader.transform.position,
				IsDone = _ => nextAction.isDone()
			});

			nextAction.setInRange(true);
			return true;
		}
		else if (Members.All(x => x.currentOrder == null))
		{
			moving = false;
		}

		return false;
	}

	public bool abortPlan()
	{
		if (needToAbortPlan)
		{
			needToAbortPlan = false;
			return true;
		}

		return false;
	}

	private void goapReset()
	{
		moving = false;
	}

	#endregion IGoap members
}