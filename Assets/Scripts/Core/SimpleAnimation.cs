using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace DKK
{
	public class SimpleAnimation : MonoBehaviour
	{
		public enum AnimationType
		{
			NoMotion = -1,
			LeftToRight = 0,
			RightToLeft = 1,
			TopToBottom = 2,
			BottomToTop = 3,
            Pulsing = 4,
		}

		public AnimationType animationType;

		protected Vector3 startPosition;
		public float shiftAmount = 50, speed = 3f, animationDelay = 0f;
		protected float animationDelayTimer = 0f;
		protected RectTransform rectTrans;
		protected bool allow = false;
		public bool withAlphaFading = false, startOnEnable = true;
		private bool noMotion = false;
		protected CanvasGroup canvasGroup;

        public bool playOnce = false;
        private bool wasPlayed = false;

        private bool wasPreInitialized = false;

        public bool intitializeOnAwake = true;

		void Awake ()
		{
            if (!intitializeOnAwake)
            {
                return;
            }

            PreInitialize();			
		}

        public void PreInitialize()
        {
            rectTrans = GetComponent<RectTransform>();
            startPosition = rectTrans.anchoredPosition3D;
            Initialize(animationType, shiftAmount, speed, animationDelay, withAlphaFading);
            wasPreInitialized = true;
        }

		void OnEnable ()
		{
            AnimateCondition();
		}

        public void RemoteAnimation()
        {          
            AnimateCondition();
        }

        public void AnimateCondition()
        {

            if (!wasPreInitialized)
                return;

            if (playOnce && wasPlayed)
                return;

            if (startOnEnable && rectTrans != null)
            {

                Animate();
            }
        }

		public void Animate ()
		{
//			return;
			if (withAlphaFading)
				canvasGroup.alpha = 0;

			animationDelayTimer = animationDelay;
            //rectTrans.DORewind();


            switch (animationType) {
			case AnimationType.NoMotion:
				noMotion = true;
				break;
			case AnimationType.BottomToTop:
				rectTrans.anchoredPosition3D = new Vector3 (startPosition.x, startPosition.y - shiftAmount, startPosition.z);
				break;
			case AnimationType.TopToBottom:
				rectTrans.anchoredPosition3D = new Vector3 (startPosition.x, startPosition.y + shiftAmount, startPosition.z);
				break;
			case AnimationType.LeftToRight:
				rectTrans.anchoredPosition3D = new Vector3 (startPosition.x - shiftAmount, startPosition.y, startPosition.z);
				break;
			case AnimationType.RightToLeft:
				rectTrans.anchoredPosition3D = new Vector3 (startPosition.x + shiftAmount, startPosition.y, startPosition.z);
				break;
                case AnimationType.Pulsing:
                    rectTrans.DOScale(1.2f, 1f).SetLoops(-1,LoopType.Yoyo);
                    break;
			}

			allow = true;
            wasPlayed = true;
		}

		/// <summary>
		/// Initialize the specified type, shiftAmount, speed, animationDelay, withAlphaFading and playNow.
		/// </summary>
		/// <param name="type">Animation type.</param>
		/// <param name="shiftAmount">Maximum position far from start.</param>
		/// <param name="speed">Speed of animation.</param>
		/// <param name="animationDelay">Delay animation (sec).</param>
		/// <param name="withAlphaFading">Enable alpha fading animation for object. Adds CanvasGroup. </param>
		/// <param name="playNow">Force animation after Initialization</param>
		public void Initialize (AnimationType type, float shiftAmount = -1f, float speed = -1f, float animationDelay = -1f, bool withAlphaFading = false, bool playNow = false)
		{
//			return;
//			"Animation INIT".Log ();
			this.animationType = type;

			if (shiftAmount >= 0)
				this.shiftAmount = shiftAmount;
			
			if (speed >= 0)
				this.speed = speed;
			
			if (animationDelay >= 0)
				this.animationDelay = animationDelay;
			
			this.withAlphaFading = withAlphaFading;

			if (withAlphaFading) {
				if (GetComponent<CanvasGroup> () == null) {			
					canvasGroup = gameObject.AddComponent<CanvasGroup> ();
				} else
					canvasGroup = gameObject.GetComponent<CanvasGroup> ();
			}
		

			if (gameObject.activeSelf && playNow && rectTrans!=null)
				Animate ();
		}

        public void ResetAnimation() {

            if (!wasPreInitialized)
                return;

            if (rectTrans != null)
            {
                rectTrans.anchoredPosition3D = startPosition;
                if (GetComponent<CanvasGroup>() != null)
                {
                    GetComponent<CanvasGroup>().alpha = 0f;
                }
            }
        }

		void OnDisable ()
		{
            //			return;
            if (startOnEnable && rectTrans != null)
            {
                ResetAnimation();
            }

			//		"Disable".Log ();
		}

		void Update ()
		{
//			return;
			if (animationDelayTimer > 0) {
				animationDelayTimer -= Time.deltaTime;
				return;
			}

			if (allow) {

				if (withAlphaFading)
				if (canvasGroup.alpha < 1)
					canvasGroup.alpha += Time.deltaTime * speed;

				if (!noMotion)
					rectTrans.anchoredPosition3D = Vector3.Lerp (rectTrans.anchoredPosition3D, startPosition, Time.deltaTime * speed);
				if (Vector3.Distance (rectTrans.anchoredPosition3D, startPosition) < 0.2f) {
					rectTrans.anchoredPosition3D = startPosition;

					if ((withAlphaFading && canvasGroup.alpha >= 1) || !withAlphaFading)
						allow = false;
				}
			}
		}
	}
}
