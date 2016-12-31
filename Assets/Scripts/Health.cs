using UnityEngine;

public class Health : MonoBehaviour
{
	public float MaxHealth = 100;
	public float CurrentHealth;

	[Header("Healthbar Display")]
	public bool DisplayInWorld = true;

	public float healthbarWorldOffset = 3f;
	public float healthbarHeight = 18;
	public float healthbarWidth = 70;
	public float screenspaceOffsetHeight = 50;

	private GUIStyle HealthBarBackground;
	private GUIStyle HealthBarForeground;

	public float Decimal { get { return CurrentHealth / MaxHealth; } }
	public float Percent { get { return CurrentHealth / MaxHealth * 100; } }

	// Use this for initialization
	private void Start()
	{
		CurrentHealth = MaxHealth;

		var redTex = new Texture2D(1, 1);
		redTex.SetPixel(0, 0, Color.red);
		redTex.Apply();
		redTex.wrapMode = TextureWrapMode.Repeat;
		HealthBarBackground = new GUIStyle();
		HealthBarBackground.normal.background = redTex;

		var greenTex = new Texture2D(1, 1);
		greenTex.SetPixel(0, 0, Color.green);
		greenTex.Apply();
		greenTex.wrapMode = TextureWrapMode.Repeat;
		HealthBarForeground = new GUIStyle();
		HealthBarForeground.normal.background = greenTex;
	}

	public void Update()
	{
		if (CurrentHealth <= 0)
		{
			Destroy(gameObject);
		}
	}

	public void OnGUI()
	{
		//show healthbar
		if (DisplayInWorld)
		{
			var screenHeight = Camera.main.pixelHeight;

			var pos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, healthbarWorldOffset, 0));
			var heading = transform.position - Camera.main.transform.position;
			if (Vector3.Dot(Camera.main.transform.forward, heading) > 0)
			{
				var healthPercent = CurrentHealth / MaxHealth;

				var hbPos = new Vector2(pos.x - healthbarWidth / 2, screenHeight - pos.y - screenspaceOffsetHeight);
				var hbSize = new Vector2(healthbarWidth, healthbarHeight);
				GUI.depth = 0;
				GUI.BeginGroup(new Rect(hbPos.x, hbPos.y, hbSize.x, hbSize.y));
				GUI.Box(new Rect(0, 0, hbSize.x, hbSize.y), "", HealthBarBackground);
				GUI.BeginGroup(new Rect(0, 0, hbSize.x * healthPercent, hbSize.y));
				GUI.Box(new Rect(0, 0, hbSize.x, hbSize.y), "", HealthBarForeground);
				GUI.EndGroup();
				GUI.EndGroup();
			}
		}
	}
}