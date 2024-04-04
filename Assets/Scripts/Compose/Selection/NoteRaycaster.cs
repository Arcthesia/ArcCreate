using System;
using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Compose.Selection
{
    public static class NoteRaycaster
    {
        private static readonly List<Vector3> Vertices = new List<Vector3>();
        private static readonly List<int> Triangles = new List<int>();

        public static int Raycast(Ray ray, NoteRaycastHit[] hitResults, int maxDistance)
        {
            int count = 0;
            int timing = Services.Gameplay.Audio.ChartTiming;
            foreach (var note in Services.Gameplay.Chart.GetRenderingNotes())
            {
                if (count >= hitResults.Length)
                {
                    return count;
                }

                if (note.TimingGroupInstance.GroupProperties.Editable
                && RayHitsNote(timing, ray, note, out Vector3 point, out float distance))
                {
                    hitResults[count] = new NoteRaycastHit
                    {
                        Note = note,
                        HitPoint = point,
                        HitDistance = distance,
                    };

                    count++;
                }
            }

            return count;
        }

        // Implementation of https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
        private static bool RayHitsNote(int timing, Ray ray, Note note, out Vector3 point, out float distance)
        {
            point = default;
            distance = default;
            note.GenerateColliderTriangles(timing, Vertices, Triangles);
            for (int i = 0; i < Triangles.Count / 3; i++)
            {
                Vector3 a = Vertices[Triangles[(i * 3) + 0]];
                Vector3 b = Vertices[Triangles[(i * 3) + 1]];
                Vector3 c = Vertices[Triangles[(i * 3) + 2]];

                Vector3 edge1 = b - a;
                Vector3 edge2 = c - a;

                Vector3 rayCrossE2 = Vector3.Cross(ray.direction, edge2);
                float det = Vector3.Dot(edge1, rayCrossE2);

                if (Mathf.Approximately(det, 0))
                {
                    continue; // This ray is parallel to this triangle.
                }

                float inverseDet = 1 / det;
                Vector3 s = ray.origin - a;
                float u = inverseDet * Vector3.Dot(s, rayCrossE2);
                if (u < 0 || u > 1)
                {
                    continue;
                }

                Vector3 sCrossE1 = Vector3.Cross(s, edge1);
                float v = inverseDet * Vector3.Dot(ray.direction, sCrossE1);
                if (v < 0 || u + v > 1)
                {
                    continue;
                }

                float t = inverseDet * Vector3.Dot(edge2, sCrossE1);

                if (t > Mathf.Epsilon)
                {
                    point = ray.origin + (ray.direction * t);
                    distance = t;
                    return true;
                }
                else
                {
                    continue;
                }
            }

            return false;
        }
    }
}