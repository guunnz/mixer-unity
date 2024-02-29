using System;
using System.Collections;
using System.Collections.Generic;
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
        public bool CastingSpell;

        public float Mana = 0;
        private float MaxManaAux = 50;
        internal float MinManaAux = 0;

        public SkillName skillName;
        public AxieClass axieClass;
        public BodyPart bodyPart;

        public float HP = 100;

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

            if (this.axieClass == AxieClass.Bird)
                Range = 4;

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

        public void CastSpell()
        {
            CastingSpell = true;
            SkeletonAnim.loop = false;
            StartCoroutine(ICastSpell());
        }

        IEnumerator ICastSpell()
        {
            float timeToWait = SkillLauncher.Instance.ThrowSkill(skillName, axieClass, bodyPart,
                CurrentTarget.transform, this.transform, SkeletonAnim, CurrentTarget);
            yield return new WaitForSeconds(timeToWait + timeToWait / 2);
            CastingSpell = false;
            Mana = MinManaAux;
            SkeletonAnim.loop = true;
        }

        private void Update()
        {
            if (Killed)
                this.gameObject.SetActive(false);

            if (CastingSpell)
                return;

            if (state == null)
                return;

            if (CurrentTarget == null)
            {
                fighting = false;
                SkeletonAnim.AnimationName = "action/idle/normal";
                SkeletonAnim.loop = true;
                return;
            }

            Mana += Time.deltaTime;

            if (Mana >= MaxManaAux)
            {
                CastSpell();
                Mana = 0;
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
                    transform.localScale =
                        new Vector3(CurrentTarget.transform.position.x > this.transform.position.x ? -0.2f : 0.2f,
                            transform.localScale.y, transform.localScale.z);
                }


                fighting = true;
                if (axieClass == AxieClass.Bird)
                {
                    SkeletonAnim.AnimationName = "attack/ranged/cast-multi";
                }
                else if (axieClass == AxieClass.Aquatic)
                {
                    SkeletonAnim.AnimationName = "attack/melee/horn-gore";
                }
                else if (axieClass == AxieClass.Beast)
                {
                    SkeletonAnim.AnimationName = "attack/melee/tail-roll";
                }
                else if (axieClass == AxieClass.Plant)
                {
                    SkeletonAnim.AnimationName = "attack/melee/mouth-bite";
                }
                else
                {
                    SkeletonAnim.AnimationName = "attack/melee/normal-attack";
                }

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