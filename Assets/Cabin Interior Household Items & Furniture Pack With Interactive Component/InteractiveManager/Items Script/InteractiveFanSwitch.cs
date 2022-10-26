using System.Collections;
using UnityEngine;

namespace MB
{
	public class InteractiveFanSwitch : InteractiveItem 
	{
		[SerializeField] private Transform _switch;
		[SerializeField] private Transform _targetObject;
		[TextArea(3,10)]
		[SerializeField] private string _switchOnText;
		[TextArea(3,10)]
		[SerializeField] private string _switchOffText;
		[SerializeField] private float _speed = 360.0f;

		private Animator _switchAnimator = null;
		private bool _isActive = false;
		private bool _firsActive = false;
		private float _currentSpeed = 0.0f;
		private IEnumerator _coroutine = null;


		protected override void Start()
		{
			base.Start();
			if(_switch)
			{
				_switchAnimator = _switch.GetComponent<Animator>();
			}
		}

		private void Update()
		{
			if(!_isActive)
			{	
				if(!_firsActive)
					_targetObject.Rotate(Vector3.up, 0 * Time.deltaTime);
				else
				{	
					if(_currentSpeed == 0) return;
					_currentSpeed =  Mathf.Lerp(_currentSpeed, 0.0f, 0.2f * Time.deltaTime);
					_targetObject.Rotate(Vector3.up, _currentSpeed * Time.deltaTime);					
				}
			}
			else
				_targetObject.Rotate(Vector3.up, _speed * Time.deltaTime);	
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
			{
				if(!_isActive)
					_switchAnimator.SetBool("Activate", true);
				else
					_switchAnimator.SetBool("Activate", false); 
			}
				
			_isActive = !_isActive;

			if(_isActive)
				_currentSpeed = _speed;
			
			_firsActive = true;
			yield return null;
		}
	}
}

