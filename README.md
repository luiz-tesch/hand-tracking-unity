# Hand Tracking Unity

Real-time hand tracking with **MediaPipe** (Python) integrated into **Unity 6.1** via UDP socket.

## How it works

```
Webcam → Python (MediaPipe) → UDP 5052 → Unity → 21 animated spheres
```

The Python script captures the webcam, detects the 21 hand landmarks using MediaPipe, and streams the coordinates as JSON over UDP. Unity receives the data and moves 21 spheres in the scene in real time.

## Tech stack

- **Unity 6.1** (6000.1.13f1)
- **Python 3** + MediaPipe + OpenCV
- **UDP socket** on port 5052

## Requirements

- Python 3.10+
- Webcam
- Unity 6.1

## Python setup

```bash
python -m venv venv
venv\Scripts\activate
pip install mediapipe opencv-python
```

The `hand_landmarker.task` model (~29 MB) is downloaded automatically on the first run.

## Running

1. Open the project in Unity and press **Play**
2. In a separate terminal, from the project root:

```bash
venv\Scripts\activate
python hand_tracker.py
```

3. Point your hand at the webcam — the 21 spheres move in the Unity scene

Press `q` to quit the Python script.

## Structure

```
hand-tracking-unity/
├── Assets/
│   ├── HandReceiver.cs   # C# script that receives UDP data in Unity
│   └── Scenes/
├── hand_tracker.py       # Python hand tracking script
└── venv/                 # Python virtual environment (not versioned)
```
