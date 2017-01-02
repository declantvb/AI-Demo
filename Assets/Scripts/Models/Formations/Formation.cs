using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Formation
{
	public float SeparationDistance = 0;
	public float CorrectionAggressiveness = 0.8f;
	public float CorrectionSpeed = 0;

	public Vector3 Get(int index)
	{
		var enumerator = GetEnumerator();

		for (int i = 0; i < index + 1; i++)
		{
			if (!enumerator.MoveNext())
			{
				Debug.LogError("formation error");
				return Vector3.zero;
			}
		}

		return enumerator.Current * SeparationDistance;
	}

	public Vector3[] GetAll(int count)
	{
		var enumerator = GetEnumerator();

		return Enumerable.Range(0, count).Select(x =>
		{
			if (!enumerator.MoveNext())
			{
				Debug.LogError("formation error - all");
				return Vector3.zero;
			}
			return enumerator.Current * SeparationDistance;
		}).ToArray();
	}

	public float SeparationCorrection(Vector3[] testPoints)
	{
		var count = testPoints.Length;
		var ideal = GetAll(count);

		var separation = 0f;

		for (int i = 0; i < count; i++)
		{
			var expected = ideal[i];
			var actual = testPoints[i];

			separation += Vector3.Distance(expected, actual);
		}

		return Mathf.Pow(CorrectionAggressiveness, separation * CorrectionSpeed);
	}

	public abstract IEnumerator<Vector3> GetEnumerator();
}