using System.Linq;
using UnityEngine;

public class Squad : MonoBehaviour
{
	public Blackboard blackboard = new Blackboard();
	public Vehicle[] Members;
	public Vector3 AveragePosition
	{
		get
		{
			return Members.Select(x => x.transform.position).Aggregate((acc, val) => acc + val) / Members.Length;
		}
	}

	private Order currentOrder;

	private void Start()
	{
		foreach (var member in Members)
		{
			member.Squad = this;
		}
	}

	private void Update()
	{
		blackboard.Update(Time.time);

		var enemies = blackboard.Get(Blackboard.Keys.Enemy);
		if (enemies.Any())
		{
			var closest = enemies.Select(x => x.Data as Transform).OrderBy(x => x == null ? float.MaxValue : Vector3.Distance(AveragePosition, x.position)).FirstOrDefault();

			if (closest != null)
			{
				DoOrder(new Order
				{
					Type = Order.Types.Attack,
					Target = closest,
					IsDone = () => closest == null
				});
			}
		}
		
		// update order
		if (currentOrder != null && currentOrder.IsDone())
		{
			currentOrder = null;
		}
	}

	public void UpdateOrder(Order order)
	{
		currentOrder = order;

		DoOrder(currentOrder);
	}

	public void DoOrder(Order order)
	{
		// basic passthrough for now
		foreach (var member in Members)
		{
			member.UpdateOrder(order);
		}
	}
}