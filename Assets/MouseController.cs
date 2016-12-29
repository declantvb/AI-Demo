using UnityEngine;

public class MouseController : MonoBehaviour
{
	private Squad squad;

	// Use this for initialization
	private void Start()
	{
		squad = GetComponent<Squad>();
	}

	// Update is called once per frame
	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit))
			{
				if (hit.collider.GetComponent<Enemy>() != null)
				{
					squad.UpdateOrder(new Order
					{
						Type = Order.Types.Attack,
						Target = hit.collider.transform,
						IsDone = () => hit.collider.transform == null
					});
				}
				else
				{
					squad.UpdateOrder(new Order
					{
						Type = Order.Types.Move,
						Target = hit.point,
						IsDone = () => Vector3.Distance(squad.AveragePosition, hit.point) < 5f
					});
				}
			}
		}
	}
}