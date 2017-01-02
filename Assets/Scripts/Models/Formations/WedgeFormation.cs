using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WedgeFormation : Formation
{
	public override IEnumerator<Vector3> GetEnumerator()
	{
		var row = 0;
		var pos = 0;
		var right = true;
		while (row < int.MaxValue)
		{
			if (pos == 0)
			{
				yield return Vector3.zero + (Vector3.back * row);
				row++;
				pos = row;
			}
			else if (right)
			{
				yield return (Vector3.back * row) + (Vector3.right * pos);
				right = false;
			}
			else
			{
				yield return (Vector3.back * row) + (Vector3.left * pos);
				right = true;
				pos--;
			}
		}
	}
}