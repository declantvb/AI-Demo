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

	public List<BlackboardEntry> ReadAll()
	{
		return dict.Values.SelectMany(x => x).ToList();
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

	public void Write(string key, object data, float expiry)
	{
		var entry = new BlackboardEntry
		(
			key: key,
			data: data,
			expiry: expiry
		);

		Write(entry);
	}

	public void Write(BlackboardEntry entry)
	{
		if (!dict.ContainsKey(entry.Key))
		{
			dict[entry.Key] = new List<BlackboardEntry>();
		}

		var valueList = dict[entry.Key];
		var existing = valueList.FirstOrDefault(x => x.Data == entry.Data);

		if (existing == null)
		{
			valueList.Add(entry);
		}
		else
		{
			existing.ExpiryTime = entry.ExpiryTime;
		}
	}

	public void Update()
	{
		foreach (var valueSet in dict.Values)
		{
			for (int i = 0; i < valueSet.Count; i++)
			{
				var value = valueSet[i];

				if ((value.Data is Object && value.Data as Object == null) || 
					value.ExpiryTime > 0 && value.ExpiryTime < Time.time)
				{
					valueSet.Remove(value);
				}
			}
		}
	}

	public static class Keys
	{
		public const string Enemy = "enemy";
		public const string Ally = "ally";
		public const string RepairStation = "repair station";
		public const string NeedRepair = "need repair";
	}
}

// mostly immutable
public class BlackboardEntry
{
	public string Key { get; private set; }
	public object Data { get; private set; }
	public float ExpiryTime { get; set; }

	public BlackboardEntry(string key, object data, float expiry)
	{
		Key = key;
		Data = data;
		ExpiryTime = expiry;
	}
}