using Mapbox.Examples;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableWaypoint : MonoBehaviour
{
	public Transform MoveTarget;
    public AbstractMap map;
    public Camera mapCamera;
    public DirectionsFactory directionsFactory;

    public bool isDragging;

	void Update()
    {
        if (!QuadTreeCameraMovement.isDragging) // avoid dragging the map at the same map
        {
            if (MapUtils.mapIsShown && Input.GetMouseButton(0))
            {
                Vector3 mousePosScreen = Input.mousePosition;
                RectTransform rectTransform = GameObject.Find("MapImage").GetComponent<RectTransform>();
                Vector2 localPoint;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosScreen, null, out localPoint))
                {
                    // convert to viewport coordinates
                    localPoint.x /= rectTransform.rect.width;
                    localPoint.y /= rectTransform.rect.height;

                    Ray ray = mapCamera.ViewportPointToRay(localPoint);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        GameObject colliderObj = hitInfo.collider.gameObject;
                        if (ReferenceEquals(colliderObj, transform.GetChild(0).gameObject) || // check if the mouse is pointing at one of the markers
                            ReferenceEquals(colliderObj, transform.GetChild(1).gameObject))
                        {
                            isDragging = true;
                        }
                    }
                }
            }

            if (Input.GetMouseButton(0) && isDragging)
            {
                Vector3 mousePosScreen = Input.mousePosition;
                RectTransform rectTransform = GameObject.Find("MapImage").transform.GetComponent<RectTransform>();
                Vector2 localPoint;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosScreen, null, out localPoint))
                {
                    // convert to viewport point
                    localPoint.x /= rectTransform.rect.width;
                    localPoint.y /= rectTransform.rect.height;

                    // cast a ray from map camera to find the intersection on the map
                    Ray ray = mapCamera.ViewportPointToRay(localPoint);
                    Plane plane = new Plane(Vector3.up, Vector3.zero); // map lies on x-z plane
                    float distance;
                    plane.Raycast(ray, out distance);
                    Vector3 hit = ray.GetPoint(distance);

                    Vector3 worldPos = new Vector3(hit.x, mapCamera.transform.localPosition.y, hit.z); // world position on map
                    Vector2d latLon = map.WorldToGeoPosition(worldPos); // convert world position to lat, lon

                    int idx = int.Parse(transform.parent.name.Substring(gameObject.name.Length - 1)) - 1;
                    directionsFactory._cachedLatLons[idx] = latLon;

                    MoveTarget.position = hit;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }        
	}
}
