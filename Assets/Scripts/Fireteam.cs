using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Fireteam : MonoBehaviour, IGoap
{
	public float MaxScoutDistance;
	public float MaxActionDistance;
	public float MaxAttackEnemyDistance;
	public float FormationSeparationDistance;
	public float FormationCorrection;
	public float NeedRepairAbortPercent;
	public Formation Formation;

	[ReadOnly]
	public FireteamOrder currentOrder;

	private bool moving = false;
	private HashSet<KeyValuePair<string, object>> currentGoal;
	private bool needToAbortPlan = false;

	private Blackboard blackboard;
	public Vehicle Leader;
	public Vehicle[] Members;
	private Formation[] Formations;

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

	public Vector3 AveragePosition()
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
		ChangeFormation(new WedgeFormation());

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
	}

	private void Update()
	{
		if (Members.Any(x => x == null))
		{
			Members = Members.Where(x => x != null).ToArray();
		}

		if (!Members.Any())
		{
			Destroy(gameObject);
			return;
		}

		// update order
		if (currentOrder != null && currentOrder.IsDone())
		{
			//notify?
			currentOrder = null;
		}

		var needRepairEntry = blackboard.Read(Blackboard.Keys.NeedRepair);
		if ((currentGoal == null || !currentGoal.Any(x => x.Key == GoapKeys.Repaired)) && (needRepairEntry.Count / (float)Members.Length * 100) > NeedRepairAbortPercent)
		{
			needToAbortPlan = true;
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

		//update move commands

	}

	#region IGoap members

	public HashSet<KeyValuePair<string, object>> getWorldState()
	{
		return new HashSet<KeyValuePair<string, object>> { };
	}

	public HashSet<KeyValuePair<string, object>> createGoalState()
	{
		var ret = new HashSet<KeyValuePair<string, object>> { };

		var needRepairEntry = blackboard.Read(Blackboard.Keys.NeedRepair);

		if (needRepairEntry.Any())
		{
			ret.Add(new KeyValuePair<string, object>(GoapKeys.Repaired, true));
		}
		else if (currentOrder != null)
		{
			if (currentOrder.Type == FireteamOrderType.Assault)
			{
				ret.Add(new KeyValuePair<string, object>(GoapKeys.Attacked, true));
			}
			else if (currentOrder.Type == FireteamOrderType.Defend)
			{
				ret.Add(new KeyValuePair<string, object>(GoapKeys.Defended, true));
			}
		}

		return ret;
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
		Debug.Log("plan finished");
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
					IsDone = x => Vector3.Distance(x.transform.position, nextAction.target.position) < MaxScoutDistance
				};
			}
			else
			{
				memberOrder = new VehicleOrder
				{
					Type = orderType,
					TargetPosition = targetPos,
					IsDone = x => Vector3.Distance(x.transform.position, targetPos) < MaxScoutDistance
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
			Leader.UpdateOrder(new VehicleOrder
			{
				Type = VehicleOrderType.Move,
				TargetPosition = Leader.transform.position,
				IsDone = x => nextAction.isDone()
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