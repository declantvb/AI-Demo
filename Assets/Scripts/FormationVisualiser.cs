using UnityEngine;

public class FormationVisualiser : MonoBehaviour
{
	public int NumberToDraw;
	public float Distance;
	private Formation formation;

	private void Start()
	{
		formation = new SkirmishFormation();
	}

	private void OnDrawGizmos()
	{
		var offsets = formation.GetAll(NumberToDraw);

		foreach (var offset in offsets)
		{
			Gizmos.DrawSphere(transform.position + offset * Distance, 1f);
		}
	}
}