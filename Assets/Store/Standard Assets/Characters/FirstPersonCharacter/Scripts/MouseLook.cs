using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.FirstPerson {
    [Serializable]
    public class MouseLook {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;
        public bool lockCursor = true;


        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;

        public void Init(Transform character, Transform camera) {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }


        public void LookRotation(Transform character, Transform camera) {
			if (m_cursorIsLocked) {
	            float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
	            float xRot = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;

	            m_CharacterTargetRot *= Quaternion.Euler (0, yRot, 0f);
	            m_CameraTargetRot *= Quaternion.Euler (-xRot, 0f, 0f);

	            if(clampVerticalRotation)
	                m_CameraTargetRot = ClampRotationAroundXAxis (m_CameraTargetRot);

	            if(smooth) {
	                character.localRotation = Quaternion.Slerp (character.localRotation, m_CharacterTargetRot, smoothTime * Time.deltaTime);
	                camera.localRotation = Quaternion.Slerp (camera.localRotation, m_CameraTargetRot, smoothTime * Time.deltaTime);
	            }
	            else {
	                character.localRotation = m_CharacterTargetRot;
	                camera.localRotation = m_CameraTargetRot;
					/*Vector3 fixedpos = character.position;
					fixedpos.y += 1.8f;
					Vector3 oldangle = camera.eulerAngles;
					if (oldangle.x < 5f && camera.eulerAngles.x >= 180f) {
						xRot = 0f;
						oldangle.x = 0.1f;
						camera.eulerAngles = oldangle;
					}
					else if (oldangle.x > 175f && camera.eulerAngles.x >= 180f) {
						xRot = 0f;
						oldangle.x = 179.9f;
						camera.eulerAngles = oldangle;
					} 
					camera.RotateAround(fixedpos, character.right, -xRot);*/


	            }
			}
            UpdateCursorLock();

        }

        public void SetCursorLock(bool value) {
            lockCursor = value;
            if(!lockCursor) {
				//we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
			UpdateCursorLock();
        }

        public void UpdateCursorLock() {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
                InternalLockUpdate();
        }

        private void InternalLockUpdate() {
			//m_cursorIsLocked = m_cursorIsLocked ? false : true;
            if(Input.GetKeyUp(KeyCode.Escape)) {
				m_cursorIsLocked = m_cursorIsLocked ? false : true;
            }

            if (m_cursorIsLocked) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_cursorIsLocked) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q) {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}
