using UnityEngine;

namespace MB
{	
	[RequireComponent(typeof(BoxCollider))]
	public class InteractiveInfo : InteractiveItem 
	{
		[SerializeField] private string _infoText;

		public override string GetText()
		{
			return _infoText;
		}
	}
}

