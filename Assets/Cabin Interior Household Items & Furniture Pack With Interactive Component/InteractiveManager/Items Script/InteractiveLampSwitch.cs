using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MB
{
	public class InteractiveLampSwitch : InteractiveItem 
	{
		[SerializeField] private Transform _switch;
		[SerializeField] private List<GameObject> _targetObject = new List<GameObject>();
		[TextArea(3,10)]
		[SerializeField] private string _switchOnText;
		[TextArea(3,10)]
		[SerializeField] private string _switchOffText;
		[SerializeField] private float _lightIntensity;

		private Animator _switchAnimator;
		private IEnumerator _coroutine;
		private bool _isActive = false;

		protected override void Start()
		{
			base.Start();
			_switchAnimator = _switch.GetComponent<Animator>();
			for(int i = 0; i < _targetObject.Count; i++)
			{
				_targetObject[i].SetActive(false);
				Light _light = _targetObject[i].GetComponent<Light>();
				_light.intensity = _lightIntensity;
			}
		}

		public override string GetText()
		{
			return !_isActive? _switchOffText:_switchOnText;
		}

		public override void Activate(PlayerInteractiveManager playerInteractiveManager)
		{
			_coroutine = DoActivation();
			StartCoroutine(_coroutine);
		}

		private IEnumerator DoActivation()
		{
			if(_switchAnimator)
				if(!_isActive)
				{
					_switchAnimator.SetBool("Activate", true);
					Set();
				}		
				else
				{
					_switchAnimator.SetBool("Activate", false);
					Set();
				}
				
			
			_isActive = !_isActive;
			yield return null;
		}

		private void Set()
		{
			for(int i = 0; i < _targetObject.Count; i++)
			{
				if(!_isActive)
					_targetObject[i].SetActive(true);
				else
					_targetObject[i].SetActive(false);
			}
		}
	}
}

