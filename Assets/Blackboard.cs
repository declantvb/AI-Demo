using System.Collections.Generic;

public class Blackboard
{
	public float ExpiryTime = 5f;

	private Dictionary<string, List<BlackboardEntry>> dict = new Dictionary<string, List<BlackboardEntry>>();

	public List<BlackboardEntry> Get(string key)
	{
		if (dict.ContainsKey(key))
		{
			return dict[key];
		}

		return new List<BlackboardEntry>();
	}

	public void Store(string key, object value, float expiry)
	{
		var entry = new BlackboardEntry
		{
			Key = key,
			Type = value.GetType().Name,
			Data = value,
			ExpiryTime = expiry
		};

		if (!dict.ContainsKey(key))
		{
			dict[key] = new List<BlackboardEntry>();
		}

		dict[key].Add(entry);
	}

	public void Update(float time)
	{
		foreach (var valueSet in dict.Values)
		{
			for (int i = 0; i < valueSet.Count; i++)
			{
				var value = valueSet[i];

				if (value.ExpiryTime > 0 && value.ExpiryTime < time)
				{
					valueSet.Remove(value);
				}
			}
		}
	}

	public class Keys
	{
		public static string Enemy = "Enemy";
	}
}

public class BlackboardEntry
{
	public string Key;
	public string Type;
	public object Data;
	public float ExpiryTime;
}