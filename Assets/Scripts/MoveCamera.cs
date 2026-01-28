using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour 
{
	public float lookScalar = 5.0f;
	public float sideScalar = 5.0f;
	public float zoomSpeed = 5.0f;

	private Vector3 mouseStart;
	private bool isMoveingSide;
	private bool isRotating;
	private bool isZooming;
	
	void Update () 
	{
		if(Input.GetMouseButtonDown(0))
		{
			mouseStart = Input.mousePosition;
			isRotating = true;
		}
		
		if(Input.GetMouseButtonDown(1))
		{
			mouseStart = Input.mousePosition;
			isMoveingSide = true;
		}
		
		if(Input.GetMouseButtonDown(2))
		{
			mouseStart = Input.mousePosition;
			isZooming = true;
		}
		
		if (!Input.GetMouseButton(0)) isRotating=false;
		if (!Input.GetMouseButton(1)) isMoveingSide=false;
		if (!Input.GetMouseButton(2)) isZooming=false;
		
		if (isRotating)
		{
	        Vector3 mousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseStart);

			transform.RotateAround(transform.position, transform.right, -mousePosition.y * lookScalar);
			transform.RotateAround(transform.position, Vector3.up, mousePosition.x * lookScalar);
		}
		
		if (isMoveingSide)
		{
	        Vector3 mousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseStart);

	        Vector3 move = new Vector3(mousePosition.x * sideScalar, mousePosition.y * sideScalar, 0);
	        transform.Translate(move, Space.Self);
		}
		
		if (isZooming)
		{
	        Vector3 mousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseStart);

	        Vector3 move = mousePosition.y * zoomSpeed * transform.forward; 
	        transform.Translate(move, Space.World);
		}
	}
}