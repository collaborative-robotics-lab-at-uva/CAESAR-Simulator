using UnityEngine;

namespace MB
{
	public class PlayerInteractiveManager : MonoBehaviour
	{	
		[SerializeField] private InteractiveHUD _interactiveHUD;
		private Camera _camera = null;
		private int _interactiveMask = 0;
		private InteractiveItemsManager _interactiveItemsManager = null;
		void Start()
		{
			_camera = Camera.main;
			_interactiveMask = 1 << LayerMask.NameToLayer("Interactive");
			_interactiveItemsManager = InteractiveItemsManager.Instance;
		}
		private void Update()
		{
			Ray ray;
			RaycastHit hit;
			RaycastHit[] hits;
			
			// Get a ray from the camera through the screen point 
			ray = _camera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0));

			//calculate ray lenght based on player movement
			float rayLenght = Mathf.Lerp(1.0f, 1.8f, Mathf.Abs(Vector3.Dot(_camera.transform.forward, Vector3.up)));

			//Cast a raycast
			hits = Physics.RaycastAll(ray, rayLenght, _interactiveMask);


			if(hits.Length > 0)
			{	   
				
				// record the highest priority of the interactive item
				int highestPriority = int.MinValue;
				InteractiveItem  priorityObject = null;

				for(int i=0; i<hits.Length; i++)
				{
					hit = hits[i];
					
					
					//Fetch its InteractiveItem script from dictionary
					InteractiveItem interactiveScript = _interactiveItemsManager.GetInteractiveItem(hit.collider.GetInstanceID());
					
					//  if this is the highest priority so far then remember it
					if(interactiveScript!=null && interactiveScript.ItemPriority > highestPriority)
					{
						priorityObject = interactiveScript;
						highestPriority = interactiveScript.ItemPriority;
					}
				}



				if(priorityObject != null)
				{
					if(_interactiveHUD)
					{
						_interactiveHUD.SetInteractionText(priorityObject.GetText());
					}
					if(Input.GetButtonDown("Use"))
					{
						priorityObject.Activate(this);
					}
				}
			}
			else
			{
				if(_interactiveHUD)
					_interactiveHUD.SetInteractionText(null);
			}
		}	
	}
}

