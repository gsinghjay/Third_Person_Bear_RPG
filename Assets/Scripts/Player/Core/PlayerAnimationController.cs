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
        
        [Header("Jump Animations")]
        [SerializeField] private ClipTransition _jumpStartAnimation;
        [SerializeField] private ClipTransition _jumpLoopAnimation;
        [SerializeField] private ClipTransition _jumpLandAnimation;
        
        [Header("Combat Animations")]
        [SerializeField] private ClipTransition _attackAnimation;
        [SerializeField] private ClipTransition _defendAnimation;
        
        [Header("Blend Settings")]
        [SerializeField] private float _transitionDuration = 0.25f;
        [SerializeField] private float _jumpTransitionDuration = 0.1f;
        
        private AnimancerState _currentJumpState;
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
            
            if (_animancer.Animator != null)
            {
                _animancer.Animator.applyRootMotion = false;
            }
            
            Debug.Log($"PlayerAnimationController: Initialized with AnimancerComponent on {gameObject.name}");
            InitializeAnimations();
            InitializeJumpAnimations();
        }
        
        private void InitializeAnimations()
        {
            if (_attackAnimation != null)
            {
                _attackAnimation.Events.OnEnd = PlayIdle;
                Debug.Log("PlayerAnimationController: Attack animation initialized");
            }
        }
        
        private void InitializeJumpAnimations()
        {
            if (_jumpStartAnimation != null)
            {
                _jumpStartAnimation.Events.OnEnd = () =>
                {
                    if (_isJumping)
                    {
                        PlayJumpLoop();
                    }
                };
                Debug.Log("PlayerAnimationController: Jump start animation initialized");
            }

            if (_jumpLandAnimation != null)
            {
                _jumpLandAnimation.Events.OnEnd = PlayIdle;
                Debug.Log("PlayerAnimationController: Jump land animation initialized");
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

            ClipTransition animationToPlay;

            if (speedValue > 0.8f) // Sprint threshold
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
                    // Scale animation speed based on movement speed, but keep a minimum to prevent very slow animations
                    float speedScale = Mathf.Max(0.5f, speedValue);
                    animState.Speed = speedScale;
                    
                    Debug.Log($"Playing animation: {animationToPlay.Clip.name} with speed {speedScale}");
                }
            }
        }
        
        public void StartJump()
        {
            _isJumping = true;
            if (_jumpStartAnimation != null)
            {
                _currentJumpState = _animancer.Play(_jumpStartAnimation, _jumpTransitionDuration);
                Debug.Log("PlayerAnimationController: Starting jump animation");
            }
            else
            {
                PlayJumpLoop();
            }
        }

        public void PlayJumpLoop()
        {
            if (_jumpLoopAnimation != null)
            {
                _currentJumpState = _animancer.Play(_jumpLoopAnimation, _jumpTransitionDuration);
                Debug.Log("PlayerAnimationController: Playing jump loop animation");
            }
        }

        public void EndJump()
        {
            _isJumping = false;
            if (_jumpLandAnimation != null)
            {
                _currentJumpState = _animancer.Play(_jumpLandAnimation, _jumpTransitionDuration);
                Debug.Log("PlayerAnimationController: Playing landing animation");
            }
            else
            {
                PlayIdle();
            }
        }

        public bool IsPlayingJumpAnimation()
        {
            return _isJumping || 
                   (_currentJumpState != null && _currentJumpState.IsPlaying && 
                    (_currentJumpState.Clip == _jumpStartAnimation?.Clip || 
                     _currentJumpState.Clip == _jumpLoopAnimation?.Clip || 
                     _currentJumpState.Clip == _jumpLandAnimation?.Clip));
        }

        public bool IsInJumpLoop()
        {
            return _currentJumpState != null && 
                   _currentJumpState.IsPlaying && 
                   _currentJumpState.Clip == _jumpLoopAnimation?.Clip;
        }
    }
} 