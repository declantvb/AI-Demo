using UnityEngine;

public class Shooter : MonoBehaviour
{
	public float Range;
	public float Damage;

	public Transform Target = null;
	public bool AttackOnSight = true;

	private void Update()
	{
		if (Target != null && Vector3.Distance(transform.position, Target.position) < Range)
		{
			var health = Target.GetComponent<Health>();

			if (health != null)
			{
				health.CurrentHealth -= Damage * Time.deltaTime;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (Target != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, Target.transform.position);
			Gizmos.color = Color.white;
		}
	}

	public void Reset()
	{
		Target = null;
		AttackOnSight = true;
	}
}