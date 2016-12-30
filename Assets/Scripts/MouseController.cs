using UnityEngine;

public class MouseController : MonoBehaviour
{
	private Fireteam Fireteam;

	// Use this for initialization
	private void Start()
	{
		Fireteam = GetComponent<Fireteam>();
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
				var vehicle = hit.collider.GetComponent<Vehicle>();
				if (vehicle != null && vehicle.Faction != "Allies")
				{
					Fireteam.UpdateOrder(new FireteamOrder
					{
						Type = FireteamOrder.Types.Assault,
						Target = hit.collider.transform.position,
						IsDone = () => hit.collider.transform == null
					});
				}
				else
				{
					var point = hit.point;
					point.y = 0;
					Fireteam.UpdateOrder(new FireteamOrder
					{
						Type = FireteamOrder.Types.Scout,
						Target = point,
						IsDone = () => Vector3.Distance(Fireteam.AveragePosition, hit.point) < 10f
					});
				}
			}
		}
	}
}