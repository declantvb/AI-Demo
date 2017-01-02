using System.Linq;
using UnityEngine;

public class RetreatAction : GoapAction
{
	public float RetreatDistance;

	private bool done = false;

	public RetreatAction()
	{
		addPrecondition(GoapKeys.InCombat, true);
		addEffect(GoapKeys.InCombat, false);
	}

	public override bool checkProceduralPrecondition(GameObject agent)
	{
		var blackboard = agent.GetComponent<Blackboard>();

		if (blackboard != null)
		{
			var enemies = blackboard.Read<Transform>(Blackboard.Keys.Enemy);

			if (enemies.Any())
			{
				var closest = enemies.OrderBy(x => Vector3.Distance(agent.transform.position, x.position)).First();

				targetPosition = agent.transform.position + (agent.transform.position - closest.position).normalized * RetreatDistance;
			}
		}

		return true;
	}

	public override bool isDone()
	{
		return done;
	}

	public override bool perform(GameObject agent)
	{
		done = true;
		return true;
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