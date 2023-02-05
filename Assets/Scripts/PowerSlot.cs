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
        public bool IsGoal;

        private GameObject _storedObject;
        private int _activeConnections = 0;

        private bool _IsActive;

        protected HashSet<PowerSlot> _powerFrom = new HashSet<PowerSlot>();


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

        public bool CanDrop(int power)
        {
            if (GetComponentInParent<LevelManager>() == null)
            {
                return true;
            }
            return GetComponentInParent<LevelManager>().CanAddPower(this, power);
        }

		// Start is called before the first frame update
		void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Connect(PowerSlot other, int remainingPower, HashSet<PowerSlot> seen = null)
        {
			if (seen == null || seen.Contains(this))
				return;

			if (seen == null)
				seen = new HashSet<PowerSlot>();
			seen.Add(this);

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
            ClearPower();
        }

        public void PowerUp(int remainingPower)
        {
            SpriteRenderer.sprite = PoweredOnSprite;
            _IsActive = true;
            gameObject.SendMessage("OnPowered", SendMessageOptions.DontRequireReceiver);
        }

        public void PowerDown()
        {
            _storedObject = null;
            _IsActive = false;
            gameObject.SendMessage("OnUnpowered", SendMessageOptions.DontRequireReceiver);
		}

        public void AddPowerFrom(PowerSlot other)
        {
            _powerFrom.Add(other);
        }

        public void ClearPower()
        {
            _powerFrom.Clear();
        }

        public virtual int ConvertPower(int inputPower)
        {
            return inputPower;
        }


        public virtual int GetPower()
        {
            return _storedObject?.GetComponent<DraggableObject>().Power ?? 0;
        }
    }
}