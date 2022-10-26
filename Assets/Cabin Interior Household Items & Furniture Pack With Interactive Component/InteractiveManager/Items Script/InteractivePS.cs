using UnityEngine;
using UnityStandardAssets.Utility;
using System.Collections.Generic;

namespace MB
{	
	[RequireComponent(typeof(AudioSource))]
	public class InteractivePS : InteractiveItem 
	{	
		[System.Serializable]
		struct ParticleSystemData {
			public float _minDuration;
			public float _maxDuration;
		}

		[Header("Target Place")]
		[SerializeField] private List<Transform> _targetPlace = new List <Transform>();
		[Header("Particle System Destroyer")]
		[SerializeField] private List<ParticleSystemDestroyer> _particleSystemDestroyer = new List<ParticleSystemDestroyer>();
		[Header("Particle System Data")]
		[Tooltip("Assign data for each of particle system")]
		[SerializeField] private List<ParticleSystemData> _particleSystemData = new List<ParticleSystemData>();
		[SerializeField] private AudioClip _particleSystemSFX;
		[TextArea(3,10)]
		[SerializeField] private string _activeText;
		[TextArea(3,10)]
		[SerializeField] private string _notActiveText;

		private bool _isActive = false;
		private ParticleSystemDestroyer[] _newParticleSystemDestroyer;
		private AudioSource _audioSource;

		private void Awake()
		{	
			if(_particleSystemDestroyer.Count == 0) return;
			_newParticleSystemDestroyer = new ParticleSystemDestroyer[_particleSystemDestroyer.Count];
		}

		protected override void Start()
		{
			base.Start();
			_audioSource = GetComponent<AudioSource>();
		}

		public override string GetText()
		{
			return !_isActive? _notActiveText:_activeText;
		}

		public override void Activate(PlayerInteractiveManager playerInteractiveManager)
		{
			if(!_isActive)
				DoActivation();
			else
				DoDeactivation();
		}

		private void DoActivation()
		{
			for(int i = 0; i < _particleSystemDestroyer.Count; i++)
			{
				_particleSystemDestroyer[i].minDuration = _particleSystemData[i]._minDuration;
				_particleSystemDestroyer[i].maxDuration = _particleSystemData[i]._maxDuration;

				_newParticleSystemDestroyer[i] = (ParticleSystemDestroyer)Instantiate(_particleSystemDestroyer[i], _targetPlace[i].position, _targetPlace[i].rotation);
				_newParticleSystemDestroyer[i].transform.SetParent(_targetPlace[i]);
			}

			_audioSource.clip = _particleSystemSFX;
			_audioSource.Play();

			_isActive = true;
		}

		private void DoDeactivation()
		{
			for(int i = 0; i < _newParticleSystemDestroyer.Length; i++)
			{
				_newParticleSystemDestroyer[i].Stop();
			}
		}

		private void OnEnable()
		{
			ParticleSystemDestroyer.OnStop += StopSound;
		}

		private void OnDisable()
		{
			ParticleSystemDestroyer.OnStop -= StopSound;
		}

		private void StopSound()
		{
			_audioSource.Stop();
			_isActive = false;
		}
	}
}

