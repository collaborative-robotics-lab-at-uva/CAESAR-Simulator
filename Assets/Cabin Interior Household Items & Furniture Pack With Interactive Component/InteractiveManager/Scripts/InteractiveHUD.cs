using UnityEngine;
using UnityEngine.UI;
//using TMPro;

public class InteractiveHUD : MonoBehaviour {
	//[SerializeField]private TextMeshProUGUI _interactiveText;
	[SerializeField]private Text _interactiveText;

	public void SetInteractionText(string text)
	{
		if(_interactiveText)
		{
			if(text==null)
			{
				//_interactiveText.SetText("");
				_interactiveText.text = "";
				_interactiveText.gameObject.SetActive(false);
			}
			else
			{
				//_interactiveText.SetText(text);
				_interactiveText.text = text;
				_interactiveText.gameObject.SetActive(true);
			}
		}
	}	
}
