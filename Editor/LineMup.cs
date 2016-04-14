using UnityEditor;
using UnityEngine;

namespace Spewnity
{
	public class LineMup: MonoBehaviour
	{
		// align in the x translation axis
		[MenuItem ("Spewnity/Align/Translation X")]
		static void AlignmentTransX()
		{
			// execute alignment for the x axis
			AlignOrDistribute(false, "transX");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Align/Translation X", true)]
		static bool ValidateAlignmentTransX()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// align in the y translation axis
		[MenuItem ("Spewnity/Align/Translation Y")]
		static void AlignmentTransY()
		{
			// execute alignment for the y axis
			AlignOrDistribute(false, "transY");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Align/Translation Y", true)]
		static bool ValidateAlignmentTransY()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// align in the z translation axis
		[MenuItem ("Spewnity/Align/Translation Z")]
		static void AlignmentTransZ()
		{
			// execute alignment for the z axis
			AlignOrDistribute(false, "transZ");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Align/Translation Z", true)]
		static bool ValidateAlignmentTransZ()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// align the rotation
		[MenuItem ("Spewnity/Align/Rotation")]
		static void AlignmentRotation()
		{
			// execute alignment in all axes
			AlignOrDistribute(false, "rotAll");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Align/Rotation", true)]
		static bool ValidateAlignmentRotation()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// align in the x scale axis
		[MenuItem ("Spewnity/Align/Scale X")]
		static void AlignmentScaleX()
		{
			// execute alignment for the x axis
			AlignOrDistribute(false, "scaleX");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Align/Scale X", true)]
		static bool ValidateAlignmentScaleX()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// align in the y scale axis
		[MenuItem ("Spewnity/Align/Scale Y")]
		static void AlignmentScaleY()
		{
			// execute alignment for the y axis
			AlignOrDistribute(false, "scaleY");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Align/Scale Y", true)]
		static bool ValidateAlignmentScaleY()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// align in the z scale axis
		[MenuItem ("Spewnity/Align/Scale Z")]
		static void AlignmentScaleZ()
		{
			// execute alignment for the z axis
			AlignOrDistribute(false, "scaleZ");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Align/Scale Z", true)]
		static bool ValidateAlignmentScaleZ()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// distribute in the x translation axis
		[MenuItem ("Spewnity/Distribute/Translation X")]
		static void DistributeTransX()
		{
			// execute distribution for the x axis
			AlignOrDistribute(true, "transX");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Distribute/Translation X", true)]
		static bool ValidateDistributeTransX()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// distribute in the y translation axis
		[MenuItem ("Spewnity/Distribute/Translation Y")]
		static void DistributeTransY()
		{
			// execute distribution for the y axis
			AlignOrDistribute(true, "transY");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Distribute/Translation Y", true)]
		static bool ValidateDistributeTransY()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// distribute in the z translation axis
		[MenuItem ("Spewnity/Distribute/Translation Z")]
		static void DistributeTransZ()
		{
			// execute distribution for the z axis
			AlignOrDistribute(true, "transZ");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Distribute/Translation Z", true)]
		static bool ValidateDistributeTransZ()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// distribute the rotation
		[MenuItem ("Spewnity/Distribute/Rotation")]
		static void DistributeRotation()
		{
			// execute distribution in all axes
			AlignOrDistribute(true, "rotAll");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Distribute/Rotation", true)]
		static bool ValidateDistributeRotation()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// distribute in the x scale axis
		[MenuItem ("Spewnity/Distribute/Scale X")]
		static void DistributeScaleX()
		{
			// execute distribution for the x axis
			AlignOrDistribute(true, "scaleX");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Distribute/Scale X", true)]
		static bool ValidateDistributeScaleX()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// distribute in the y scale axis
		[MenuItem ("Spewnity/Distribute/Scale Y")]
		static void DistributeScaleY()
		{
			// execute distribution for the y axis
			AlignOrDistribute(true, "scaleY");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Distribute/Scale Y", true)]
		static bool ValidateDistributeScaleY()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		// distribute in the z scale axis
		[MenuItem ("Spewnity/Distribute/Scale Z")]
		static void DistributeScaleZ()
		{
			// execute distribution for the z axis
			AlignOrDistribute(true, "scaleZ");
		}
	
		// determine if the void can be executed.
		[MenuItem ("Spewnity/Distribute/Scale Z", true)]
		static bool ValidateDistributeScaleZ()
		{
			// only return true if there is a transform in the selection.
			return (Selection.activeTransform != null);
		}
	
		static void AlignOrDistribute(bool shouldDist, string theAxis)
		{
		
			// create some variables to store values
			Transform firstObj = Selection.activeTransform;
			Transform furthestObj = firstObj;
			float firstVal = 0.0f;
			float furthestVal = 0.0f;
			float curDist = 0.0f;
			float lastDist = 0.0f;
			int selCount = 0;
		
			// collect the number of tranforms in the selection and find the object that is furthest away from the active selected object
			foreach(Transform transform in Selection.transforms)
			{
				// collect the current distance
				curDist = Vector3.Distance(firstObj.position, transform.position);
			
				// get the object with the greatest distance from the first selected object
				if(curDist > lastDist)
				{
					furthestObj = transform;
					lastDist = curDist;
				}
			
				// increment count
				selCount += 1;
			}
		
			// distribute or align?
			if(shouldDist)
			{
				// collect the first value and furthest value to distribute between
				switch(theAxis)
				{
					case "transX":
						firstVal = firstObj.position.x;
						furthestVal = furthestObj.position.x;
						break;
					case "transY":
						firstVal = firstObj.position.y;
						furthestVal = furthestObj.position.y;
						break;
					case "transZ":
						firstVal = firstObj.position.z;
						furthestVal = furthestObj.position.z;
						break;
					case "scaleX":
						firstVal = firstObj.localScale.x;
						furthestVal = furthestObj.localScale.x;
						break;
					case "scaleY":
						firstVal = firstObj.localScale.y;
						furthestVal = furthestObj.localScale.y;
						break;
					case "scaleZ":
						firstVal = firstObj.localScale.z;
						furthestVal = furthestObj.localScale.z;
						break;
					default:
						break;
				}	
			
				// calculate the spacing for the distribution
				float objSpacing = (firstVal - furthestVal) / (selCount - 1);
				float curSpacing = objSpacing;
				float rotSpacing = 1.0f / (selCount - 1);
				float curRotSpacing = rotSpacing;
			
				// update every object in the selection to distribute evenly
				foreach(Transform transform in Selection.transforms)
				{
					switch(theAxis)
					{
						case "transX":
							if((transform != firstObj) && (transform != furthestObj))
							{
								transform.position = new Vector3(firstVal - curSpacing, transform.position.y, transform.position.z);
								curSpacing += objSpacing;
							}
							break;
						case "transY":
							if((transform != firstObj) && (transform != furthestObj))
							{					
								transform.position = new Vector3(transform.position.x, firstVal - curSpacing, transform.position.z);
								curSpacing += objSpacing;
							}
							break;
						case "transZ":
							if((transform != firstObj) && (transform != furthestObj))
							{
								transform.position = new Vector3(transform.position.x, transform.position.y, firstVal - curSpacing);
								curSpacing += objSpacing;
							}
							break;
						case "rotAll":
							if((transform != firstObj) && (transform != furthestObj))
							{
								transform.rotation = Quaternion.Slerp(firstObj.rotation, furthestObj.rotation, curRotSpacing);
								curRotSpacing += rotSpacing;
							}
							break;
						case "scaleX":
							if((transform != firstObj) && (transform != furthestObj))
							{
								transform.localScale = new Vector3(firstVal - curSpacing, transform.localScale.y, transform.localScale.z);
								curSpacing += objSpacing;
							}
							break;
						case "scaleY":
							if((transform != firstObj) && (transform != furthestObj))
							{
								transform.localScale = new Vector3(transform.localScale.x, firstVal - curSpacing, transform.localScale.z);
								curSpacing += objSpacing;
							}
							break;
						case "scaleZ":
							if((transform != firstObj) && (transform != furthestObj))
							{
								transform.localScale = new Vector3(transform.localScale.z, transform.localScale.y, firstVal - curSpacing);
								curSpacing += objSpacing;
							}
							break;
						default:
							break;
					}
				}
			}
			else
			{	
				// snap every object in the selection to the first objects value
				foreach(Transform transform in Selection.transforms)
				{
					switch(theAxis)
					{
						case "transX":
							transform.position = new Vector3(firstObj.position.x, transform.position.y, transform.position.z);
							break;
						case "transY":
							transform.position = new Vector3(transform.position.x, firstObj.position.y, transform.position.z);
							break;
						case "transZ":
							transform.position = new Vector3(transform.position.x, transform.position.y, firstObj.position.z);
							break;
						case "rotAll":
							transform.rotation = firstObj.rotation;
							break;
						case "scaleX":
							transform.localScale = new Vector3(firstObj.localScale.x, transform.localScale.y, transform.localScale.z);
							break;
						case "scaleY":
							transform.localScale = new Vector3(transform.localScale.x, firstObj.localScale.y, transform.localScale.z);
							break;
						case "scaleZ":
							transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, firstObj.localScale.z);
							break;
						default:
							break;
					}
				}
			}
		}
	}
}