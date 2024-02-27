using System;
using Spine.Unity;
using UnityEngine;

namespace finished3
{
    public class CharacterInfo : MonoBehaviour
    {
        public OverlayTile standingOnTile;
        public string axieId;
        public int Range = 1;
        public CharacterInfo CurrentTarget;
        public SkeletonAnimation SkeletonAnim;
        public bool Grabbed = false;

        public bool beingHovered;

        public MouseController movementController;

        private MouseController.CharacterState state;

        private void Start()
        {
            movementController = FindObjectOfType<MouseController>();
            SkeletonAnim = transform.GetChild(0).GetComponent<SkeletonAnimation>();
            state = movementController.GetCharacterState(axieId);
        }

        private void OnMouseOver()
        {
            // This method is called when the mouse is over the collider of this GameObject
            beingHovered = true;
        }

        private void OnMouseExit()
        {
            // This method is called when the mouse leaves the collider
            beingHovered = false;
        }

        private void Update()
        {
            if (Grabbed)
            {
                if (SkeletonAnim.AnimationName == "action/idle/random-03")
                    return;
                SkeletonAnim.AnimationName = "action/idle/random-03";
                SkeletonAnim.loop = true;
            }
            else if (beingHovered)
            {
                if (SkeletonAnim.AnimationName == "action/idle/random-01")
                    return;
                SkeletonAnim.AnimationName = "action/idle/random-01";
                SkeletonAnim.loop = true;
            }
            else if (state.isMoving == false)
            {
                if (SkeletonAnim.AnimationName == "action/idle/normal")
                    return;
                SkeletonAnim.AnimationName = "action/idle/normal";
                SkeletonAnim.loop = true;
            }

            if (CurrentTarget != null && state.isMoving == false)
            {
                if (SkeletonAnim.AnimationName == "attack/melee/tail-roll")
                    return;
                SkeletonAnim.AnimationName = "attack/melee/tail-roll";
                SkeletonAnim.loop = true;
                return;
            }
            else if (state.isMoving)
            {
                if (SkeletonAnim.AnimationName == "action/run")
                    return;
                SkeletonAnim.AnimationName = "action/run";
                SkeletonAnim.loop = true;
                return;
            }
        }
    }
}