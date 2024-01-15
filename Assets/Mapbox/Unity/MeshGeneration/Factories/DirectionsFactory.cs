namespace Mapbox.Unity.MeshGeneration.Factories
{
	using UnityEngine;
	using Mapbox.Directions;
	using System.Collections.Generic;
	using System.Linq;
	using Mapbox.Unity.Map;
	using Data;
	using Modifiers;
	using Mapbox.Utils;
	using Mapbox.Unity.Utilities;
	using System.Collections;

	public class DirectionsFactory : MonoBehaviour
	{
		[SerializeField]
		AbstractMap _map;

		[SerializeField]
		MeshModifier[] MeshModifiers;
		[SerializeField]
		Material _material;

		[SerializeField]
		public Transform[] _waypoints;
		private List<Vector3> _cachedWaypoints;
		public List<Vector2d> _cachedLatLons;

		[SerializeField]
		[Range(1,10)]
		private float UpdateFrequency = 2;

		public DraggableWaypoint[] draggableWaypoints;
		public bool shouldShowRoute = false;
		private bool isSettingWaypointsPosition = false;

		private Directions _directions;
		private int _counter;

		private GameObject _directionsGO;
		private bool _recalculateNext;

		protected virtual void Awake()
		{
			if (_map == null)
			{
				_map = FindObjectOfType<AbstractMap>();
			}
			_directions = MapboxAccess.Instance.Directions;
			_map.OnInitialized += Query;
			_map.OnUpdated += Query;
		}

		public void Start()
		{
			_cachedWaypoints = new List<Vector3>(_waypoints.Length);
			_cachedLatLons = new List<Vector2d>(_waypoints.Length);
			foreach (var item in _waypoints)
			{
				_cachedWaypoints.Add(item.position);
				_cachedLatLons.Add(_map.WorldToGeoPosition(item.position));
			}
			_recalculateNext = false;

			foreach (var modifier in MeshModifiers)
			{
				modifier.Initialize();
			}

			StartCoroutine(QueryTimer());
			SetRouteVisibility(false);
		}

		// update waypoints position when dragging the map
		void Update()
		{
			if (!isSettingWaypointsPosition) {
				for (int i = 0; i < _waypoints.Length; i++)
				{
					if (!draggableWaypoints[i].isDragging)
					{
						if (_map.WorldToGeoPosition(_waypoints[i].position) != _cachedLatLons[i])
						{
							_waypoints[i].position = _map.GeoToWorldPosition(_cachedLatLons[i]);
							_cachedWaypoints[i] = _waypoints[i].position;
						}
					}
				}
			}
		}

		protected virtual void OnDestroy()
		{
			_map.OnInitialized -= Query;
			_map.OnUpdated -= Query;
		}

		void Query()
		{
			if (shouldShowRoute)
			{
				var count = _waypoints.Length;
				var wp = new Vector2d[count];
				for (int i = 0; i < count; i++)
				{
					wp[i] = _waypoints[i].GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
				}
				var _directionResource = new DirectionResource(wp, RoutingProfile.Driving);
				_directionResource.Steps = true;
				_directions.Query(_directionResource, HandleDirectionsResponse); // retrieve route data
			}
		}

		public IEnumerator QueryTimer()
		{
			while (true)
			{
				yield return new WaitForSeconds(UpdateFrequency);
				for (int i = 0; i < _waypoints.Length; i++)
				{
					if (_waypoints[i].position != _cachedWaypoints[i])
					{
						_recalculateNext = true;
						_cachedWaypoints[i] = _waypoints[i].position;
					}
				}

				if (_recalculateNext)
				{
					Query();
					_recalculateNext = false;
				}
			}
		}

		void HandleDirectionsResponse(DirectionsResponse response)
		{
			if (response == null || null == response.Routes || response.Routes.Count < 1)
			{
				return;
			}

			var meshData = new MeshData();
			var dat = new List<Vector3>();
			foreach (var point in response.Routes[0].Geometry)
			{
				dat.Add(Conversions.GeoToWorldPosition(point.x, point.y, _map.CenterMercator, _map.WorldRelativeScale).ToVector3xz());
			}

			var feat = new VectorFeatureUnity();
			feat.Points.Add(dat);

			foreach (MeshModifier mod in MeshModifiers.Where(x => x.Active))
			{
				mod.Run(feat, meshData, _map.WorldRelativeScale);
			}

			CreateGameObject(meshData); // create route object
		}

		GameObject CreateGameObject(MeshData data)
		{
			if (_directionsGO != null)
			{
				_directionsGO.Destroy();
			}
			_directionsGO = new GameObject("Route");
			var mesh = _directionsGO.AddComponent<MeshFilter>().mesh;
			mesh.subMeshCount = data.Triangles.Count;

			mesh.SetVertices(data.Vertices);
			_counter = data.Triangles.Count;
			for (int i = 0; i < _counter; i++)
			{
				var triangle = data.Triangles[i];
				mesh.SetTriangles(triangle, i);
			}

			_counter = data.UV.Count;
			for (int i = 0; i < _counter; i++)
			{
				var uv = data.UV[i];
				mesh.SetUVs(i, uv);
			}

			mesh.RecalculateNormals();
			_directionsGO.AddComponent<MeshRenderer>().material = _material;
			return _directionsGO;
		}

		public void SetWaypointsPosition(Vector2d[] latLon)
		{
			isSettingWaypointsPosition = true;

			for (int i = 0; i < latLon.Length; i++)
			{
				_waypoints[i].position = _map.GeoToWorldPosition(latLon[i]);
				_cachedLatLons[i] = latLon[i];
				_cachedWaypoints[i] = _waypoints[i].position;
			}

			isSettingWaypointsPosition = false;
		}

		public void SetWaypointsPosition(Vector3[] worldPositions)
		{
			isSettingWaypointsPosition = true;

			for (int i = 0; i < worldPositions.Length; i++)
			{
				_waypoints[i].position = worldPositions[i];
				_cachedLatLons[i] = _map.WorldToGeoPosition(worldPositions[i]);
				_cachedWaypoints[i] = worldPositions[i];
			}

			isSettingWaypointsPosition = false;
		}

		public void SetRouteVisibility(bool isVisible)
		{
			foreach (Transform waypoint in _waypoints)
			{
				waypoint.gameObject.SetActive(isVisible);
			}

			if (_directionsGO != null)
			{
				_directionsGO.SetActive(isVisible);
			}

			shouldShowRoute = isVisible;

			if (isVisible)
            {
				Query();
            }
		}
	}
}
