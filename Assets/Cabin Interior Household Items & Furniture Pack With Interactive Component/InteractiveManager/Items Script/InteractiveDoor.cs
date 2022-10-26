using System.Collections;
using UnityEngine;

namespace MB
{	
	[RequireComponent(typeof(AudioSource))]
	public class InteractiveDoor : InteractiveItem 
	{
		[SerializeField] private Transform _doorhandle_back;
		[SerializeField] private Transform _doorhandle_front;
		[SerializeField] private Transform _door;

		[TextArea (3, 10)]
		[SerializeField] private string _closedText = null;
		[TextArea (3, 10)]
		[SerializeField] private string _openingText = null;
		[TextArea (3, 10)]
		[SerializeField] private string _closingText = null;
		[TextArea (3, 10)]
		[SerializeField] private string _openedText = null;
		[SerializeField] private AudioClip  _door_open_sfx = null;
		[SerializeField] private AudioClip  _door_close_sfx = null;
		[SerializeField] private float _duration = 1.0f;


		private AudioSource _audioSource;
		private Animator _doorhandle_back_animator = null;
		private Animator _doorhandle_front_animator = null;
		private Animator _door_animator = null;
		private IEnumerator _coroutine = null;
		private bool _isOpen = false;

		protected override void Start()
		{
			base.Start();
			if(_doorhandle_back)
				_doorhandle_back_animator = _doorhandle_back.GetComponent<Animator>();
			if(_doorhandle_front)
				_doorhandle_front_animator = _doorhandle_front.GetComponent<Animator>();
			if(_door)
				_door_animator = _door.GetComponent<Animator>();
			_audioSource = GetComponent<AudioSource>();
		}


		public override string GetText()
		{	
			if(_coroutine!= null)
				return !_isOpen? _openingText:_closingText;
			else if(!_isOpen)
				return _closedText;
			else
				return _openedText;
				
		}

		public override void Activate(PlayerInteractiveManager playerInteractiveManager)
		{	
			if(_coroutine!=null) return;
			_coroutine = TurningHandleDown();
			StartCoroutine(_coroutine);
		}

		private IEnumerator TurningHandleDown()
		{
			if(_doorhandle_back_animator)	
				_doorhandle_back_animator.SetTrigger("Activate");
			if(_doorhandle_front)
				_doorhandle_front_animator.SetTrigger("Activate");
				
			yield return new WaitForSeconds(_duration);

			if(!_isOpen)
			{
				_coroutine = OpenTheDoor();
				if(_door_open_sfx)
					_audioSource.clip = _door_open_sfx;
			}
				
			else
			{
				_coroutine = CloseTheDoor();
				if(_door_close_sfx)
					_audioSource.clip = _door_close_sfx;
			}
			_isOpen = !_isOpen;

			StartCoroutine(_coroutine);
			_audioSource.Play();
		}

		private IEnumerator OpenTheDoor()
		{	
			if(_door_animator)
				_door_animator.SetBool("Open", true);
			yield return new WaitForSeconds(_duration);
			_coroutine = null;
		}

		private IEnumerator CloseTheDoor()
		{
			if(_door_animator)
				_door_animator.SetBool("Open", false);
			yield return new WaitForSeconds(_duration);
			_coroutine = null;
		}

		
	}
}

