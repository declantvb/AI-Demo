using UnityEngine;

public class Sensor : MonoBehaviour
{
	public float SensorRadius = 20;

	private Vehicle vehicle;
	private Blackboard blackboard;

	public void Start()
	{
		vehicle = GetComponent<Vehicle>();
		blackboard = GetComponent<Blackboard>();
	}

	private void Update()
	{
		var seen = Physics.OverlapSphere(transform.position, SensorRadius);

		foreach (var collider in seen)
		{
			var seenVehicle = collider.GetComponent<Vehicle>();

			if (seenVehicle != null)
			{
				if (seenVehicle == vehicle)
				{
					// it's-a me
					return;
				}

				if (seenVehicle.Faction == vehicle.Faction)
				{
					blackboard.Write("ally", seenVehicle.transform, Time.time + 5);
				}
				else
				{
					blackboard.Write("enemy", seenVehicle.transform, Time.time + 5);
				}
			}

			var seenRepairStation = collider.GetComponent<RepairStation>();

			if (seenRepairStation != null)
			{
				blackboard.Write("repair station", seenRepairStation.transform, Time.time + 60);
			}
		}
	}

	private void OnDrawGizmos()
	{
		//Gizmos.color = Color.yellow;
		//foreach (var enemy in blackboard.Get("enemy"))
		//{
		//	if (enemy.Transform != null)
		//	{
		//		Gizmos.DrawLine(transform.position, enemy.Transform.position);
		//	}
		//}
		//Gizmos.color = Color.green;
		//foreach (var enemy in Allies)
		//{
		//	if (enemy.Transform != null)
		//	{
		//		Gizmos.DrawLine(transform.position, enemy.Transform.position);
		//	}
		//}
		//Gizmos.color = Color.white;
	}
}