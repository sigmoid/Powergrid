using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Powergrid
{
    public interface IDraggable
    {
        public bool CanDrag();
        void OnBeginDrag();
        bool OnEndDrag();
    }
    public interface IDroppable
    {
        void OnDragOff();
        void OnDrop(GameObject droppedObject);
        bool CanDrop();
    }

public class DragDropController : MonoBehaviour
    {
        public LayerMask DraggableLayers;

        private GameObject _draggingObject;
        private Vector3 _dragObjectOriginalPos;
        private Vector3 _dragObjectOffset;


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            HandleMouse();
        }

        void HandleMouse()
        {
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (_draggingObject != null)
            {
                _draggingObject.transform.position = new Vector3(mouseWorldPos.x + _dragObjectOffset.x,
                    mouseWorldPos.y + _dragObjectOffset.y,
                    _dragObjectOriginalPos.z);

                if (Input.GetMouseButtonUp(0))
                {
                    DropObject();
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                TryGetDragObject();
            }
        }

        void TryGetDragObject()
        {
            var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero, 0, DraggableLayers);

            if (hit.transform != null && (hit.transform.gameObject.GetComponent<IDraggable>()?.CanDrag() ?? false))
            {
                _draggingObject = hit.transform.gameObject;
                _dragObjectOffset = new Vector3(hit.point.x, hit.point.y) - hit.transform.position;
                _dragObjectOriginalPos = hit.transform.position;
                _draggingObject.GetComponent<IDraggable>().OnBeginDrag();
            }
        }

        void DropObject()
        {
            if (!_draggingObject.GetComponent<IDraggable>().OnEndDrag())
            {
                _draggingObject.transform.position = _dragObjectOriginalPos;
            }

            _draggingObject = null;
        }
    }
}
