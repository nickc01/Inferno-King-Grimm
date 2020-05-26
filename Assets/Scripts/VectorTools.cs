using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Extensions
{
	public static class VectorTools
	{
		public static Vector2 RotationToVector(this float angle)
		{
			var angleRads = angle * Mathf.Deg2Rad;

			return new Vector2(Mathf.Cos(angleRads),Mathf.Sin(angleRads));
		}

		public static Vector3 PolarToCartesian(float radiansAngle, float magnitude = 1f)
		{
			return new Vector3(Mathf.Cos(radiansAngle) * magnitude,Mathf.Sin(radiansAngle) * magnitude);
		}
	}
}
