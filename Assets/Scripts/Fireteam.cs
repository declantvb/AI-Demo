using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Fireteam : MonoBehaviour, IGoap
{
	public float MaxScoutDistance;
	public float MaxActionDistance;
	public float MaxAttackEnemyDistance;

	//temp
	public bool Defend;

	private Blackboard blackboard;
	public Vehicle[] Members;

	public Vector3 AveragePosition
	{
		get
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
	}

	private FireteamOrder currentOrder;
	private bool moving = false;

	private void Start()
	{
		blackboard = GetComponent<Blackboard>();

		foreach (var member in Members)
		{
			member.Fireteam = this;
		}

		//add goap actions
		gameObject.AddComponent<ExploreAction>();
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
			//ded
			return;
		}

		// update order
		if (currentOrder != null && currentOrder.IsDone())
		{
			//notify?
			currentOrder = null;
		}
	}

	public void UpdateOrder(FireteamOrder order)
	{
		currentOrder = order;
	}

	#region IGoap members

	public HashSet<KeyValuePair<string, object>> getWorldState()
	{
		return new HashSet<KeyValuePair<string, object>>
		{
			new KeyValuePair<string, object>(GoapKeys.ShouldDefend, Defend)
		};
	}

	public HashSet<KeyValuePair<string, object>> createGoalState()
	{
		var ret = new HashSet<KeyValuePair<string, object>> { };

		var needRepairEntry = blackboard.Read(Blackboard.Keys.NeedRepair);

		if (needRepairEntry.Any())
		{
			ret.Add(new KeyValuePair<string, object>(GoapKeys.Repaired, true));
		}
		else if (Defend)
		{
			ret.Add(new KeyValuePair<string, object>(GoapKeys.Defending, true));
		}

		return ret;
	}

	public void planFailed(HashSet<KeyValuePair<string, object>> failedGoal)
	{
		Debug.LogWarning("plan failed");
	}

	public void planFound(HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions)
	{
		Debug.Log("plan found");
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
			var memberOrder = new VehicleOrder
			{
				Type = VehicleOrder.Types.Move,
				Target = targetPos,
				IsDone = x => Vector3.Distance(x.transform.position, targetPos) < MaxScoutDistance
			};

			foreach (var member in Members)
			{
				member.UpdateOrder(memberOrder);
			}

			moving = true;
		}
		else if (Vector3.Distance(AveragePosition, targetPos) < MaxActionDistance)
		{
			goapReset();
			nextAction.setInRange(true);
			return true;
		}
		else if (Members.All(x => x.currentOrder == null))
		{
			moving = false;
		}

		return false;
	}

	private void goapReset()
	{
		moving = false;
	}

	#endregion IGoap members
}