using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sensor : MonoBehaviour
{
	public float SensorRadius = 20;
	public float DecayTime = 5;
	public List<SensedThing> Enemies = new List<SensedThing>();
	public List<SensedThing> Allies = new List<SensedThing>();

	public SensedThing ClosestEnemy
	{
		get
		{
			return Enemies.OrderBy(x => x.Transform == null ? float.MaxValue : Vector3.Distance(transform.position, x.Transform.position)).FirstOrDefault();
		}
	}

	private void Update()
	{
		var seen = Physics.OverlapSphere(transform.position, SensorRadius);
		var thisVehicle = GetComponent<Vehicle>();

		foreach (var collider in seen)
		{
			var enemy = collider.GetComponent<Enemy>();

			if (enemy != null)
			{
				var exist = Enemies.FirstOrDefault(x => x.Transform == enemy.transform);
				if (exist == null)
				{
					Enemies.Add(new SensedThing
					{
						Transform = enemy.transform,
						DecayTime = Time.time + DecayTime
					});
				}
				else
				{
					exist.DecayTime = Time.time + DecayTime;
				}
			}

			var ally = collider.GetComponent<Vehicle>();

			if (ally != null && ally != thisVehicle)
			{

				var exist = Allies.FirstOrDefault(x => x.Transform == ally.transform);
				if (exist == null)
				{
					Allies.Add(new SensedThing
					{
						Transform = ally.transform,
						DecayTime = Time.time + DecayTime
					});
				}
				else
				{
					exist.DecayTime = Time.time + DecayTime;
				}
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
}

public class SensedThing
{
	public Transform Transform;
	public float DecayTime;
}