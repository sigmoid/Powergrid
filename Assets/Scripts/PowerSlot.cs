using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Powergrid
{
    public class PowerSlot : MonoBehaviour, IDroppable
    {
        public List<PowerSlot> OutgoingConnections = new List<PowerSlot>();
        public SpriteRenderer SpriteRenderer;
        public Sprite PoweredOnSprite;
        public Sprite PoweredOffSprite;
        public bool IsStart;
        public bool IsAnchor;
        public bool Removed;

        private GameObject _storedObject;
        private int _activeConnections = 0;

        private bool _IsActive;


		public void OnDrop(GameObject droppedObject)
		{
            droppedObject.transform.SetParent(this.transform);
            droppedObject.transform.localPosition = new Vector3(0,0, -1);
            _storedObject = droppedObject;

            PowerUp(1);
		}

        public void OnDragOff()
        {
            PowerDown();
        }

        public bool CanDrop()
        {
            return GetComponentInParent<LevelManager>().CanAddPower(this);
        }

		// Start is called before the first frame update
		void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Connect(PowerSlot other, int remainingPower)
        {
            if (remainingPower > 0)
                PowerUp(remainingPower);
        }

        public void Disconnect(PowerSlot other)
        {
            _activeConnections--;
            if (_activeConnections <= 0)
            {
                SpriteRenderer.sprite = PoweredOffSprite;
            }
        }

        public void Activate()
        { 
        }

        public void Deactivate()
        {
            _activeConnections = 0;
        }

        public void PowerUp(int remainingPower)
        {
            SpriteRenderer.sprite = PoweredOnSprite;

            foreach (var connect in OutgoingConnections)
            {
                // TODO, recursively find children
                connect.Connect(this, 1);
            }

            _IsActive = true;
        }

        public void PowerDown()
        {
            _storedObject = null;
            _IsActive = false;
		}

        public int GetPower()
        {
            return _storedObject?.GetComponent<DraggableObject>().Power ?? 0;
        }
    }
}