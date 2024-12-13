# Special Attack Animation Implementation Guide
## Overview
This document explains how we implemented special attack animations using Animancer Lite 8 in our project, including challenges faced and solutions.

## Key Components

### 1. Animation System Structure
```csharp
// PlayerAnimationController.cs
[SerializeField] private AnimancerComponent _animancer;
[SerializeField] private ClipTransition _specialAttackAnimation;
```
- Uses AnimancerComponent for animation management
- ClipTransition assets hold animation data
- Transitions handled through simple fade durations

### 2. State Machine Integration
```csharp
// States handle combat through CombatState
public override void HandleCombat()
{
    if (animationController.IsJumping()) return;

    if (playerInput.IsSpecialAttacking)
    {
        combatHandler.PerformSpecialAttack();
        attackTimer = playerController.AttackDuration;
    }
}
```

## Implementation Steps

1. **Input Setup**
   - Add IsSpecialAttacking to IPlayerInput interface
   - Implement in PlayerInput class using right mouse button

2. **Animation Controller**
   - Create ClipTransition asset in Unity
   - Assign animation clip to ClipTransition
   - Add field to PlayerAnimationController
   - Implement PlaySpecialAttack method

3. **State Machine**
   - Update IdleState, SprintState, and CombatState
   - Add special attack handling
   - Prevent attacks during jumps

## Challenges Faced & Solutions

### 1. Animancer Lite Limitations
**Problem**: Tried using advanced features not in Lite version
**Solution**: 
```csharp
// Simple direct play approach
var state = _animancer.Play(_specialAttackAnimation, _transitionDuration);
state.Time = 0;
```

### 2. State Management
**Problem**: Animation states weren't transitioning properly
**Solution**: 
```csharp
// Stop current animation before playing new one
_animancer.Stop();
_animancer.Play(_specialAttackAnimation, _transitionDuration);
```

### 3. Event Handling
**Problem**: Complex event system not available in Lite
**Solution**: Use ClipTransition's basic events
```csharp
_specialAttackAnimation.Events.OnEnd = () => PlayIdle();
```

## Unity Setup Requirements

1. **ClipTransition Asset**
   - Create new ClipTransition asset
   - Assign animation clip
   - Set fade duration
   - Disable loop

2. **Inspector Settings**
   ```
   AnimancerComponent:
   - Layer 0: Override mode
   - Clear Animator Controller

   ClipTransition:
   - Fade Duration: 0.25 (or as needed)
   - Speed: 1
   - Loop: False
   ```

## Best Practices

1. **Animation Control**
   - Keep transitions simple
   - Use Stop() before playing new animations
   - Reset Time to 0 for attacks

2. **State Management**
   - Check IsJumping before allowing attacks
   - Maintain proper state transitions
   - Handle animation completion

3. **Debug Support**
   ```csharp
   Debug.Log($"PlayerAnimationController: Special attack animation playing");
   ```

## Adding New Animations Checklist

1. Create animation clip
2. Create ClipTransition asset
3. Configure ClipTransition settings
4. Add SerializeField in PlayerAnimationController
5. Implement Play method
6. Update relevant states
7. Add input handling
8. Test state transitions

## Common Pitfalls

1. Not stopping current animation
2. Using unavailable Animancer Pro features
3. Complex event systems
4. Missing inspector assignments
5. Incorrect animation import settings

## References

- [Animancer Lite Documentation](https://kybernetik.com.au/animancer/docs/introduction/)
- [Unity Animation System](https://docs.unity3d.com/Manual/AnimationSection.html)