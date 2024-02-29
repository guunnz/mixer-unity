using System;
using Spine.Unity;
using UnityEngine;

namespace finished3
{
    public class CharacterInfo : MonoBehaviour
    {
        public OverlayTile standingOnTile;
        public string axieId;
        public float Range = 1f;
        public CharacterInfo CurrentTarget;
        public SkeletonAnimation SkeletonAnim;
        public bool Grabbed = false;

        public bool beingHovered;

        public MyTeam movementController;
        public EnemyTeam movementController2;
        public bool Killed;

        private MyTeam.CharacterState state;

        private bool fighting = false;
        private CharacterInfo[] allCharacters;

        private bool imGood;

        private void Start()
        {
            if (this.standingOnTile.grid2DLocation.x >= 4)
            {
                movementController2 = FindObjectOfType<EnemyTeam>();
                state = movementController2.GetCharacterState(axieId);
            }
            else
            {
                imGood = true;
                movementController = FindObjectOfType<MyTeam>();
                state = movementController.GetCharacterState(axieId);
            }

            SkeletonAnim = transform.GetChild(0).GetComponent<SkeletonAnimation>();
            SetAllCharacters();
        }

        public void SetAllCharacters()
        {
            allCharacters = FindObjectsOfType<CharacterInfo>();
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
            if (Killed)
                this.gameObject.SetActive(false);

            if (state == null)
                return;

            if (CurrentTarget == null)
            {
                fighting = false;
                SkeletonAnim.AnimationName = "action/idle/normal";
                SkeletonAnim.loop = true;
                return;
            }

     
            
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
                GetManhattanDistancePlayer(this.transform.position, CurrentTarget.transform.position) <= Range)
            {
                if (CurrentTarget.standingOnTile.grid2DLocation.y != standingOnTile.grid2DLocation.y)
                {
                    if (imGood)
                    {
                        transform.localScale = new Vector3(-0.2f, transform.localScale.y, transform.localScale.z);
                    }
                    else
                    {
                        transform.localScale = new Vector3(0.2f, transform.localScale.y, transform.localScale.z);
                    }
                }
                else
                {
                    transform.localScale = new Vector3(CurrentTarget.transform.position.x > this.transform.position.x ? -0.2f : 0.2f, transform.localScale.y, transform.localScale.z);
                }


                fighting = true;
                SkeletonAnim.AnimationName = "attack/melee/tail-roll";
                SkeletonAnim.loop = true;
                return;
            }
            else if (state.isMoving)
            {
                fighting = false;
                SkeletonAnim.AnimationName = "action/run";
                SkeletonAnim.loop = true;
                return;
            }
            else
            {
                fighting = false;
                SkeletonAnim.AnimationName = "action/idle/normal";
                SkeletonAnim.loop = true;
            }
        }

        private int GetManhattanDistancePlayer(Vector3 tile1, Vector3 tile2)
        {
            return Mathf.RoundToInt(Mathf.Abs(tile1.x - tile2.x) +
                                    Mathf.Abs(tile1.z - tile2.z));
        }

        private int GetManhattanDistance(OverlayTile tile1, OverlayTile tile2)
        {
            return Mathf.Abs(tile1.gridLocation.x - tile2.gridLocation.x) +
                   Mathf.Abs(tile1.gridLocation.z - tile2.gridLocation.z);
        }
    }
}