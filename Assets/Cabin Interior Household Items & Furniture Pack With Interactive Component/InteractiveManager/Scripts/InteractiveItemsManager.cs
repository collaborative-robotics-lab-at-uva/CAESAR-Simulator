using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MB
{
	public class InteractiveItemsManager : MonoBehaviour 
	{

		//Instance
		private static InteractiveItemsManager _instance = null;
		public static InteractiveItemsManager Instance
		{
			get
			{
				if(_instance==null)
					_instance = (InteractiveItemsManager)FindObjectOfType(typeof(InteractiveItemsManager));
				return _instance;
			}
		}

		//Private
		private Dictionary<int,InteractiveItem> _interactiveItemDictionary = new Dictionary<int, InteractiveItem>();

		//Methods
		public void RegisterInteractiveItem(int key, InteractiveItem script)
		{	
			//Debug.Log("Register");
			//Stores InteraciveItem script reference in the Dictionary
			// indexed by its key(instanceID of its collider)
			if(!_interactiveItemDictionary.ContainsKey(key))
			{
				_interactiveItemDictionary[key] = script;
			}
		}

		public InteractiveItem GetInteractiveItem(int key)
		{	
			//Debug.Log("Fetched");
			InteractiveItem item = null;
			_interactiveItemDictionary.TryGetValue(key, out item);
			return item;
		}

	}
}

