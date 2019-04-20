﻿using System.Collections;
using UnityEngine;

namespace Multiplayer
{
    public class MultiplayerListener : Photon.MonoBehaviour
    {
        [Header ("States")]
        public StateAction.State localState;
        public StateAction.StateActions initLocalPlayer;
        public StateAction.State clientState;
        public StateAction.StateActions initClientPlayer;

        [Header ("Prediction")]
        public float snapAngle = 40f;
        public float snapDistance = 4f;
        public float predictionSpeed = 10f;
        public float angleThreshold = 0.05f;
        public float movementThreshold = 0.05f;

        private Vector3 lastPosition;
        private Vector3 lastDirection;
        private Quaternion lastRotation;
        private Vector3 targetAimPosition;

        Transform transformInstance;


        private StateAction.StateManager states;

        private void OnPhotonInstantiate (PhotonMessageInfo messageInfo)
        {
            states = GetComponent<StateAction.StateManager> ();
            transformInstance = this.transform;

            if (photonView.isMine)
            {
                states.currentState.isLocal = true;
                states.SetCurrentState (localState);
                initLocalPlayer.Execute (states);
            }
            else
            {
                states.currentState.isLocal = false;
                states.SetCurrentState (clientState);
                initClientPlayer.Execute (states);
                states.multiplayerListener = this;
            }
        }

        public void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo messageInfo)
        {
            if (stream.isWriting)
            {
                stream.SendNext (transformInstance.position);
                stream.SendNext (transformInstance.rotation);

                stream.SendNext (states.movementProperties.vertical);
                stream.SendNext (states.movementProperties.horizontal);

                stream.SendNext (states.currentState.isAiming);
                stream.SendNext (states.currentState.isCrouching);
            }
            else
            {
                Vector3 position = (Vector3)stream.ReceiveNext ();
                Quaternion rotation = (Quaternion)stream.ReceiveNext ();

                ReceivePositionRotation (position, rotation);

                states.movementProperties.vertical = (float)stream.ReceiveNext ();
                states.movementProperties.horizontal = (float)stream.ReceiveNext ();

                states.currentState.isAiming = (bool)stream.ReceiveNext ();
                states.currentState.isCrouching = (bool)stream.ReceiveNext ();
            }
        }

        #region Prediction

        public void Prediction ()
        {
            Vector3 currentPosition = transformInstance.position;
            Quaternion currentRotation = transformInstance.rotation;

            float distance = Vector3.Distance (lastPosition, currentPosition);
            float angle = Vector3.Angle (lastRotation.eulerAngles, currentRotation.eulerAngles);

            if (distance > snapDistance)
                transformInstance.position = lastPosition;

            if (angle > snapAngle)
                transformInstance.rotation = lastRotation;

            currentPosition += lastDirection;
            currentRotation *= lastRotation;

            transformInstance.position = Vector3.Lerp (currentPosition, lastPosition, predictionSpeed * states.deltaTime);
            transformInstance.rotation = Quaternion.Slerp (currentRotation, lastRotation, predictionSpeed * states.deltaTime);
        }

        private void ReceivePositionRotation (Vector3 position, Quaternion rotation)
        {
            lastDirection = position - lastPosition;
            lastDirection /= 10;

            if (lastDirection.magnitude > movementThreshold)
                lastDirection = Vector3.zero;

            Vector3 lastEuler = lastRotation.eulerAngles;
            Vector3 newEuler = rotation.eulerAngles;

            if (Quaternion.Angle (lastRotation, rotation) < angleThreshold)
                lastRotation = Quaternion.Euler ((newEuler - lastEuler) / 10);
            else lastRotation = Quaternion.identity;

            lastPosition = position;
            lastRotation = rotation;
        }

        #endregion
    }
}