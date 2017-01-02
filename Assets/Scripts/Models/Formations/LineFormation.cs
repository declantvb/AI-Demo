using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineFormation : Formation
{
	public override IEnumerator<Vector3> GetEnumerator()
	{
		var row = 0;
		var right = true;
		while (row < int.MaxValue)
		{
			if (row == 0)
			{
				yield return Vector3.zero;
				row++;
			}
			else if (right)
			{
				yield return Vector3.right * row;
				right = false;
			}
			else
			{
				yield return Vector3.left * row;
				right = true;
				row++;
			}
		}
	}
}