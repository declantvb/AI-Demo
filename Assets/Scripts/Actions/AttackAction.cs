using System.Linq;
using UnityEngine;

public class AttackAction : GoapAction
{
	public float MaxChaseDistance;

	private float clearRadius;
	private bool destroyTarget = false;
	private bool done = false;

	public AttackAction()
	{
		addEffect(GoapKeys.Attack, true);
		cost = 2f;
	}

	public override bool checkProceduralPrecondition(GameObject agent)
	{
		var fireteam = agent.GetComponent<Fireteam>();
		if (fireteam != null && fireteam.currentOrder != null && fireteam.currentOrder.Type == FireteamOrderType.Assault)
		{
			if (fireteam.currentOrder.Target != null)
			{
				destroyTarget = true;
				target = fireteam.currentOrder.Target;
			}
			else
			{
				targetPosition = fireteam.currentOrder.TargetPosition;
				clearRadius = 15f;
			}
			return true;
		}

		var blackboard = agent.GetComponent<Blackboard>();
		if (blackboard != null)
		{
			var known = blackboard.Read<Transform>(Blackboard.Keys.Enemy);

			if (known.Any())
			{
				var closest = known.OrderBy(x => Vector3.Distance(agent.transform.position, x.position)).First();

				destroyTarget = true;
				target = closest;
				return true;
			}
		}

		return false;
	}

	public override bool isDone()
	{
		return done;
	}

	public override bool perform(GameObject agent)
	{
		if (destroyTarget)
		{
			if (target == null)
			{
				done = true;
				return true;
			}

			if (Vector3.Distance(agent.transform.position, target.position) < MaxChaseDistance)
			{
				return true;
			}
		}
		else if (targetPosition.HasValue)
		{
			//no nearby enemies

			var blackboard = agent.GetComponent<Blackboard>();
			if (blackboard != null)
			{
				var known = blackboard.Read<Transform>(Blackboard.Keys.Enemy);

				if (known.Where(x => Vector3.Distance(targetPosition.Value, x.position) < clearRadius).All(x => x == null))
				{
					done = true;
				}

				//timeout?
				return true;
			}
		}

		return false;
	}

	public override bool requiresInRange()
	{
		return true;
	}

	public override void reset()
	{
		done = false;
	}
}