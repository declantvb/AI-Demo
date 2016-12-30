using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Fireteam : MonoBehaviour, IGoap
{
	public float MaxScoutDistance;
	public float MaxActionDistance;
	public float MaxAttackEnemyDistance;
	public float WanderDistance;
	public float WanderError;
	public float WanderWaitTimeMin;
	public float WanderWaitTimeMax;

	//temp
	public bool Defend;

	private Blackboard blackboard;
	public Vehicle[] Members;

	public Vector3 AveragePosition
	{
		get
		{
			var nonNull = Members
				.Where(x => x != null);

			if (!nonNull.Any())
			{
				return Vector3.zero;
			}

			return nonNull
				.Select(x => x.transform.position)
				.Aggregate((acc, val) => acc + val) / nonNull.Count();
		}
	}

	private FireteamOrder currentOrder;
	private bool attacking = false;
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
	}

	private void Update()
	{
		if (Members.All(x => x == null))
		{
			return;
		}

		// attack closest
		//var enemies = blackboard.Get(Blackboard.Keys.Enemy);
		//if (enemies.Any())
		//{
		//	var closest = enemies.Select(x => x.Data as Transform).OrderBy(x => x == null ? float.MaxValue : Vector3.Distance(AveragePosition, x.position)).FirstOrDefault();

		//	if (closest != null)
		//	{
		//		DoOrder(new FireteamOrder
		//		{
		//			Type = FireteamOrder.Types.Assault,
		//			Target = closest.position,
		//			IsDone = () => closest == null
		//		});

		//		attacking = true;
		//	}
		//}
		//else
		//{
		//	attacking = false;
		//}

		// update order
		if (currentOrder != null && currentOrder.IsDone())
		{
			currentOrder = null;
		}

		//wander around current area
		//if (currentOrder == null && !attacking)
		//{
		//	var pos = AveragePosition;

		//	var fullCircle = Mathf.PI * 2;

		//	var angleBetween = fullCircle / Members.Length;
		//	var i = 0;
		//	foreach (var member in Members)
		//	{
		//		if (member.currentOrder == null || member.currentOrder.Type != VehicleOrder.Types.Wander)
		//		{
		//			var offset = Quaternion.AngleAxis(angleBetween * i * Mathf.Rad2Deg, Vector3.up) * Vector3.forward * WanderDistance;
		//			var rand = UnityEngine.Random.insideUnitCircle * WanderError;
		//			var waitTime = Time.time + UnityEngine.Random.value * (WanderWaitTimeMax - WanderWaitTimeMin) + WanderWaitTimeMin;
		//			member.UpdateOrder(new VehicleOrder
		//			{
		//				Type = VehicleOrder.Types.Wander,
		//				Target = pos + offset + new Vector3(rand.x, 0, rand.y),
		//				IsDone = x => Time.time > waitTime
		//			});
		//		}

		//		i++;
		//	}
		//}
	}

	public void UpdateOrder(FireteamOrder order)
	{
		currentOrder = order;

		DoOrder(currentOrder);
	}

	public void DoOrder(FireteamOrder order)
	{
		VehicleOrder memberOrder = null;
		switch (order.Type)
		{
			case FireteamOrder.Types.Assault:
				memberOrder = new VehicleOrder
				{
					Type = VehicleOrder.Types.Attack,
					Target = order.Target,
					IsDone = x => Vector3.Distance(x.transform.position, order.Target) < MaxScoutDistance && !x.EnemiesWithin(MaxAttackEnemyDistance).Any()
				};
				break;

			//case FireteamOrder.Types.Defend:
			//	memberOrder = new VehicleOrder
			//	{
			//		Type = VehicleOrder.Types.Hold,
			//		Target = order.Target,
			//		IsDone = ???
			//	};
			//	break;

			case FireteamOrder.Types.Scout:
				memberOrder = new VehicleOrder
				{
					Type = VehicleOrder.Types.Move,
					Target = order.Target,
					IsDone = x => Vector3.Distance(x.transform.position, order.Target) < MaxScoutDistance
				};
				break;

			default:
				break;
		}

		if (memberOrder != null)
		{
			foreach (var member in Members)
			{
				member.UpdateOrder(memberOrder);
			}
		}
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
		if (Defend)
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
			targetPos = nextAction.target.transform.position;
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

		return false;
	}

	private void goapReset()
	{
		moving = false;
	}

	#endregion IGoap members
}