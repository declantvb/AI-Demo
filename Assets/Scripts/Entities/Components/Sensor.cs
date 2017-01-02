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
			var seenVehicle = collider.GetComponentInParent<Vehicle>();

			if (seenVehicle != null)
			{
				if (seenVehicle == vehicle)
				{
					// it's-a me
					continue;
				}

				if (seenVehicle.Faction == vehicle.Faction)
				{
					blackboard.Write(Blackboard.Keys.Ally, seenVehicle.transform, Time.time + 5);
				}
				else
				{
					blackboard.Write(Blackboard.Keys.Enemy, seenVehicle.transform, Time.time + 5);
				}
			}

			var seenRepairStation = collider.GetComponentInParent<RepairStation>();

			if (seenRepairStation != null && seenRepairStation.Faction == vehicle.Faction)
			{
				blackboard.Write(Blackboard.Keys.RepairStation, seenRepairStation.transform, Time.time + 600);
			}
		}
	}
}