using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EchelonLeftFormation : Formation
{
	public override IEnumerator<Vector3> GetEnumerator()
	{
		var row = 0;
		while (row < int.MaxValue)
		{
			if (row == 0)
			{
				yield return Vector3.zero;
				row++;
			}
			else
			{
				yield return (Vector3.back + Vector3.left) * row;
				row++;
			}
		}
	}
}