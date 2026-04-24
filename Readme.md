A high-fidelity 2D Slot Machine simulation built with Unity. This project focuses on realistic mechanical reel movement, modular code architecture, and a smooth user experience.

Game Overview

This project simulates a classic three-reel mechanical slot machine. Players interact with a physical handle to trigger a spin, with the goal of matching three symbols across the centre line to win prizes.
Core Features:

	• Physics-Based Spin Logic: Reels utilize custom deceleration curves and "snap-to-grid" settlement to mimic real-world mechanical friction.
	• Dynamic Payout System: Automatic result evaluation with a structured payout dictionary.
	• Responsive UI: Real-time feedback via TextMeshPro, guiding the player from "Pull the handle" to "You Win!".
	• Audio Feedback: Integrated Sound Manager for win states.

Instructions to Run WebGL Build

To play the game in your browser visit the following link:- https://lordeoflights.itch.io/slot-game

Thought Process & Approach
1. Architecture: Decoupling and Events
	I chose an Event-Driven Architecture for the spin trigger. Instead of the GameControl script manually telling every reel to spin, it fires a static event Action HandlePulled.
    	Why? This allows the reels to be "self-aware." I can add or remove reels (e.g., making it a 5-reel slot) without ever changing the code in the handle or the main controller.

2. The "Feel" of the Spin (The Row Logic)
	The biggest challenge in slot games is making the reels feel "heavy."
   	The Solution: I implemented a three-phase Coroutine system in Row.cs:
        Full Speed: Constant looping.
        Deceleration: Interpolating between fullSpeed and crawlSpeed using an AnimationCurve.
        Settle: A final Lerp to ensure the Y-position is perfectly centered.
3. Data Management
	I used a Dictionary<string, int> for payouts and a Dictionary<float, string> for icon mapping.
    	Why? Dictionaries provide O(1) look-up time. By mapping the final Y-position of the reel to a String name, I avoided complex collision detection or trigger zones, making the win-check logic extremely lightweight and bug-free.

4. Error Handling
	I implemented a resultsChecked flag to ensure the game doesn't try to award a prize while the reels are still moving, preventing "double-win" glitches.
