using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Spewnity
{
	public class InputController : MonoBehaviour
	{
		public InputHandler[] inputHandlers;
		private Touch? lastTouch = null;

		void Update()
		{
			foreach(InputHandler handler in inputHandlers)
			{
				handler.inputEvent.keys = Input.inputString; // Use InputType.AnyKey to see when this changes

				switch(handler.type)
				{
					case InputType.AnyKey:
						if(Input.anyKeyDown)
							handler.callback.Invoke(handler.inputEvent);
						break;

					case InputType.KeyUp:
						if(Input.GetKeyUp(handler.keyCode))
							handler.callback.Invoke(handler.inputEvent);
						break;

					case InputType.KeyDown:
						if(Input.GetKeyDown(handler.keyCode))
							handler.callback.Invoke(handler.inputEvent);
						break;

					case InputType.KeyHeld:
						if(Input.GetKey(handler.keyCode))
							handler.callback.Invoke(handler.inputEvent);
						break;

					case InputType.ButtonUp:
						if(Input.GetButtonUp(handler.buttonName))
							handler.callback.Invoke(handler.inputEvent);
						break;

					case InputType.ButtonDown:
						if(Input.GetButtonDown(handler.buttonName))
							handler.callback.Invoke(handler.inputEvent);
						break;

					case InputType.ButtonHeld:
						if(Input.GetButton(handler.buttonName))
							handler.callback.Invoke(handler.inputEvent);
						break;

				// Detects a SINGLE touch going down - if multitouch, only initial touch is considered
					case InputType.TouchDown:
						if(lastTouch == null && Input.touchCount > 0)
						{
							handler.inputEvent.touch = Input.GetTouch(0);
							handler.callback.Invoke(handler.inputEvent);
						}
						break;
				
				// Detects a SINGLE touch coming up - if multitouch, only initial touch is considered
					case InputType.TouchUp:
						if(lastTouch != null)
							break;

						foreach(Touch touch in Input.touches)
							if(touch.fingerId == ((Touch) lastTouch).fingerId)
							{
								handler.inputEvent.touch = touch;
								handler.callback.Invoke(handler.inputEvent);
								break;
							}
						break;

				// Detects a SINGLE touch held - if multitouch, it's the oldest touch
				// Fires on first touch and all subsequent touches by same finger
					case InputType.TouchHeld:
						if(lastTouch == null)
						{
							if(Input.touchCount > 0)
							{ // Fire first touch found
								handler.inputEvent.touch = Input.GetTouch(0);
								handler.callback.Invoke(handler.inputEvent);
							}
						}
						else
						{ // Find last touch
							foreach(Touch touch in Input.touches)
								if(touch.fingerId == ((Touch) lastTouch).fingerId)
								{
									handler.inputEvent.touch = touch;
									handler.callback.Invoke(handler.inputEvent);
									break;
								}
						}
						break;

					case InputType.Axis:
						float axis1 = handler.axisOptions.raw ? Input.GetAxisRaw(handler.axisName1) : Input.GetAxis(handler.axisName1);
						if(handler.axisOptions.invertAxis1)
							axis1 = -axis1;
						if(handler.axisOptions.noRepeats && handler.inputEvent.axis.x != axis1)
							break;
						if(handler.axisOptions.noZeros && axis1 == 0 && handler.inputEvent.axis.y == 0)
							break;

						handler.inputEvent.axis.Set(axis1, 0);
						handler.callback.Invoke(handler.inputEvent);
						break;

					case InputType.DoubleAxis:
						float daxis1 = handler.axisOptions.raw ? Input.GetAxisRaw(handler.axisName1) : Input.GetAxis(handler.axisName1);
						float daxis2 = handler.axisOptions.raw ? Input.GetAxisRaw(handler.axisName2) : Input.GetAxis(handler.axisName2);
						if(handler.axisOptions.invertAxis1)
							daxis1 = -daxis1;
						if(handler.axisOptions.invertAxis2)
							daxis2 = -daxis2;
						if(handler.axisOptions.noDiagonals)
						{
							if(Mathf.Abs(daxis1) > Mathf.Abs(daxis2))
								daxis2 = 0;
							else
								daxis1 = 0;
						}

						if(handler.axisOptions.noRepeats && (handler.inputEvent.axis.x != daxis1 || handler.inputEvent.axis.y != daxis2))
							break;
						if(handler.axisOptions.noZeros && daxis1 == 0 && daxis2 == 0)
							break;

						handler.inputEvent.axis.Set(daxis1, daxis2);
						handler.callback.Invoke(handler.inputEvent);
						break;
				}
			}

			// Not sure if this always works with multi-touch; will the held touch at index 0 always be index 0?
			lastTouch = (Input.touchCount > 0 ? Input.GetTouch(0) : (Touch?) null);
		}
	}

	[System.Serializable]
	public class InputHandler
	{
		public InputType type;
		public KeyCode keyCode;
		public string buttonName;
		public string axisName1;
		public string axisName2;
		[Tooltip("Additional options for Axis and DoubleAxis types")]
		public AxisOptions
			axisOptions;
		public Callback callback;
		[HideInInspector]
		public InputEvent
			inputEvent;	// The result of the input, to be passed to the callback
	}

	[System.Serializable]
	public struct AxisOptions
	{
		[Tooltip("Value of axis 1 is reversed")]
		public bool
			invertAxis1;
		[Tooltip("Value of axis 2 is reversed (DoubleAxis only)")]
		public bool
			invertAxis2;
		[Tooltip("If both axes are non-zero, the absolute smaller axis is set to zero (DoubleAxis only)")]
		public bool
			noDiagonals;
		[Tooltip("The handler is only triggered when the values change")]
		public bool
			noRepeats;
		[Tooltip("If true, input smoothing is not used and raw axes are read")]
		public bool
			raw;
		[Tooltip("If true, 0,0 is not reported")]
		public bool
			noZeros; 
	}

	public struct InputEvent
	{
		public Vector2 axis;	// Axis reading; axis1 value is in x; for double axis, axis2 value is in y
		public Touch touch;		// Primary touch processed
		public string keys;		// Keys entered since last frame
		public InputHandler handler;
	}

	public enum InputType
	{
		KeyDown,
		KeyUp,
		KeyHeld,
		AnyKey,
		ButtonDown,
		ButtonUp,
		ButtonHeld, 
		Axis,
		DoubleAxis,
		TouchDown,
		TouchUp,
		TouchHeld
	}

	[System.Serializable]
	public class Callback: UnityEvent<InputEvent>
	{
	}
}