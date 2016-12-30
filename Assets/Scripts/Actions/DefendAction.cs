using UnityEngine;

public class DefendAction : GoapAction
{
	private bool done = false;

	public DefendAction()
	{
		addPrecondition(GoapKeys.ShouldDefend, true);
		addEffect(GoapKeys.Defending, true);
	}

	public override bool checkProceduralPrecondition(GameObject agent)
	{
		return true;
	}

	public override bool isDone()
	{
		return done;
	}

	public override bool perform(GameObject agent)
	{
		return true;
	}

	public override bool requiresInRange()
	{
		return false;
	}

	public override void reset()
	{
		done = false;
	}
}