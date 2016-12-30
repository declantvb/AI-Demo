using UnityEngine;

public class Shooter : MonoBehaviour
{
	public float Range;
	public float Damage;

	public Transform OrderTarget = null;
	public bool AttackOnSight = true;

	private void Update()
	{
		if (OrderTarget != null && Vector3.Distance(transform.position, OrderTarget.position) < Range)
		{
			var health = OrderTarget.GetComponent<Health>();

			if (health != null)
			{
				health.CurrentHealth -= Damage * Time.deltaTime;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (OrderTarget != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, OrderTarget.transform.position);
			Gizmos.color = Color.white;
		}
	}
}