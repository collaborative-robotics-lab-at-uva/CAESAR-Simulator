using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MB
{
	public class HomesafeInteractive : InteractiveItem 
	{
		[TextArea(3,10)]
		[SerializeField] private string _openText;
		[TextArea(3,10)]
		[SerializeField] private string _closeText;
		[SerializeField] private string _passcodeText;
		[SerializeField] private string _passcode = "0000";
		[SerializeField] private Transform _door;
		[SerializeField] private float _openDegree = 70.0f;
		[SerializeField] private float _OpeningSpeed = 5.0f;

		private bool _isLocked = true;
		private bool _enterPasscode = false;
		private string _input;
		private string _cachePasscode;
		//private bool _firstUse = false;
		private bool _isOpen;
		private Quaternion _closeRotation;
		private Quaternion _openRotation;

		protected override void Start()
		{
			base.Start();
			if(_door)
			{	
				_closeRotation = _door.transform.localRotation;
				_openRotation = Quaternion.Euler(0,_openDegree,0);
			}
		}

		private void Update()
		{	
			//Debug.Log(_input);

			if(_enterPasscode)
			{	
				_input = Input.inputString;

				foreach(char c in _input)
				{
					if ((c == '\n') || (c == '\r')) // enter/return
					{	
						if(_cachePasscode != _passcode)
							Reset();
						else
							DoOpening();				
					}
				}

				_cachePasscode += _input;
				_passcodeText = _cachePasscode;
				
				//Debug.Log(_cachePasscode.Length);
				if(Input.GetButtonDown("Clear"))
				{
					Clear(out _cachePasscode);
					Clear(out _input);
				}				
			}


			if(_isOpen)
			{	
				//if(!_firstUse) return;

				if(Quaternion.Angle(_door.localRotation, _openRotation) > 0.001f)
				{
					_door.localRotation = Quaternion.Lerp(_door.localRotation, _openRotation, _OpeningSpeed * Time.deltaTime);
				}					
			}
			else
			{  
				//if(!_firstUse) return;

				if(Quaternion.Angle(_door.localRotation, _closeRotation) > 0.001f)
				{
					_door.localRotation = Quaternion.Lerp(_door.localRotation, _closeRotation, _OpeningSpeed * Time.deltaTime);
				}																			
			}
				
		}

		public override string GetText()
		{	
			if(_isLocked && !_enterPasscode )
				return _closeText;
			else if(_enterPasscode)
				return _passcodeText;
			else if(!_isLocked && _isOpen)
				return _openText;
			else
				return _closeText;
		}

		public override void Activate(PlayerInteractiveManager playerInteractiveManager)
		{	
			if(!_enterPasscode && !_isOpen)
			{
				StartCoroutine(Initiate());
			}
			
			if(!_enterPasscode && _isOpen)
				Reset();
				
			
		}

		private void Clear(out string s)
		{	
			s = null;
		}

		IEnumerator Initiate()
		{
			yield return new WaitForEndOfFrame();
			_enterPasscode = true;
		}

		private void DoOpening()
		{	
			Debug.Log(_cachePasscode);
			Debug.Log("Open");
			_isLocked = false;
			_isOpen = true;
			_enterPasscode = false;
		}

		private void Reset()
		{	
			Clear(out _input);
			Clear(out _cachePasscode);
			Debug.Log("Reset");
			_enterPasscode = false;	
			_isOpen = false;
			_passcodeText = "";
		}
	}
}