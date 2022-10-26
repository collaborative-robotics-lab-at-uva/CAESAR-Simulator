using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MB
{	
	public class InteractiveDrawer : InteractiveItem 
	{	
		[SerializeField] private Transform _drawer;
		[TextArea(3,10)]
		[SerializeField] private string _openText;
		[TextArea(3,10)]
		[SerializeField] private string _closeText;
		[SerializeField] private float _openDistance = 1f;
		[SerializeField] private float _OpeningSpeed = 0.5f;
		[SerializeField] private OpenAxis  _openAxis = OpenAxis.Foward;
		[SerializeField] private OpenDirection _openDirection = OpenDirection.Positive;
		[SerializeField] private Transform _contentsMount;
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
			if(_drawer)
			{
				_closePosition = _drawer.transform.localPosition;
				if(_openAxis == OpenAxis.Foward)
				{
					if(_openDirection == OpenDirection.Positive)
						_openPosition = _closePosition + _drawer.transform.forward * _openDistance;
					else
						_openPosition = _closePosition + -_drawer.transform.forward * _openDistance;
				}			
				if(_openAxis == OpenAxis.Up)
				{	
					if(_openDirection == OpenDirection.Positive)
						_openPosition = _closePosition + _drawer.transform.up * _openDistance;
					else
						_openPosition = _closePosition + -_drawer.transform.up * _openDistance;
				}			
				if(_openAxis == OpenAxis.Right)
				{	
					if(_openDirection == OpenDirection.Positive)
						_openPosition = _closePosition + _drawer.transform.right * _openDistance;
					else
						_openPosition = _closePosition + -_drawer.transform.right * _openDistance;
				}
			}
			else
				Debug.LogError("Please insert drawer in the inspector");
			
			if(_contentsMount != null)
			{
				Collider[] colliders = _contentsMount.GetComponentsInChildren<Collider>();
				if(colliders != null)
				{
					foreach(Collider c in colliders)
					{
						if(!_isOpen)
							c.enabled = false;
						else
							c.enabled = true;
					}
				}
			}
		}

		private void Update()
		{	
			if(_isUpdate)
			{
				if(_isOpen)
				{	
					if(!_firstUse) return;

					if((Vector3.Distance(_drawer.localPosition, _openPosition)) > 0.001f)
					{
						_drawer.localPosition = Vector3.Lerp(_drawer.localPosition, _openPosition, _OpeningSpeed * Time.deltaTime);
					}
					if(_contentsMount != null)
					{
						Collider[] colliders = _contentsMount.GetComponentsInChildren<Collider>();
						if(colliders != null)
						{
							foreach(Collider c in colliders)
							{				
								c.enabled = true;
							}
						}			
					}		
												
				}
				else
				{  
					if(!_firstUse) return;

					if((Vector3.Distance(_drawer.localPosition, _closePosition)) > 0.001f)
					{
						_drawer.localPosition = Vector3.Lerp(_drawer.localPosition, _closePosition, _OpeningSpeed * Time.deltaTime);
					}
					if(_contentsMount != null)
					{
						Collider[] colliders = _contentsMount.GetComponentsInChildren<Collider>();
						if(colliders != null)
						{
							foreach(Collider c in colliders)
							{				
								c.enabled = false;
							}
						}
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
					while((Vector3.Distance(_drawer.localPosition, _openPosition)) > 0.001f)
					{
						_drawer.localPosition = Vector3.Lerp(_drawer.localPosition, _openPosition, _OpeningSpeed * Time.deltaTime);
						if(_contentsMount != null)
						{
							Collider[] colliders = _contentsMount.GetComponentsInChildren<Collider>();
							if(colliders != null)
							{
								foreach(Collider c in colliders)
								{				
									c.enabled = false;
								}
							}
						}	
						yield return null;
					}	
												
				}
				else
				{  
					while((Vector3.Distance(_drawer.localPosition, _closePosition)) > 0.001f)
					{
						_drawer.localPosition = Vector3.Lerp(_drawer.localPosition, _closePosition, _OpeningSpeed * Time.deltaTime);
						if(_contentsMount != null)
						{
							Collider[] colliders = _contentsMount.GetComponentsInChildren<Collider>();
							if(colliders != null)
							{
								foreach(Collider c in colliders)
								{				
									c.enabled = true;
								}
							}
						}	
						yield return null;
					}																					
				}
			}	
			_isOpen = !_isOpen;
			_firstUse = true;				
		}
	}
}

