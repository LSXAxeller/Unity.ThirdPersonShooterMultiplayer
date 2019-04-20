﻿using UnityEngine;

namespace StateAction
{
	[System.Serializable]
	public class StateProperties
	{
        public bool isLocal;
		public bool isAiming;
        public bool isVaulting;
        public bool isShooting;
        public bool isReloading;
        public bool isCrouching;
        public bool isInteracting;

        public void SetCrouching ()
        {
            isCrouching = !isCrouching;
        }

        public void SetReloading ()
        {
            isReloading = true;
        }
    }
}