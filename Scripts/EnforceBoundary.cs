using UnityEngine;
using System.Collections;

namespace Spewnity
{
	public class EnforceBoundary : MonoBehaviour
	{
		public Boundary[] boundaries;

		public void Update()
		{
			if(!transform.hasChanged) return;
			transform.hasChanged = true;

			foreach(Boundary b in boundaries)
			{
				switch(b.side)
				{
					case BoundarySide.MinX:
						if(transform.position.x < b.value)
						{
							Vector3 pos = transform.position;
							pos.x = b.value;
							transform.position = pos;
						}
						break;
					case BoundarySide.MaxX:
						if(transform.position.x > b.value)
						{
							Vector3 pos = transform.position;
							pos.x = b.value;
							transform.position = pos;
						}
						break;
					case BoundarySide.MinY:
						if(transform.position.y < b.value)
						{
							Vector3 pos = transform.position;
							pos.y = b.value;
							transform.position = pos;
						}
						break;
					case BoundarySide.MaxY:
						if(transform.position.y > b.value)
						{
							Vector3 pos = transform.position;
							pos.y = b.value;
							transform.position = pos;
						}
						break;
				}
			}
		}
	}

	[System.Serializable]
	public struct Boundary
	{
		public BoundarySide side;
		public float value;
	}

	public enum BoundarySide
	{
		MinX,
		MaxX,
		MinY,
		MaxY
	}
}