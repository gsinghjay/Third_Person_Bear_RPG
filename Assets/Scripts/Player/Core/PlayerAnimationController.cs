using UnityEngine;
using Animancer;

namespace Player.Core
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private AnimancerComponent _animancer;
        
        [Header("Movement Animations")]
        [SerializeField] private ClipTransition _idleAnimation;
        [SerializeField] private ClipTransition _walkForwardAnimation;
        [SerializeField] private ClipTransition _walkBackwardAnimation;
        [SerializeField] private ClipTransition _walkLeftAnimation;
        [SerializeField] private ClipTransition _walkRightAnimation;
        [SerializeField] private ClipTransition _sprintAnimation;
        
        [Header("Combat Animations")]
        [SerializeField] private ClipTransition _attackAnimation;
        [SerializeField] private ClipTransition _defendAnimation;
        
        [Header("Other Animations")]
        [SerializeField] private ClipTransition _jumpAnimation;
        
        [Header("Blend Settings")]
        [SerializeField] private float _transitionDuration = 0.25f;
        
        private void Awake()
        {
            if (_animancer == null)
            {
                _animancer = GetComponent<AnimancerComponent>();
                if (_animancer == null)
                {
                    Debug.LogError("PlayerAnimationController: No AnimancerComponent found!");
                    return;
                }
            }
            
            // Disable root motion on the Animator
            if (_animancer.Animator != null)
            {
                _animancer.Animator.applyRootMotion = false;
            }
            
            Debug.Log($"PlayerAnimationController: Initialized with AnimancerComponent on {gameObject.name}");
            InitializeAnimations();
        }
        
        private void InitializeAnimations()
        {
            if (_attackAnimation != null)
            {
                _attackAnimation.Events.OnEnd = PlayIdle;
                Debug.Log("PlayerAnimationController: Attack animation initialized");
            }

            if (_jumpAnimation != null)
            {
                _jumpAnimation.Events.OnEnd = PlayIdle;
                Debug.Log("PlayerAnimationController: Jump animation initialized");
            }
        }
        
        public void PlayIdle()
        {
            _animancer.Play(_idleAnimation, _transitionDuration);
        }
        
        public void PlayWalk()
        {
            _animancer.Play(_walkForwardAnimation, _transitionDuration);
        }
        
        public void PlaySprint()
        {
            _animancer.Play(_sprintAnimation, _transitionDuration);
        }
        
        public void PlayAttack()
        {
            var state = _animancer.Play(_attackAnimation);
            Debug.Log("PlayerAnimationController: Playing attack animation");
        }
        
        public void PlayDefend()
        {
            _animancer.Play(_defendAnimation, _transitionDuration);
        }
        
        public void PlayJump()
        {
            var state = _animancer.Play(_jumpAnimation);
            Debug.Log("PlayerAnimationController: Playing jump animation");
        }
        
        public void UpdateMovementAnimation(float speedValue, Vector2 movementDirection)
        {
            if (_animancer == null) return;

            if (speedValue <= 0)
            {
                PlayIdle();
                return;
            }

            // Determine primary movement direction
            float absX = Mathf.Abs(movementDirection.x);
            float absY = Mathf.Abs(movementDirection.y);

            var state = speedValue > 0.5f ? _sprintAnimation : 
                (absY > absX) ? 
                    (movementDirection.y > 0 ? _walkForwardAnimation : _walkBackwardAnimation) :
                    (movementDirection.x > 0 ? _walkRightAnimation : _walkLeftAnimation);

            if (state != null)
            {
                var animState = _animancer.Play(state, _transitionDuration);
                if (animState != null)
                {
                    // Adjust animation speed based on movement speed
                    animState.Speed = speedValue;
                }
            }
        }
    }
} 