﻿namespace Castle
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public static class CastleManager
	{
		public static CastleObject selectedObject, hoveredObject;
		public static Vector2 tapPosition;
		public static Collider2D[] colliderBuffer;
		public static Collider[] collider3DBuffer;
		static CastleObject focusedObject;
		static Plane inputPlane;
		static Vector3 worldTapPos;
		public static bool showLog;

		public enum HoverState
		{
			None,
			EnterHover,
			Hover,
			ExitHover
		}

		public enum SelectedState
		{
			None,
			Tap,
			Hold,
			Release
		}

		public enum CastleInputMode
		{
			SIMPLE,
			COMPLEX,
			PERSPECTIVE
		}

		public static CastleInputMode inputMode;
		
		/// <summary>
		/// Initialises your input;
		/// </summary>
		/// <param name="_inputMode">Input mode for checks. Simple for non tilted cameras, complex for cameras with angles.</param>
		public static void Init(CastleInputMode _inputMode = CastleInputMode.SIMPLE)
		{
			inputMode = _inputMode;
			switch(inputMode)
			{
				case CastleInputMode.SIMPLE:

					break;
				case CastleInputMode.COMPLEX:
					inputPlane = new Plane(-Vector3.forward, Vector3.zero);
					break;
			}
		}
		public static void SetInputPlane(Vector3 normal, Vector3 planePos)
		{
			inputPlane.SetNormalAndPosition(-normal, planePos);
		}
		/// <summary>
		/// Call this function using your game manager to handle touch input.
		/// </summary>
		public static void CastleUpdate()
		{
			switch (inputMode)
			{
				case CastleInputMode.SIMPLE:

#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR)
					if(Input.touchCount > 0)
					{
						worldTapPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
					}
#else
					worldTapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
#endif
					break;
				case CastleInputMode.COMPLEX:
					//inputPlane.SetNormalAndPosition(-Vector3.forward, Vector3.zero);

#if ((UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR)
					if (Input.touchCount > 0)
					{
						Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
						float hitdist = 0.0f;

						if (inputPlane.Raycast(ray, out hitdist))
						{
							worldTapPos = Camera.main.ScreenToWorldPoint((Vector3)Input.GetTouch(0).position + (Vector3.forward * hitdist));
						}
					}
#else
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					float hitdist = 0.0f;
					if (inputPlane.Raycast(ray, out hitdist))
					{
						worldTapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + (Vector3.forward * hitdist));
					}
#endif
					break;
				case CastleInputMode.PERSPECTIVE:
					Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition - Vector3.forward);
					tapPosition = pos;
					Vector3 dir = (Camera.main.transform.position - pos).normalized;
					RaycastHit[] hitPoints = Physics.RaycastAll(Camera.main.transform.position,dir,100);
					collider3DBuffer = new Collider[hitPoints.Length];
					for(int i = 0; i <hitPoints.Length; i++)
					{
						collider3DBuffer[i] = hitPoints[i].collider;
					}
					break;
			}
			if(inputMode == CastleInputMode.PERSPECTIVE)
			{
				focusedObject = IsolateObject(collider3DBuffer);
			}
			else
			{
				tapPosition = new Vector2(worldTapPos.x, worldTapPos.y);
				colliderBuffer = Physics2D.OverlapPointAll(tapPosition);
				focusedObject = IsolateObject(colliderBuffer);
			}
			
			Hover(focusedObject);
			Select(focusedObject);
		}

		static bool DetectObject(Collider2D[] _colls)
		{
			if (_colls.Length == 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		static bool DetectObject(Collider[] _colls)
		{
			if (_colls.Length == 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		static bool CheckObject(Collider2D _coll)
		{
			for(int i = 0; i < colliderBuffer.Length; i++)
			{
				if(_coll == colliderBuffer[i])
				{
					return true;
				}
			}
			return false;
		}
		static bool CheckObject(Collider _coll)
		{
			for(int i = 0; i < collider3DBuffer.Length; i++)
			{
				if(_coll == collider3DBuffer[i])
				{
					return true;
				}
			}
			return false;
		}

		static CastleObject IsolateObject(Collider2D[] _colls)
		{
			if (DetectObject(_colls))
			{
				return ClosestObject(_colls);
			}
			else
			{
				return null;
			}
		}
		static CastleObject IsolateObject(Collider[] _colls)
		{
			if (DetectObject(_colls))
			{
				return ClosestObject(_colls);
			}
			else
			{
				return null;
			}
		}

		static CastleObject ClosestObject(Collider2D[] _colls, bool excludeSelected = true)
		{
			float closestDist = 99;
			int chosenColl = 0;
			for (int i = 0; i < _colls.Length; i++)
			{
				if (_colls[i].transform.position.z < closestDist)
				{
					if (excludeSelected && selectedObject)
					{
						if (_colls[i] != selectedObject.coll)
						{
							closestDist = _colls[i].transform.position.z;
							chosenColl = i;
						}
					}
					else
					{
						closestDist = _colls[i].transform.position.z;
						chosenColl = i;
					}
				}
			}
			if (closestDist == 99)
			{
				return null;
			}
			else
			{
				return _colls[chosenColl].GetComponent<CastleObject>();
			}
		}
		static CastleObject ClosestObject(Collider[] _colls, bool excludeSelected = true)
		{
			float closestDist = 99;
			int chosenColl = 0;
			for (int i = 0; i < _colls.Length; i++)
			{
				if (_colls[i].transform.position.z < closestDist)
				{
					if (excludeSelected && selectedObject)
					{
						if (_colls[i] != selectedObject.coll3D)
						{
							closestDist = _colls[i].transform.position.z;
							chosenColl = i;
						}
					}
					else
					{
						closestDist = _colls[i].transform.position.z;
						chosenColl = i;
					}
				}
			}
			if (closestDist == 99)
			{
				return null;
			}
			else
			{
				return _colls[chosenColl].GetComponent<CastleObject>();
			}
		}
		public static Vector3 TapPosition(float z)
		{
			Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition - Vector3.forward);
			Vector3 dir = (Camera.main.transform.position - pos);
			float zOffset = (z - Camera.main.transform.position.z);
			return Camera.main.transform.position + (dir * zOffset);
		}
		public static Vector3 PosZ(Vector3 position, float z)
		{
			Vector3 dir = (Camera.main.transform.position - position);
			dir /= dir.z;
			float zOffset = (z - Camera.main.transform.position.z);
			return Camera.main.transform.position + (dir * zOffset);
		}
		public static void Drag(this CastleObject _object, float dragDelay = 10, bool instant = false)
		{
			if(inputMode == CastleInputMode.PERSPECTIVE)
			{
				if (instant)
				{
					_object.transform.position = TapPosition(_object.transform.position.z);
				}
				else
				{
					_object.transform.position = Vector3.Lerp(_object.transform.position, TapPosition(_object.transform.position.z), Time.deltaTime * dragDelay);
				}
			}
			else
			{
				if (instant)
				{
					_object.transform.position = CastleTools.Vec3RepZ(tapPosition, _object.transform.position.z);
				}
				else
				{
					_object.transform.position = Vector3.Lerp(_object.transform.position, CastleTools.Vec3RepZ(tapPosition, _object.transform.position.z), Time.deltaTime * dragDelay);
				}
			}
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			System.Random rng = new System.Random();
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static void Unselect()
		{
			if (selectedObject)
			{
				selectedObject.Release();
				selectedObject = null;
			}
		}

		public static void Hover(CastleObject _object)
		{
			if (!_object)
			{
				if (hoveredObject)
				{
					hoveredObject.ExitHover();
					hoveredObject = null;
				}
			}
			else if (!hoveredObject)
			{
				hoveredObject = _object;
				hoveredObject.EnterHover();
			}
			else if (hoveredObject == _object)
			{
				hoveredObject.Hover();
			}
			else
			{
				hoveredObject.ExitHover();
				hoveredObject = _object;
				hoveredObject.EnterHover();
			}
		}

		public static void Select(CastleObject _object, bool _override = false)
		{
			if (!selectedObject && !_object)
			{
				return;
			}
			if (_override)
			{
				if (selectedObject)
				{
					selectedObject.Release();
				}
				selectedObject = _object;
				selectedObject.Tap();
				return;
			}
            HandleInput(_object);
		}
        static void HandleInput(CastleObject _object)
        {
            if (Input.GetMouseButtonDown(0))
            {
                selectedObject = _object;
                if (selectedObject)
                {
                    selectedObject.Tap();
                }
            }
            else if(Input.GetMouseButton(0))
            {
                if (selectedObject)
                {
                    if (inputMode == CastleInputMode.PERSPECTIVE)
                    {
                        if (CheckObject(selectedObject.coll3D))
                        {
                            selectedObject.Hold();
                        }
                        else
                        {
                            selectedObject.DragOff();
                        }
                    }
                    else
                    {
                        if (CheckObject(selectedObject.coll))
                        {
                            selectedObject.Hold();
                        }
                        else
                        {
                            selectedObject.DragOff();
                        }
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (selectedObject)
                {
                    selectedObject.Release();
                    selectedObject = null;
                }
            }
        }
    }
}
