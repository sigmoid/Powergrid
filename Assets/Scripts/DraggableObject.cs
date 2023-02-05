using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Powergrid
{
    public class DraggableObject : MonoBehaviour, IDraggable
    {
		public int Power = 1;
		IDroppable _dropArea = null;

		private bool _hasBeenPlaced = false;

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
				return false;
			}
			else
			{
				_dropArea = dropArea;
				dropArea?.OnDrop(this.gameObject);
				_hasBeenPlaced = true;
				return true;
			}
		}

		public void Return()
		{
			_dropArea.OnDrop(this.gameObject);
		}

		public bool CanDrag()
		{
			if (!_hasBeenPlaced)
				return true;

			var slot = GetComponentInParent<PowerSlot>();
			if (slot != null)
			{
				if (GetComponentInParent<LevelManager>() == null)
					return true;
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
				if (hit.GetComponent<IDroppable>()?.CanDrop(this.Power) ?? false)
				{
					return hit.GetComponent<IDroppable>();
				}
			}

			return null;
		}
	}
}