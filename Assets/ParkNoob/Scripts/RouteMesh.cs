using UnityEngine;

namespace ParkNoob
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RouteMesh : MonoBehaviour
    {
        [field: SerializeField]
        public MeshFilter MeshFilter { get; private set; }
        
        [field: SerializeField]
        public MeshRenderer MeshRenderer { get; private set; }
    }
}