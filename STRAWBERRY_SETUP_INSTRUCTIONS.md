# Two-Stage Plant Growth Setup Instructions

## Overview

The game now uses a **two-stage plant growth system**:
- **Stage 1 (0-3 seconds)**: `plant(1)` model is displayed
- **Stage 2 (3-10 seconds)**: Automatically transitions to `willow-tree` model
- **Total growth time**: 10 seconds
- **Starting seeds**: 1000 seeds

## Step 1: Import Your Plant Models into Unity

1. Open Unity and your 3D Farm Game project
2. In the Unity Editor, create a new folder structure:
   - Right-click in the **Assets** folder
   - Create New Folder → Name it **"Resources"**
   - Inside Resources, create another folder → Name it **"Crops"**

3. Import your plant models:
   - Drag and drop your plant model files into the **Assets** folder
   - Unity will import the models and their textures
   - You should see the models appear in your Project window

## Step 2: Create the Two Prefabs

You need to create **TWO** prefabs for the two-stage growth system:

### Prefab 1: plant(1) (Stage 1 - First 3 seconds)

1. Drag your small plant model from the Assets folder into the Scene view (temporarily)
2. In the Hierarchy, select the model
3. Adjust the model if needed:
   - Scale: Make sure it's an appropriate size
   - Rotation: Ensure it's facing the right direction
   - Position: Set to (0, 0, 0)
4. Create the prefab:
   - Drag the model from the Hierarchy into **Assets/Resources/Crops/**
   - Name it exactly: **"plant(1)"** (without quotes, with parentheses)
   - Delete the model from the Hierarchy

### Prefab 2: willow-tree (Stage 2 - Last 7 seconds)

1. Drag your willow tree model from the Assets folder into the Scene view (temporarily)
2. In the Hierarchy, select the model
3. Adjust the model if needed:
   - Scale: Should be larger than plant(1) to show growth
   - Rotation: Same as plant(1)
   - Position: Set to (0, 0, 0)
4. Create the prefab:
   - Drag the model from the Hierarchy into **Assets/Resources/Crops/**
   - Name it exactly: **"willow-tree"** (without quotes, with hyphen, all lowercase)
   - Delete the model from the Hierarchy

## Step 3: Verify Your Setup

Make sure you have these two files in the correct location:
- **Assets/Resources/Crops/plant(1)** (or plant(1).prefab)
- **Assets/Resources/Crops/willow-tree** (or willow-tree.prefab)

**Important**: The names must be exactly as shown above (case-sensitive, including parentheses and hyphen)!

## Step 4: Test the Game

1. Save your scene (Ctrl+S or Cmd+S)
2. Press the **Play** button in Unity
3. In the game:
   - Use **WASD** to move
   - Use **Mouse** to look around
   - Look at a planting spot (brown square on the ground)
   - Press **E** to plant a seed (you start with 1000 seeds!)
   - Watch the plant grow:
     - **0-3 seconds**: You'll see the `plant(1)` model
     - **3 seconds**: Automatically transitions to the `willow-tree` model
     - **10 seconds**: Fully grown and ready to harvest!
   - Press **E** again when fully grown to harvest

## Troubleshooting

### Only terrain visible when playing:
- **FIXED**: The Main Camera in the scene is now automatically disabled
- The PlayerCamera is created and used instead
- If you still have issues, check the Console for errors

### Models don't appear when planting:
- Check that both prefabs are named exactly **"plant(1)"** and **"willow-tree"** (case-sensitive, with parentheses/hyphen)
- Check that they're in the correct path: **Assets/Resources/Crops/**
- Check the Console window for any error messages
- If no models are found, the game will use a simple cylinder as a fallback

### Plant doesn't transition from plant(1) to willow-tree:
- Make sure both prefabs exist in Assets/Resources/Crops/
- Check the Console for any errors at the 3-second mark
- The transition happens automatically at exactly 3 seconds

### Models are too big/small:
- Select each prefab in Assets/Resources/Crops/
- Adjust the scale in the Transform component
- Make sure both models are similar in scale for a smooth transition
- Recommended scale: Try (0.5, 0.5, 0.5) or (1, 1, 1) depending on your models

### Models are rotated wrong:
- Select each prefab
- Adjust the rotation in the Transform component
- Make sure both models have the same rotation for consistency
- Usually (0, 0, 0) or (0, 180, 0) works well

### Can't see anything in the scene:
- Make sure you're looking at the planting spots (they're in front of the player)
- The player starts at position (0, 1.2, -6) looking forward
- The planting grid is centered around (0, 0, 2)
- Try moving forward with **W** key

## Current Game Controls

- **WASD** - Move
- **Mouse** - Look around
- **E** - Interact with planting spots (plant/harvest)
- **I** - Toggle inventory
- **F5** - Save game
- **F9** - Load game

## How It Works

The two-stage growth system:
1. You start with **1000 seeds** in your inventory
2. When you press **E** on an empty planting spot, the system plants a seed
3. **Stage 1 (0-3s)**: The `plant(1)` prefab is loaded and displayed
4. **Stage 2 (3-10s)**: At exactly 3 seconds, the system automatically:
   - Destroys the `plant(1)` model
   - Loads and displays the `willow-tree` model
5. **Harvest (10s)**: After 10 seconds total, the willow tree is fully grown
6. Press **E** again to harvest and get produce in your inventory

## Fallback System

If the prefabs are not found, the game will use a simple growing cylinder as a fallback:
- Yellow cylinder that grows taller and turns green
- This lets you test the game mechanics even without 3D models

Enjoy your farm! �

