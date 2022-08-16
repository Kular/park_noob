using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ParkNoob
{
    public class Car : MonoBehaviour
    {
        public int Id { get; private set; }
        
        [field: SerializeField]
        public Color Color { get; private set; }

        [SerializeField]
        private Transform model;
        
        private RaycastHit hit;
        private RaycastHit hitWithCar;

        private bool isMoving;

        private Vector3 defaultPos;

        public void Initialize(int id)
        {
            Id = id;
            isMoving = false;
            defaultPos = model.transform.position;
        }

        public void MoveAlong(List<Vector2> positions)
        {
            StartCoroutine(DoMove(positions));
        }

        private IEnumerator DoMove(List<Vector2> positions)
        {
            if (isMoving) yield return null;
            isMoving = true;
            model.transform.position = defaultPos;
            for (var i = 0; i < positions.Count; i++) {
                model.transform.position = new Vector3(positions[i].x, defaultPos.y, positions[i].y);
                if (i < positions.Count - 1)
                    model.transform.LookAt(new Vector3(positions[i + 1].x, defaultPos.y, positions[i + 1].y));
                yield return null;
            }
            isMoving = false;
        }
    }

}