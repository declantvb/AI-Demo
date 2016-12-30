using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Blackboard : MonoBehaviour
{
	private Dictionary<string, List<BlackboardEntry>> dict = new Dictionary<string, List<BlackboardEntry>>();

	public List<BlackboardEntry> Read(string key)
	{
		if (dict.ContainsKey(key))
		{
			return dict[key];
		}

		return new List<BlackboardEntry>();
	}

	public List<T> Read<T>(string key) where T : class
	{
		if (dict.ContainsKey(key))
		{
			var values = dict[key];

			// need to do this, as Unity nulls things when they are destroyed
			if (!values.All(x => x.Data is T))
			{
				Debug.LogWarning("tried to retrieve " + typeof(T).Name);
				return new List<T>();
			}

			return values.Select(x => x.Data as T).ToList();
		}

		return new List<T>();
	}

	public void Write(string key, object value, float expiry)
	{
		var entry = new BlackboardEntry
		{
			Key = key,
			Type = value.GetType().Name,
			Data = value,
			ExpiryTime = expiry
		};

		Write(entry);
	}

	public void Write(BlackboardEntry entry)
	{
		if (!dict.ContainsKey(entry.Key))
		{
			dict[entry.Key] = new List<BlackboardEntry>();
		}

		var valueList = dict[entry.Key];
		if (!valueList.Any(x=>x.Data == entry.Data))
		{
			valueList.Add(entry);
		}
	}

	public void Update()
	{
		foreach (var valueSet in dict.Values)
		{
			for (int i = 0; i < valueSet.Count; i++)
			{
				var value = valueSet[i];

				if (value.Data == null || value.ExpiryTime > 0 && value.ExpiryTime < Time.time)
				{
					valueSet.Remove(value);
				}
			}
		}
	}

	public static class Keys
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