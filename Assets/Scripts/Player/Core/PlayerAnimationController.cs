using UnityEngine;
using Animancer;
using System.Collections;

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
        
        [Header("Jump Animation")]
        [SerializeField] private ClipTransition _jumpAnimation;
        
        [Header("Combat Animations")]
        [SerializeField] private ClipTransition _attackAnimation;
        
        [Header("Blend Settings")]
        [SerializeField] private float _transitionDuration = 0.25f;
        
        [Header("Combat Settings")]
        [SerializeField] private float _attackAnimationSpeed = 0.5f;
        
        private bool _isJumping;

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
            
            InitializeAnimations();
        }

        private void InitializeAnimations()
        {
            if (_attackAnimation != null)
            {
                _attackAnimation.Events.OnEnd = () =>
                {
                    PlayIdle();
                    Debug.Log("Attack animation completed");
                };
            }
            else
            {
                Debug.LogError("Attack animation not assigned!");
            }

            if (_jumpAnimation != null)
            {
                _jumpAnimation.Events.OnEnd = () =>
                {
                    _isJumping = false;
                    PlayIdle();
                };
            }
        }

        public void PlayAttack()
        {
            if (_attackAnimation == null)
            {
                Debug.LogError("PlayerAnimationController: Attack animation not assigned!");
                return;
            }

            var state = _animancer.Play(_attackAnimation, _transitionDuration);
            if (state != null)
            {
                state.Time = 0;
                // Don't set speed for Animancer Lite
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

        public void StartJump()
        {
            if (!_isJumping)
            {
                _isJumping = true;
                _animancer.Play(_jumpAnimation, _transitionDuration);
            }
        }

        public void UpdateMovementAnimation(float speedValue, Vector2 movementDirection)
        {
            if (_animancer == null) return;

            // Don't interrupt attack animation
            if (_attackAnimation != null && 
                _animancer.States.Current != null && 
                _animancer.States.Current.Clip == _attackAnimation.Clip)
            {
                return;
            }

            // Allow movement updates during jump
            if (_isJumping)
            {
                if (speedValue > 0.1f)
                {
                    _animancer.Play(_walkForwardAnimation, _transitionDuration);
                }
                return;
            }

            // Instead of scaling animation speed, use different animations
            if (speedValue <= 0.1f)
            {
                _animancer.Play(_idleAnimation, _transitionDuration);
                return;
            }

            float absX = Mathf.Abs(movementDirection.x);
            float absY = Mathf.Abs(movementDirection.y);

            ClipTransition animationToPlay;

            // Use distinct animations instead of speed scaling
            if (speedValue > 0.8f)
            {
                animationToPlay = _sprintAnimation;
            }
            else if (absX < 0.1f && absY < 0.1f)
            {
                animationToPlay = _idleAnimation;
            }
            else if (absY > absX)
            {
                animationToPlay = movementDirection.y > 0 ? _walkForwardAnimation : _walkBackwardAnimation;
            }
            else
            {
                animationToPlay = movementDirection.x > 0 ? _walkRightAnimation : _walkLeftAnimation;
            }

            if (animationToPlay != null)
            {
                _animancer.Play(animationToPlay, _transitionDuration);
            }
        }

        public bool IsJumping()
        {
            return _isJumping;
        }
    }
} 