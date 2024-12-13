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
        
        [Header("Jump Animation")]
        [SerializeField] private ClipTransition _jumpAnimation;
        
        [Header("Combat Animations")]
        [SerializeField] private ClipTransition _attackAnimation;
        [SerializeField] private ClipTransition _defendAnimation;
        
        [Header("Blend Settings")]
        [SerializeField] private float _transitionDuration = 0.25f;
        
        private CharacterController characterController;
        private bool _isJumping;

        private void Awake()
        {
            characterController = GetComponentInParent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError("PlayerAnimationController: No CharacterController found in parent!");
            }

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
                _attackAnimation.Events.OnEnd = PlayIdle;
            }

            if (_jumpAnimation != null)
            {
                _jumpAnimation.Events.OnEnd = () =>
                {
                    _isJumping = false;
                    PlayIdle();
                    Debug.Log("Jump animation completed");
                };
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
            _animancer.Play(_attackAnimation);
        }

        public void PlayDefend()
        {
            _animancer.Play(_defendAnimation, _transitionDuration);
        }

        public void StartJump()
        {
            if (!_isJumping)
            {
                _isJumping = true;
                _animancer.Play(_jumpAnimation, _transitionDuration);
                Debug.Log("Playing jump animation");
            }
        }

        public void UpdateMovementAnimation(float speedValue, Vector2 movementDirection)
        {
            if (_animancer == null) return;

            if (_isJumping) return; // Don't update movement animations while jumping

            if (speedValue <= 0)
            {
                PlayIdle();
                return;
            }

            float absX = Mathf.Abs(movementDirection.x);
            float absY = Mathf.Abs(movementDirection.y);

            ClipTransition animationToPlay;

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
                var animState = _animancer.Play(animationToPlay, _transitionDuration);
                if (animState != null)
                {
                    float speedScale = Mathf.Max(0.5f, speedValue);
                    animState.Speed = speedScale;
                }
            }
        }

        public bool IsJumping()
        {
            return _isJumping;
        }
    }
} 