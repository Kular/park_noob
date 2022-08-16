using System.Collections.Generic;
using UnityEngine;

namespace ParkNoob
{
    public class Route : MonoBehaviour
    {
        [SerializeField]
        private LayerMask groundLayerMask;

        [SerializeField]
        private LayerMask carLayerMask;

        [SerializeField]
        private List<Car> cars;

        [SerializeField]
        private float routeWidth = 0.2f;

        [SerializeField]
        private Material mat;

        private RaycastHit hit;
        private RaycastHit hitWithCar;

        private List<Vector2> positions;

        private float sqrThreshold;
        private int currentCarId = -1;
        private Matrix4x4 worldToLocal;
        private List<Material> materials;
        private List<RouteMesh> routeMeshes;

        private void Start()
        {
            Initialize(0.5f);
        }

        public void Initialize(float threshold)
        {
            positions = new List<Vector2>();
            materials = new List<Material>(cars.Count);
            routeMeshes = new List<RouteMesh>(cars.Count);
            for (var i = 0; i < cars.Count; i++) {
                cars[i].Initialize(i);
                var material = new Material(mat);
                material.SetColor("_BaseColor", cars[i].Color);
                materials.Add(material);

                var prefab = Instantiate(Resources.Load("RouteMesh")) as GameObject;
                var tmpPos = prefab.transform.position;
                tmpPos.y = transform.position.y;
                prefab.transform.position = tmpPos;
                routeMeshes.Add(prefab.GetComponent<RouteMesh>());
            }
            sqrThreshold = threshold * threshold;
            worldToLocal = transform.worldToLocalMatrix;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (! Physics.Raycast(ray, out hitWithCar, 100f, carLayerMask)) return;
                currentCarId = hitWithCar.collider.GetComponent<Car>().Id;
                if (! Physics.Raycast(hitWithCar.point, Vector3.down, out hit, 10f, groundLayerMask)) return;
                var posOnGround = new Vector2(hit.point.x, hit.point.z);
                positions.Clear();
                positions.Add(posOnGround);
            }

            if (positions.Count > 0 && Input.GetMouseButton(0)) {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (! Physics.Raycast(ray, out hit, 100f, groundLayerMask)) return;
                var posOnGround = new Vector2(hit.point.x, hit.point.z);
                if (Vector2.SqrMagnitude(posOnGround - positions[positions.Count - 1]) < sqrThreshold) return;
                positions.Add(posOnGround);
                DrawMesh();
            }

            if (Input.GetMouseButtonUp(0)) cars[currentCarId].MoveAlong(positions);
        }

        private void DrawMesh()
        {
            if (positions.Count < 2) return;
            var mesh = new Mesh();
            mesh.name = $"route_{currentCarId}";

            routeMeshes[currentCarId].MeshRenderer.material = materials[currentCarId];

            var meshInfos = new MeshInfo[positions.Count];
            for (var i = 0; i < positions.Count; i++) {
                if (i == 0) {
                    meshInfos[i].direction = positions[i + 1] - positions[i];
                } else if (i == positions.Count - 1) {
                    meshInfos[i].direction = positions[i] - positions[i - 1];
                } else {
                    meshInfos[i].direction = positions[i + 1] - positions[i - 1];
                }
                meshInfos[i].direction.Normalize();

                Vector2 side = Quaternion.AngleAxis(90f, Vector3.back) * meshInfos[i].direction;
                side.Normalize();

                meshInfos[i].left = positions[i] - side * routeWidth / 2f;
                meshInfos[i].right = positions[i] + side * routeWidth / 2f;
            }
            int meshNum = positions.Count - 1;
            var vertices = new Vector3[meshNum * 4];
            var triangles = new int[meshNum * 2 * 3];

            var positionIndex = 0;
            for (var i = 0; i < meshNum; i++) {
                vertices[i * 4 + 0] = worldToLocal.MultiplyPoint3x4(new Vector3(meshInfos[i].left.x, transform.position.y, meshInfos[i].left.y));
                vertices[i * 4 + 1] = worldToLocal.MultiplyPoint3x4(new Vector3(meshInfos[i].right.x, transform.position.y, meshInfos[i].right.y));
                vertices[i * 4 + 2] = worldToLocal.MultiplyPoint3x4(new Vector3(meshInfos[i + 1].left.x, transform.position.y, meshInfos[i + 1].left.y));
                vertices[i * 4 + 3] = worldToLocal.MultiplyPoint3x4(new Vector3(meshInfos[i + 1].right.x, transform.position.y, meshInfos[i + 1].right.y));

                triangles[positionIndex++] = (i * 4) + 1;
                triangles[positionIndex++] = (i * 4) + 0;
                triangles[positionIndex++] = (i * 4) + 2;
                triangles[positionIndex++] = (i * 4) + 2;
                triangles[positionIndex++] = (i * 4) + 3;
                triangles[positionIndex++] = (i * 4) + 1;
            }
            mesh.vertices = vertices;
            mesh.triangles = triangles;

            routeMeshes[currentCarId].MeshFilter.mesh = mesh;
        }
    }

    public struct MeshInfo
    {
        public Vector2 left;
        public Vector2 right;
        public Vector2 direction;
    }
}