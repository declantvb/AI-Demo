using UnityEngine;

public class Vehicle : MonoBehaviour
{
	public float AutoAttackRange;
	public Squad Squad;

	private Order currentOrder;

	private Mover mover;
	private Shooter shooter;
	private Sensor sensor;

	private void Start()
	{
		mover = GetComponent<Mover>();
		shooter = GetComponent<Shooter>();
		sensor = GetComponent<Sensor>();
	}

	private void Update()
	{
		if (sensor.ClosestEnemy != null && sensor.ClosestEnemy.Transform != null && Vector3.Distance(transform.position, sensor.ClosestEnemy.Transform.position) < AutoAttackRange)
		{
			//todo detect already distracted

			DoOrder(new Order
			{
				Type = Order.Types.Attack,
				Target = sensor.ClosestEnemy.Transform,
				IsDone = () => sensor.ClosestEnemy.Transform == null
			});
		}

		// update order
		if (currentOrder != null && currentOrder.IsDone())
		{
			currentOrder = null;
			shooter.OrderTarget = null;
			mover.OrderTarget = Vector3.zero;
			mover.OrderTargetRange = 0f;
			mover.MoveType = MoveType.None;
		}

		// update squad
		foreach (var enemy in sensor.Enemies)
		{
			Squad.blackboard.Store(Blackboard.Keys.Enemy, enemy.Transform, Time.time + Squad.blackboard.ExpiryTime);
		}
	}

	public void UpdateOrder(Order order)
	{
		currentOrder = order;

		DoOrder(currentOrder);
	}

	public void DoOrder(Order order)
	{
		switch (order.Type)
		{
			case Order.Types.Attack:
				Attack(order.Target);
				break;

			case Order.Types.Move:
				Move(order.Target);
				break;

			default:
				Debug.LogWarning("bad order type");
				break;
		}
	}

	private void Attack(object target)
	{
		var trans = target as Transform;
		if (trans != null)
		{
			shooter.OrderTarget = trans;
			mover.OrderTarget = trans.position;
			mover.OrderTargetRange = shooter.Range;
			mover.MoveType = MoveType.Move;
		}
		else
		{
			Debug.LogError("bad Attack target");
		}
	}

	private void Move(object target)
	{
		var vec = target as Vector3?;
		if (vec != null)
		{
			shooter.OrderTarget = null;
			mover.OrderTarget = vec.Value;
			mover.OrderTargetRange = 0f;
			mover.MoveType = MoveType.Move;
		}
		else
		{
			Debug.LogError("bad Move target");
		}
	}
}