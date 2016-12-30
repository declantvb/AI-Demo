using UnityEngine;

public class ExploreAction : GoapAction
{
	private bool done = false;

	public override bool checkProceduralPrecondition(GameObject agent)
	{
		//todo make this more complex
		targetPosition = Random.insideUnitCircle.ToFlatVector3() * 80;

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