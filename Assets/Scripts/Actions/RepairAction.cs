using System.Linq;
using UnityEngine;

public class RepairAction : GoapAction
{
	private bool done = false;

	public RepairAction()
	{
		addPrecondition(GoapKeys.InCombat, false);
		addPrecondition(GoapKeys.Damaged, true);
		addEffect(GoapKeys.Damaged, false);
	}

	public override bool checkProceduralPrecondition(GameObject agent)
	{
		var blackboard = agent.GetComponent<Blackboard>();

		if (blackboard != null)
		{
			var known = blackboard.Read<Transform>(Blackboard.Keys.RepairStation);

			if (known.Any())
			{
				var closest = known.OrderBy(x => Vector3.Distance(agent.transform.position, x.position)).First();

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
		var fireteam = agent.GetComponent<Fireteam>();
		if (fireteam != null)
		{
			if (fireteam.Members.Select(x => x.GetComponent<Health>()).All(x => x != null && x.Percent == 100))
			{
				done = true;
			}
		}
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