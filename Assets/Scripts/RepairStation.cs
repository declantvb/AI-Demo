using UnityEngine;

public class RepairStation : MonoBehaviour
{
	public string Faction;
	public float RepairPerSecond;
	public float RepairRadius;

	private void Update()
	{
		var nearby = Physics.OverlapSphere(transform.position, RepairRadius);

		foreach (var collider in nearby)
		{
			var vehicle = collider.GetComponent<Vehicle>();
			if (vehicle != null && vehicle.Faction == Faction)
			{
				var health = vehicle.GetComponent<Health>();

				if (health == null)
				{
					Debug.LogError("vehicle missing health component");
				}

				var healthLost = health.MaxHealth - health.CurrentHealth;

				health.CurrentHealth += Mathf.Min(RepairPerSecond * Time.deltaTime, healthLost);
			}
		}
	}
}