namespace Mapbox.Examples
{
	using UnityEngine;

	public class CameraBillboard : MonoBehaviour
	{
		//public Camera _camera;

		public void Start()
		{
			//_camera = GameObject.Find("MapCamera").GetComponent<Camera>();
		}

		void Update()
		{
			//transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward, _camera.transform.rotation * Vector3.up);
			transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.up);

			if (transform.position.y < 10) {
				transform.position += new Vector3(0, 10, 0);
			}
		}
	}
}