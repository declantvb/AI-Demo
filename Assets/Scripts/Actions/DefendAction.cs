using UnityEngine;

public class DefendAction : GoapAction
{
	private bool done = false;

	public DefendAction()
	{
		addEffect(GoapKeys.Defend, true);
	}

	public override bool checkProceduralPrecondition(GameObject agent)
	{
		var fireteam = agent.GetComponent<Fireteam>();
		if (fireteam != null && fireteam.currentOrder != null && fireteam.currentOrder.Type == FireteamOrderType.Defend)
		{
			if (fireteam.currentOrder.Target != null)
			{
				target = fireteam.currentOrder.Target;
			}
			else
			{
				targetPosition = fireteam.currentOrder.TargetPosition;
			}
			return true;
		}

		return false;
	}

	public override bool isDone()
	{
		return done;
	}

	public override bool perform(GameObject agent)
	{
		//when is this done?
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