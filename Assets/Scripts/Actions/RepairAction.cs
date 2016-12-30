using UnityEngine;

public class RepairAction : GoapAction
{
	private bool done = false;

	public override bool checkProceduralPrecondition(GameObject agent)
	{
		var vehicle = agent.GetComponent<Vehicle>();
		if (vehicle != null)
		{
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