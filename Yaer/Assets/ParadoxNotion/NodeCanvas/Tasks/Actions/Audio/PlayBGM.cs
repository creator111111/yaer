using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Tasks.Actions
{

    [Category("Audio")]
    public class PlayBGM : ActionTask<Transform> { 

        [RequiredField]
        public BBParameter<AudioClip> bgmClip;
		public BBParameter<AudioSource> audioSource;
		public BBParameter<float> volume = 1;
        public bool waitActionFinish;
        protected override string info {
            get { return "PlayBGM " + bgmClip.ToString(); }
        }

        protected override void OnExecute() {
			if (audioSource.value != null && bgmClip.value != null) {
				audioSource.value.clip = bgmClip.value;
				audioSource.value.loop = true;
                audioSource.value.volume = volume.value;
				audioSource.value.Play();
			}

			if ( !waitActionFinish )
                EndAction();
        }

        protected override void OnUpdate() {
            if ( elapsedTime >= bgmClip.value.length )
                EndAction();
        }
		protected override void OnStop() {
			base.OnStop();

		}
	}
}