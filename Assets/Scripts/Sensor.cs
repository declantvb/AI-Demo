using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sensor : MonoBehaviour
{
	public float SensorRadius = 20;
	public float DecayTime = 5;
	public List<SensedThing> SensedThings = new List<SensedThing>();

	public List<SensedThing> Enemies { get { return SensedThings.Where(x => x.Transform != null && IsEnemy(x)).ToList(); } }
	public List<SensedThing> Allies { get { return SensedThings.Where(x => x.Transform != null && IsAlly(x)).ToList(); } }

	public SensedThing ClosestEnemy
	{
		get
		{
			return Enemies.OrderBy(x => Vector3.Distance(transform.position, x.Transform.position)).FirstOrDefault();
		}
	}

	private Vehicle vehicle;

	public void Start()
	{
		vehicle = GetComponent<Vehicle>();
	}

	private void Update()
	{
		var seen = Physics.OverlapSphere(transform.position, SensorRadius);

		foreach (var collider in seen)
		{
			var seenVehicle = collider.GetComponent<Vehicle>();

			if (seenVehicle == null || seenVehicle == vehicle)
			{
				continue;
			}

			var exist = SensedThings.FirstOrDefault(x => x.Transform == seenVehicle.transform);
			if (exist == null)
			{
				SensedThings.Add(new SensedThing
				{
					Transform = seenVehicle.transform,
					DecayTime = Time.time + DecayTime
				});
			}
			else
			{
				exist.DecayTime = Time.time + DecayTime;
			}
		}

		for (int i = 0; i < Enemies.Count; i++)
		{
			var enemy = Enemies[i];

			if (enemy.DecayTime < Time.time)
			{
				Enemies.Remove(enemy);
			}
		}

		for (int i = 0; i < Allies.Count; i++)
		{
			var enemy = Allies[i];

			if (enemy.DecayTime < Time.time)
			{
				Allies.Remove(enemy);
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		foreach (var enemy in Enemies)
		{
			if (enemy.Transform != null)
			{
				Gizmos.DrawLine(transform.position, enemy.Transform.position);
			}
		}
		Gizmos.color = Color.green;
		foreach (var enemy in Allies)
		{
			if (enemy.Transform != null)
			{
				Gizmos.DrawLine(transform.position, enemy.Transform.position);
			}
		}
		Gizmos.color = Color.white;
	}

	private bool IsEnemy(SensedThing x)
	{
		var other = x.Transform.GetComponent<Vehicle>();
		return other != null && other.Faction != vehicle.Faction;
	}

	private bool IsAlly(SensedThing x)
	{
		var other = x.Transform.GetComponent<Vehicle>();
		return other != null && other.Faction == vehicle.Faction;
	}
}

public class SensedThing
{
	public Transform Transform;
	public float DecayTime;
}