using UnityEngine;

namespace MB
{	
	public enum OpenAxis
	{
		Foward,Up,Right
	}
	public enum OpenDirection
	{
		Positive,Negative
	}

	public enum Method
	{
		Coroutine,Update
	}
	public class InteractiveItem : MonoBehaviour 
	{

		// Inspector
		public int ItemPriority = 0; 

		protected InteractiveItemsManager _interactiveItemsManager = null;
		protected Collider _collider = null;

		// Methods for child class
		public virtual string GetText() { return null;}
		public virtual void Activate (PlayerInteractiveManager playerInteractiveManager) {}
		
		protected virtual void Start()
		{
			_interactiveItemsManager = InteractiveItemsManager.Instance;
			_collider = GetComponent<Collider>();

			if(_interactiveItemsManager!=null && _collider!=null)
			{
			_interactiveItemsManager.RegisterInteractiveItem(_collider.GetInstanceID(), this);
			}
		}
	}
}

