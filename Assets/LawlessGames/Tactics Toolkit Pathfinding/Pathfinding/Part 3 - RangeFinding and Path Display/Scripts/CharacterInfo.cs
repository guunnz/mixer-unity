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

        public MyTeam movementController;
        public EnemyTeam movementController2;

        private MyTeam.CharacterState state;

        private void Start()
        {
            if (this.standingOnTile.grid2DLocation.x >= 4)
            {
                movementController2 = FindObjectOfType<EnemyTeam>();
                state = movementController2.GetCharacterState(axieId);
            }
            else
            {
                movementController = FindObjectOfType<MyTeam>();
                state = movementController.GetCharacterState(axieId);
            }

            SkeletonAnim = transform.GetChild(0).GetComponent<SkeletonAnimation>();
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
            if (state == null)
                return;

            if (Grabbed)
            {
                CurrentTarget = null;
                SkeletonAnim.AnimationName = "action/idle/random-03";
                SkeletonAnim.loop = true;
                return;
            }
            else if (beingHovered && CurrentTarget == null)
            {
                SkeletonAnim.AnimationName = "action/idle/random-01";
                SkeletonAnim.loop = true;
                return;
            }
            else if (state.isMoving == false && CurrentTarget == null)
            {
                SkeletonAnim.AnimationName = "action/idle/normal";
                SkeletonAnim.loop = true;
                return;
            }

            if (CurrentTarget != null && state.isMoving == false &&
                Math.Round(Vector2.Distance(CurrentTarget.standingOnTile.grid2DLocation,
                    standingOnTile.grid2DLocation)) <= (Range + 0.3f))
            {
                SkeletonAnim.AnimationName = "attack/melee/tail-roll";
                SkeletonAnim.loop = true;
                return;
            }
            else if (state.isMoving)
            {
                SkeletonAnim.AnimationName = "action/run";
                SkeletonAnim.loop = true;
                return;
            }
            else
            {
                SkeletonAnim.AnimationName = "action/idle/normal";
                SkeletonAnim.loop = true;
            }
        }
    }
}