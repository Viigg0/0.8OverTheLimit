# 0.8 Over The Limit 🍺🚗

> A serious transformational game about drunk driving.  
> Built in Unity 6000.3.11f1 · Game Research & Development Exam Project

---

## 🎮 Game Concept

A two-stage first-person serious game designed to make players **feel** the consequences of drunk driving rather than just reading about them.

**Stage 1 — The Bar**  
Player sits at a bar in first-person. They can order drinks from the bartender. Each drink raises the `drunkLevel` (0.0 → 1.0). The bartender reacts with increasing concern. Player chooses when to "drive home."

**Stage 2 — The Drive**  
Player drives home in first-person. Visual distortion (vignette, chromatic aberration, lens wobble, depth of field blur) and steering impairment scale with how much they drank. Hazards include deer, trash cans, and oncoming cars.

---

## 🗂️ Project Structure

```
Assets/
├── Animations/          # Character & object animations
├── Audio/
│   ├── Music/           # Bar ambience, driving music
│   └── SFX/             # Crash, pour, clink sounds
├── Materials/           # Shared materials
├── Models/
│   ├── Bar/             # Bar interior, bottles, glasses
│   ├── Characters/      # Bartender, bar patrons, FPS hands
│   ├── Environment/     # Road, trees, props, city assets
│   └── Vehicles/        # Player car exterior + interior
├── Prefabs/
│   ├── Bar/
│   ├── Characters/
│   ├── Environment/
│   ├── UI/
│   └── Vehicles/
├── Scenes/
│   ├── BarScene         # Stage 1
│   ├── DrivingScene     # Stage 2
│   └── EndScene         # Outcome screen
├── Scripts/
│   ├── Bar/             # Drink ordering, bartender dialogue
│   ├── Core/            # Game state, drunk level, utilities
│   ├── Driving/         # Car physics, post-processing, collisions
│   └── UI/              # HUD and menu scripts
├── Settings/            # URP render pipeline asset
├── Shaders/             # Custom shaders if needed
└── Textures/            # Textures organised by category
```

---

## 🧩 Asset Credits

### Bar Scene
- **Bar Environment** — [Bar / Restaurant (CGTrader)](https://www.cgtrader.com/free-3d-models/furniture/other/bar-resturant)

### Characters
- **NPCs** — [City People FREE Samples by Denys Almaral](https://assetstore.unity.com/packages/3d/characters/city-people-free-samples-260446)

### Vehicles
- **Cars** — [Stylized Vehicles Pack FREE by Alex Lenk](https://assetstore.unity.com/packages/3d/vehicles/land/stylized-vehicles-pack-free-150318)

### Environment
- **Road / City** — [Free 3D Low Poly City Asset Pack by Stars and Shells Studio](https://starsandshellsstudio.itch.io/free-3d-low-poly-city-asset-pack)
- **Additional Foliage** — [Low Poly Trees Pack Lite](https://assetstore.unity.com/packages/3d/vegetation/trees/low-poly-trees-pack-lite-free-stylized-nature-environment-assets-295464)

---

## ⚙️ Unity Setup Instructions

### 1. Requirements
- Unity **6000.3.11f1** (required — do not use other versions)
- Universal Render Pipeline (URP)

### 2. Open Project
```bash
git clone https://github.com/YOUR_USERNAME/0.8OverTheLimit.git
```
Open in Unity Hub → Add project from disk → select the cloned folder.

### 3. Import Assets
Import from Unity Asset Store (Window → Package Manager → My Assets):
- [ ] Stylized Vehicles Pack FREE (Alex Lenk) → `Assets/Models/Vehicles/`
- [ ] City People FREE Samples (Denys Almaral) → `Assets/Models/Characters/`
- [ ] Low Poly Trees Pack Lite → `Assets/Models/Environment/`

Download and place manually:
- [ ] Free 3D Low Poly City Asset Pack (itch.io) → `Assets/Models/Environment/`
- [ ] Bar / Restaurant (CGTrader) → `Assets/Models/Bar/`

### 4. Configure URP
Edit → Project Settings → Graphics → Set Scriptable Render Pipeline to the URP asset in `Assets/Settings/`

### 5. Post Processing
Add a **Global Volume** to each scene and assign a profile with:
- Vignette
- Chromatic Aberration
- Lens Distortion
- Depth of Field
- Color Adjustments

---

## 🎯 Evaluation Notes (Type 2 — Exam)

This game is built for a **user study**. Evaluation metrics to consider:
- Pre/post questionnaire on drunk driving awareness
- In-game data: drinks consumed, crash time, crash type
- Player experience questionnaire: engagement, immersion, message clarity
- Consider NASA-TLX for cognitive load during driving stage

---

## 👥 Team

| Name | Role |
|------|------|
| TBD  | Unity Developer |
| TBD  | Level Design |
| TBD  | UI/UX |
| TBD  | Research & Paper |
| TBD  | Evaluation |

---

## 📄 License
Academic project — all third-party assets subject to their respective licenses (see Asset Credits above).
