# Game1

Hybrid-casual block puzzle game for iOS and Android.

## Overview

A 9x9 grid block puzzle with meta progression, ads, and IAP. Target: profitable hybrid-casual game with strong retention mechanics.

## Quick Start

### 1. Create Unity Project

**This step is required** - Unity projects must be created via Unity Hub:

1. Open **Unity Hub**
2. Click **New project**
3. Select **2D (Built-in Render Pipeline)**
4. Set location to this repo's parent directory
5. Name the project `game1`
6. Click **Create project**

Unity will merge with the existing folder structure.

### 2. After Project Creation

1. Open the project in Unity
2. Go to **Edit > Project Settings > Player**
3. Set Company Name and Product Name
4. Configure iOS/Android build settings

### 3. Run Tests

1. Open **Window > General > Test Runner**
2. Run EditMode tests to verify game logic
3. All tests should pass

## Project Structure

```
Assets/
  _Game/
    Scripts/
      Core/        - GameManager, GameConfig, GameLoopController
      Grid/        - GridData, ClearDetector, PlacementValidator, GameOverDetector
      Pieces/      - PieceData, PieceLibrary, PieceGenerator
      Progression/ - ProgressionManager, DailyRewardManager, DailyChallengeManager
      Economy/     - EconomyManager
      Ads/         - IAdService, MockAdService, AdManager
      IAP/         - IIAPService, MockIAPService, IAPManager
      Analytics/   - AnalyticsManager
      Save/        - SaveManager, SaveData
      UI/          - UIManager, MainMenuUI, GameUI, SettingsUI, TutorialManager
    Prefabs/
    ScriptableObjects/
    Scenes/
    UI/
    Art/
    Audio/
  Editor/
    DebugPanel.cs
    Tests/
```

## Architecture

### Core Game Loop

1. Generate 3 pieces
2. Player drags piece onto grid
3. Validate placement (bounds + overlap)
4. Place piece, check clears (rows/cols/3x3 blocks)
5. Apply clears, update score + combo
6. When all 3 pieces used, generate new set
7. Check game over (brute-force validation)

### Key Design Decisions

- **Deterministic game over detection**: Brute-force scan ensures no false positives
- **Precomputed rotations**: No runtime float math for piece rotation
- **Atomic saves**: Write to temp file, then replace main file
- **Interface-based services**: IAdService/IIAPService for easy SDK swapping

## Configuration

All tuning values live in ScriptableObjects:

- **GameConfig**: Grid size, scoring, economy, ad timing
- **ProgressionConfig**: XP curve, level unlocks

Create via **Assets > Create > BlockPuzzle > [Config Type]**

## Debug Tools

In Unity Editor: **BlockPuzzle > Debug Panel**

- Force game over
- Add coins/XP
- Trigger mock ads
- Force save/load
- View tuning values

## Monetization

### Ads (IAdService)

- **Rewarded**: Continue, extra piece, double daily reward
- **Interstitial**: After every 2-3 games (configurable)
- Never mid-play, never before 3rd session

### IAP (IIAPService)

- Remove Ads ($4.99)
- Booster Pack ($2.99)
- Theme Pack ($1.99)

## Wiring Real Ad SDKs

1. Install SDK (AdMob, Unity Ads, ironSource, etc.)
2. Create class implementing `IAdService`
3. In `AdManager.Start()`, replace mock with real service:

```csharp
_adService = new AdMobService(); // Your implementation
_adService.Initialize();
```

## Wiring Real IAP

1. Install Unity IAP package
2. Create class implementing `IIAPService`
3. In `IAPManager.Start()`, replace mock with real service

## Analytics Events

All events ready for Firebase/Unity Analytics:

- session_start, run_start, run_end
- placement, clear
- ad_shown, ad_completed, ad_failed
- iap_attempt, iap_success
- daily_reward_claimed, daily_challenge_*
- level_up, unlock

## KPI Tuning Checklist

### Retention
- [ ] D1 retention > 40%
- [ ] D7 retention > 15%
- [ ] Session length 5-10 min

### Monetization
- [ ] Rewarded ad engagement > 30%
- [ ] IAP conversion > 2%
- [ ] ARPDAU > $0.05

### Tuning Levers
- Piece spawn weights (GameConfig)
- Coin economy (rewards vs costs)
- Ad frequency
- Daily reward escalation

## Soft Launch Checklist

1. Deploy to TestFlight / Internal Testing
2. Enable analytics
3. Monitor:
   - Crash rate < 1%
   - D1/D7 retention
   - Session length
   - Ad engagement
   - IAP revenue
4. Iterate on tuning
5. Scale to larger audience

## Git Workflow

```bash
# Check status
git status

# Stage and commit
git add .
git commit -m "Description of changes"

# Push
git push origin main
```

## Post-Launch TODOs

- [ ] Real ad SDK integration
- [ ] Real IAP integration
- [ ] Firebase Analytics integration
- [ ] Push notifications
- [ ] Leaderboards
- [ ] Achievements
- [ ] More themes
- [ ] More piece types
- [ ] Sound effects
- [ ] Background music
- [ ] Haptic feedback (iOS)
- [ ] App Store optimization
- [ ] Localization

## License

Private - All rights reserved
