using UnityEngine;

public class MouseController : MonoBehaviour
{
	private Fireteam Fireteam;
	private int formationIndex = 0;
	private Formation[] formations;
	private bool guiClick = false;

	// Use this for initialization
	private void Start()
	{
		Fireteam = GetComponent<Fireteam>();

		formations = new Formation[]
		{
			new WedgeFormation(),
			new LineFormation(),
			new VeeFormation()
		};
	}

	// Update is called once per frame
	private void Update()
	{
		if (Input.GetMouseButtonDown(0) & !guiClick)
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit))
			{
				var vehicle = hit.collider.GetComponentInParent<Vehicle>();
				if (vehicle != null && vehicle.Faction != "Allies")
				{
					Fireteam.UpdateOrder(new FireteamOrder
					{
						Type = FireteamOrderType.Assault,
						Target = vehicle.transform,
						IsDone = () => vehicle == null
					});
				}
				else
				{
					var point = hit.point;
					point.y = 0;
					Fireteam.UpdateOrder(new FireteamOrder
					{
						Type = FireteamOrderType.Defend,
						TargetPosition = point,
						IsDone = () => Vector3.Distance(Fireteam.AveragePosition(), point) < 10f
					});
				}
			}
		}
	}

	public void OnGUI()
	{
		var rect = new Rect(10, 10, 150, 25);
		if (GUI.Button(rect, "Change Formation"))
		{
			formationIndex = (formationIndex + 1) % formations.Length;
			Fireteam.ChangeFormation(formations[formationIndex]);
		}
		guiClick = rect.Contains(Event.current.mousePosition);
	}
}