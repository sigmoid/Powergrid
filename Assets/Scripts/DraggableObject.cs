using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Powergrid
{
    public class DraggableObject : MonoBehaviour, IDraggable
    {
		IDroppable _dropArea = null;

		public void OnBeginDrag()
		{
			if (_dropArea != null)
			{
				_dropArea.OnDragOff();
			}
		}

		public bool OnEndDrag()
		{
			var dropArea = GetDropArea();
			if (dropArea == null)
			{
				_dropArea?.OnDrop(this.gameObject);
				return false;
			}
			else
			{
				_dropArea = dropArea;
				dropArea?.OnDrop(this.gameObject);
				return true;
			}
		}

		public bool CanDrag()
		{
			var slot = GetComponentInParent<PowerSlot>();
			if (slot != null)
			{
				return GetComponentInParent<LevelManager>().CanRemovePower(slot);
			}
			return true;
		}

		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

		private void OnTriggerEnter2D(Collider2D collision)
		{
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
		}

		private IDroppable? GetDropArea()
		{
			var hitInfo = Physics2D.OverlapCircleAll(this.transform.position, this.GetComponent<CircleCollider2D>().radius);

			foreach (var hit in hitInfo)
			{
				if (hit.GetComponent<IDroppable>()?.CanDrop() ?? false)
				{
					return hit.GetComponent<IDroppable>();
				}
			}

			return null;
		}
	}
}