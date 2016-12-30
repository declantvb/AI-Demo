using System.Linq;
using UnityEngine;

public class Fireteam : MonoBehaviour
{
	public float MaxScoutDistance;
	public float MaxActionDistance;
	public float MaxAttackEnemyDistance;
	public float WanderDistance;
	public float WanderError;
	public float WanderWaitTimeMin;
	public float WanderWaitTimeMax;

	public Blackboard blackboard = new Blackboard();
	public Vehicle[] Members;

	public Vector3 AveragePosition
	{
		get
		{
			return Members
				.Where(x => x != null)
				.Select(x => x.transform.position)
				.Aggregate((acc, val) => acc + val) / Members.Length;
		}
	}

	private FireteamOrder currentOrder;
	private bool attacking;

	private void Start()
	{
		foreach (var member in Members)
		{
			member.Fireteam = this;
		}
	}

	private void Update()
	{
		blackboard.Update(Time.time);

		if (Members.All(x => x == null))
		{
			return;
		}

		var enemies = blackboard.Get(Blackboard.Keys.Enemy);
		if (enemies.Any())
		{
			var closest = enemies.Select(x => x.Data as Transform).OrderBy(x => x == null ? float.MaxValue : Vector3.Distance(AveragePosition, x.position)).FirstOrDefault();

			if (closest != null)
			{
				DoOrder(new FireteamOrder
				{
					Type = FireteamOrder.Types.Assault,
					Target = closest.position,
					IsDone = () => closest == null
				});

				attacking = true;
			}
		}
		else
		{
			attacking = false;
		}

		// update order
		if (currentOrder != null && currentOrder.IsDone())
		{
			currentOrder = null;
		}

		if (currentOrder == null && !attacking)
		{
			//wander around current area
			var pos = AveragePosition;

			var fullCircle = Mathf.PI * 2;

			var angleBetween = fullCircle / Members.Length;
			var i = 0;
			foreach (var member in Members)
			{
				if (member.currentOrder == null || member.currentOrder.Type != VehicleOrder.Types.Wander)
				{
					var offset = Quaternion.AngleAxis(angleBetween * i * Mathf.Rad2Deg, Vector3.up) * Vector3.forward * WanderDistance;
					var rand = Random.insideUnitCircle * WanderError;
					var waitTime = Time.time + Random.value * (WanderWaitTimeMax - WanderWaitTimeMin) + WanderWaitTimeMin;
					member.UpdateOrder(new VehicleOrder
					{
						Type = VehicleOrder.Types.Wander,
						Target = pos + offset + new Vector3(rand.x, 0, rand.y),
						IsDone = x => Time.time > waitTime
					});
				}

				i++;
			}
		}
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
}