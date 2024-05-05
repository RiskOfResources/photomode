using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;

namespace PhotoMode;

public class SmoothCurve {
	public static List<PhotoModeCameraState> GenerateSmoothCurve(LineRenderer lineRenderer,
		List<PhotoModeCameraState> controlPoints, int numberOfPoints, bool smoothDolly)
	{
		Color color = Color.white;
		float width = 0.2f;
		var curve = new List<PhotoModeCameraState>();

		if (!lineRenderer) {
			Logger.Log("No line renderer found?");
		}

		lineRenderer.useWorldSpace = true;
		lineRenderer.startColor = color;
		lineRenderer.endColor = color;
		lineRenderer.startWidth = width;
		lineRenderer.endWidth = width;
		lineRenderer.positionCount = numberOfPoints * (controlPoints.Count - 1);
		
		if (controlPoints.Count < 2) {
			Logger.Log("Not enough points specified for curve");
			return curve;
		}

		// loop over segments of spline
		for(int j = 0; j < controlPoints.Count - 1; j++) {
			// determine control points of segment
			var p0 = controlPoints[j].State.position;
			var p1 = controlPoints[j + 1].State.position;

			Vector3 m0;
			if (j > 0) {
				m0 = 0.5f * (controlPoints[j + 1].State.position - controlPoints[j - 1].State.position);
			}
			else {
				m0 = controlPoints[j + 1].State.position - controlPoints[j].State.position;
			}

			Vector3 m1;
			if (j < controlPoints.Count - 2) {
				m1 = 0.5f * (controlPoints[j + 2].State.position - controlPoints[j].State.position);
			}
			else {
				m1 = controlPoints[j + 1].State.position - controlPoints[j].State.position;
			}

			// set points of Hermite curve
			var pointStep = 1.0f / numberOfPoints;

			if (j == controlPoints.Count - 2) {
				pointStep = 1.0f / (numberOfPoints - 1.0f);
				// last point of last segment should reach p1
			}  
			for(int i = 0; i < numberOfPoints; i++) {
				var t = i * pointStep;
				var position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * p0 
				               + (t * t * t - 2.0f * t * t + t) * m0 
				               + (-2.0f * t * t * t + 3.0f * t * t) * p1 
				               + (t * t * t - t * t) * m1;
				lineRenderer.SetPosition(i + j * numberOfPoints, position);
				var currentState = controlPoints[j];
				var nextState = controlPoints[j + 1];
				var ratio = (float) (i + j * numberOfPoints) / lineRenderer.positionCount;
				
				// if smooth only consider the starting and ending rotations
				// so there's no bouncing around control points
				// when the rotation speed abruptly changes
				if (smoothDolly) {
					curve.Add(new PhotoModeCameraState() {
						position = position,
						rotation = Quaternion.Slerp(controlPoints[0].rotation, controlPoints.Last().rotation, ratio),
						fov = Mathf.Lerp(currentState.fov, nextState.fov, (float) i / numberOfPoints),
						FocusDistance = Mathf.Lerp(currentState.FocusDistance, nextState.FocusDistance, (float) i / numberOfPoints)
					});
				}
				else {
					curve.Add(new PhotoModeCameraState() {
						position = position,
						rotation = Quaternion.Slerp(currentState.rotation, nextState.rotation, (float) i / numberOfPoints),
						fov = Mathf.Lerp(currentState.fov, nextState.fov, (float) i / numberOfPoints),
						FocusDistance = Mathf.Lerp(currentState.FocusDistance, nextState.FocusDistance, (float) i / numberOfPoints)
					});
				}
			}
		}

		Logger.Log($"Returning Curve {curve.Count}");
		return curve;
	}
}
