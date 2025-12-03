using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelGame
{
    /// <summary>
    /// Base class for boss encounters with multi-phase support
    /// </summary>
    public abstract class BossBase : EnemyBase
    {
        [Header("Boss Settings")]
        [SerializeField] protected List<BossPhase> phases = new List<BossPhase>();
        [SerializeField] protected bool invincibleDuringPhaseTransition = true;
        [SerializeField] protected float phaseTransitionDuration = 2f;

        [Header("Boss UI")]
        [SerializeField] protected string bossName = "Boss";
        [SerializeField] protected Sprite bossPortrait;

        protected int currentPhaseIndex = 0;
        protected bool isInPhaseTransition = false;

        [System.Serializable]
        protected class BossPhase
        {
            public string phaseName = "Phase 1";
            [Range(0f, 1f)]
            public float healthThreshold = 0.75f; // Enter this phase at this health %
            public float damageMultiplier = 1f;
            public float speedMultiplier = 1f;
            public List<string> abilityNames = new List<string>();
        }

        protected override void Start()
        {
            base.Start();

            // Initialize first phase
            if (phases.Count > 0)
            {
                EnterPhase(0);
            }

            // Announce boss
            GameEvents.PlaySoundEffect("BossAppear", transform.position);
        }

        protected override void Update()
        {
            base.Update();

            // Check for phase transitions
            if (!isInPhaseTransition)
            {
                CheckPhaseTransition();
            }
        }

        protected virtual void CheckPhaseTransition()
        {
            float healthPercent = GetHealthPercentage();

            for (int i = currentPhaseIndex + 1; i < phases.Count; i++)
            {
                if (healthPercent <= phases[i].healthThreshold)
                {
                    StartCoroutine(TransitionToPhase(i));
                    break;
                }
            }
        }

        protected IEnumerator TransitionToPhase(int phaseIndex)
        {
            if (phaseIndex < 0 || phaseIndex >= phases.Count) yield break;

            isInPhaseTransition = true;
            bool wasInvincible = isDead;

            // Make invincible during transition
            if (invincibleDuringPhaseTransition)
            {
                isDead = true; // Temporary invincibility hack
            }

            // Stop all actions
            rb.linearVelocity = Vector2.zero;
            currentState = EnemyState.Stunned;

            // Phase transition effects
            OnPhaseTransitionStart(phaseIndex);

            // Wait for transition
            yield return new WaitForSeconds(phaseTransitionDuration);

            // Enter new phase
            EnterPhase(phaseIndex);

            // Restore state
            isDead = wasInvincible;
            isInPhaseTransition = false;
            currentState = EnemyState.Idle;

            OnPhaseTransitionEnd(phaseIndex);
        }

        protected virtual void EnterPhase(int phaseIndex)
        {
            if (phaseIndex < 0 || phaseIndex >= phases.Count) return;

            currentPhaseIndex = phaseIndex;
            BossPhase phase = phases[phaseIndex];

            // Apply phase modifiers
            damage *= phase.damageMultiplier;
            moveSpeed *= phase.speedMultiplier;

            // Visual feedback
            Debug.Log($"{bossName} entered {phase.phaseName}!");

            OnPhaseEnter(phase);
        }

        protected virtual void OnPhaseTransitionStart(int phaseIndex)
        {
            // Override for custom transition effects
            // Spawn particles, play animation, etc.
        }

        protected virtual void OnPhaseTransitionEnd(int phaseIndex)
        {
            // Override for post-transition effects
        }

        protected virtual void OnPhaseEnter(BossPhase phase)
        {
            // Override for phase-specific behavior
        }

        public override void Die()
        {
            GameEvents.BossDefeated(this);
            base.Die();

            // Boss-specific death effects
            // Spawn big reward, unlock next area, etc.
        }

        protected BossPhase GetCurrentPhase()
        {
            if (currentPhaseIndex >= 0 && currentPhaseIndex < phases.Count)
            {
                return phases[currentPhaseIndex];
            }
            return null;
        }

        /// <summary>
        /// Check if boss can use a specific ability in current phase
        /// </summary>
        protected bool CanUseAbility(string abilityName)
        {
            BossPhase phase = GetCurrentPhase();
            return phase != null && phase.abilityNames.Contains(abilityName);
        }
    }
}
