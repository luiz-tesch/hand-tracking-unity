import cv2
import mediapipe as mp
from mediapipe.tasks import python as mp_python
from mediapipe.tasks.python import vision
import socket
import json
import os
import time
import urllib.request

UDP_IP = "127.0.0.1"
UDP_PORT = 5052

MODEL_PATH = "hand_landmarker.task"
MODEL_URL = "https://storage.googleapis.com/mediapipe-models/hand_landmarker/hand_landmarker/float16/1/hand_landmarker.task"

HAND_CONNECTIONS = [
    (0,1),(1,2),(2,3),(3,4),
    (0,5),(5,6),(6,7),(7,8),
    (5,9),(9,10),(10,11),(11,12),
    (9,13),(13,14),(14,15),(15,16),
    (13,17),(17,18),(18,19),(19,20),
    (0,17),
]

if not os.path.exists(MODEL_PATH):
    print("Baixando modelo hand_landmarker.task...")
    urllib.request.urlretrieve(MODEL_URL, MODEL_PATH)
    print("Modelo baixado.")

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)


def get_gesture(landmarks):
    thumb_tip = landmarks[4]
    index_tip = landmarks[8]
    dx = thumb_tip.x - index_tip.x
    dy = thumb_tip.y - index_tip.y
    dist = (dx * dx + dy * dy) ** 0.5
    return "pinch" if dist < 0.07 else "open"


def draw_landmarks(frame, landmarks, gesture):
    h, w = frame.shape[:2]
    pts = [(int(lm.x * w), int(lm.y * h)) for lm in landmarks]
    for a, b in HAND_CONNECTIONS:
        cv2.line(frame, pts[a], pts[b], (0, 255, 0), 2)
    for pt in pts:
        cv2.circle(frame, pt, 5, (0, 0, 255), -1)

    color = (0, 255, 255) if gesture == "pinch" else (255, 255, 255)
    cv2.putText(frame, gesture.upper(), (10, 40),
                cv2.FONT_HERSHEY_SIMPLEX, 1.2, color, 2)


base_options = mp_python.BaseOptions(model_asset_path=MODEL_PATH)
options = vision.HandLandmarkerOptions(
    base_options=base_options,
    running_mode=vision.RunningMode.VIDEO,
    num_hands=1,
)

cap = cv2.VideoCapture(0)
start_time = time.time()

with vision.HandLandmarker.create_from_options(options) as landmarker:
    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            break

        rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb)
        timestamp_ms = int((time.time() - start_time) * 1000)

        result = landmarker.detect_for_video(mp_image, timestamp_ms)

        if result.hand_landmarks:
            hand = result.hand_landmarks[0]
            gesture = get_gesture(hand)

            payload = {
                "landmarks": [
                    {"x": lm.x, "y": lm.y, "z": lm.z}
                    for lm in hand
                ],
                "gesture": gesture
            }
            sock.sendto(json.dumps(payload).encode("utf-8"), (UDP_IP, UDP_PORT))
            draw_landmarks(frame, hand, gesture)

        cv2.imshow("Hand Tracker", frame)
        if cv2.waitKey(1) & 0xFF == ord("q"):
            break

cap.release()
cv2.destroyAllWindows()
sock.close()
