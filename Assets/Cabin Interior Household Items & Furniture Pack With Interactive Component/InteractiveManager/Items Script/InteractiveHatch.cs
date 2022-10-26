using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MB
{	
	[RequireComponent(typeof(BoxCollider))]
	public class InteractiveHatch : InteractiveItem 
	{
		[SerializeField] private Transform _cabinetDoor;
		[TextArea(3,10)]
		[SerializeField] private string _openText;
		[TextArea(3,10)]
		[SerializeField] private string _closeText;
		[SerializeField] private float _openDegree = 80.0f;
		[SerializeField] private float _OpeningSpeed = 5.0f;
		[SerializeField] private OpenAxis  _openAxis = OpenAxis.Foward;
		[SerializeField] private Transform _contentsMount;
		[SerializeField] private OpenDirection _openDirection = OpenDirection.Positive;
		[SerializeField] private Method _method = Method.Coroutine;

		private bool _isOpen;
		private Quaternion _closeRotation;
		private Quaternion _openRotation;
		private IEnumerator _coroutine;
		private bool _isUpdate = false;
		private bool _firstUse = false;

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
			if(_cabinetDoor)
			{
				_closeRotation = _cabinetDoor.transform.localRotation;
				if(_openAxis == OpenAxis.Foward)
				{
					if(_openDirection == OpenDirection.Positive)
						_openRotation = Quaternion.Euler(0,0,_openDegree);
					else
						_openRotation = Quaternion.Euler(0,0,-_openDegree);
				}			
				if(_openAxis == OpenAxis.Up)
				{	
					if(_openDirection == OpenDirection.Positive)
						_openRotation = Quaternion.Euler(0,_openDegree,0);
					else
						_openRotation = Quaternion.Euler(0,-_openDegree,0);
				}			
				if(_openAxis == OpenAxis.Right)
				{	
					if(_openDirection == OpenDirection.Positive)
						_openRotation = Quaternion.Euler(_openDegree,0,0);
					else
						_openRotation = Quaternion.Euler(-_openDegree,0,0);
				}
			}

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

					if(Quaternion.Angle(_cabinetDoor.localRotation, _openRotation) > 0.001f)
					{
						_cabinetDoor.localRotation = Quaternion.Lerp(_cabinetDoor.localRotation, _openRotation, _OpeningSpeed * Time.deltaTime);
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

					if(Quaternion.Angle(_cabinetDoor.localRotation, _closeRotation) > 0.001f)
					{
						_cabinetDoor.localRotation = Quaternion.Lerp(_cabinetDoor.localRotation, _closeRotation, _OpeningSpeed * Time.deltaTime);
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
				  while(Quaternion.Angle(_cabinetDoor.localRotation, _openRotation) > 0.001f)
				  {
					  _cabinetDoor.localRotation = Quaternion.Lerp(_cabinetDoor.localRotation, _openRotation, _OpeningSpeed * Time.deltaTime);
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
				  while(Quaternion.Angle(_cabinetDoor.localRotation, _closeRotation) > 0.001f)
				  {
					  _cabinetDoor.localRotation = Quaternion.Lerp(_cabinetDoor.localRotation, _closeRotation, _OpeningSpeed * Time.deltaTime);
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

