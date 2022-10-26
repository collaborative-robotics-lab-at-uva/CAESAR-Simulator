using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MB
{	
	public class InteractiveWindow : InteractiveItem 
	{	
		[SerializeField] private Transform _window;
		[TextArea(3,10)]
		[SerializeField] private string _openText;
		[TextArea(3,10)]
		[SerializeField] private string _closeText;
		[SerializeField] private float _openDistance = 1f;
		[SerializeField] private float _OpeningSpeed = 0.5f;
		[SerializeField] private OpenAxis  _openAxis = OpenAxis.Up;
		[SerializeField] private OpenDirection _openDirection = OpenDirection.Negative;
		[SerializeField] private Method _method = Method.Coroutine;
		

		private bool _isOpen = false;
		private bool _firstUse = false;
		private Vector3 _closePosition;
		private Vector3 _openPosition;
		private IEnumerator _coroutine;
		private bool _isUpdate = false;

		private void Awake()
		{
			if(_method == Method.Update)
				_isUpdate = true;
			else
				_isUpdate = false;
		}
		
		protected override void Start()
		{
			base.Start();
			if(_window)
			{
				_closePosition = _window.transform.localPosition;
				if(_openAxis == OpenAxis.Foward)
				{
					if(_openDirection == OpenDirection.Positive)
						_openPosition = _closePosition + _window.transform.forward * _openDistance;
					else
						_openPosition = _closePosition + -_window.transform.forward * _openDistance;
				}			
				if(_openAxis == OpenAxis.Up)
				{	
					if(_openDirection == OpenDirection.Positive)
						_openPosition = _closePosition + _window.transform.up * _openDistance;
					else
						_openPosition = _closePosition + -_window.transform.up * _openDistance;
				}			
				if(_openAxis == OpenAxis.Right)
				{	
					if(_openDirection == OpenDirection.Positive)
						_openPosition = _closePosition + _window.transform.right * _openDistance;
					else
						_openPosition = _closePosition + -_window.transform.right * _openDistance;
				}
			}
			else
				Debug.LogError("Please insert object in the inspector");
		}

		private void Update()
		{
			if(_isUpdate)
			{
				if(_isOpen)
				{	
					if(!_firstUse) return;

					if((Vector3.Distance(_window.localPosition, _openPosition)) > 0.001f)
					{
						_window.localPosition = Vector3.Lerp(_window.localPosition, _openPosition, _OpeningSpeed * Time.deltaTime);			
					}	
												
				}
				else
				{  	
					if(!_firstUse) return;

					if((Vector3.Distance(_window.localPosition, _closePosition)) > 0.001f)
					{
						_window.localPosition = Vector3.Lerp(_window.localPosition, _closePosition, _OpeningSpeed * Time.deltaTime);
					}																					
				}
			}					
		}

		public override string GetText()
		{
			return !_isOpen? _closeText:_openText;
		}

		public override void Activate(PlayerInteractiveManager playerInteractiveManager)
		{	
			_coroutine = DoActivation();
			StartCoroutine(_coroutine);
		}

		private IEnumerator DoActivation()
		{	
			if(!_isUpdate)
			{
				if(!_isOpen)
				{	
					while((Vector3.Distance(_window.localPosition, _openPosition)) > 0.001f)
					{
						_window.localPosition = Vector3.Lerp(_window.localPosition, _openPosition, _OpeningSpeed * Time.deltaTime);
						yield return null;
					}									
				}
				else
				{  	
					while((Vector3.Distance(_window.localPosition, _closePosition)) > 0.001f)
					{
						_window.localPosition = Vector3.Lerp(_window.localPosition, _closePosition, _OpeningSpeed * Time.deltaTime);
						yield return null;
					}																					
				}
			}
			_isOpen = !_isOpen;
			_firstUse = true;	
		}
	}
}

