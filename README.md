# Hand Tracking Unity

Projeto de portfólio para o mestrado em Computação Gráfica — UFRGS.

Rastreamento de mãos em tempo real com **MediaPipe** (Python) integrado ao **Unity 6.1** via socket UDP.

## Como funciona

```
Webcam → Python (MediaPipe) → UDP 5052 → Unity → 21 esferas animadas
```

O script Python captura a webcam, detecta os 21 landmarks da mão com MediaPipe e envia as coordenadas como JSON via UDP. O Unity recebe esses dados e move 21 esferas na cena em tempo real.

## Tecnologias

- **Unity 6.1** (6000.1.13f1)
- **Python 3** + MediaPipe + OpenCV
- Comunicação via **UDP socket** (porta 5052)

## Requisitos

- Python 3.10+
- Webcam
- Unity 6.1

## Setup Python

```bash
# Na raiz do projeto
python -m venv venv
venv\Scripts\activate
pip install mediapipe opencv-python
```

Na primeira execução, o modelo `hand_landmarker.task` (~29 MB) é baixado automaticamente.

## Como rodar

1. Abra o projeto no Unity e clique em **Play**
2. Em outro terminal, na raiz do projeto:

```bash
venv\Scripts\activate
python hand_tracker.py
```

3. Aponte a mão para a webcam — as 21 esferas se movem na cena Unity

Pressione `q` para encerrar o script Python.

## Estrutura

```
hand-tracking-unity/
├── Assets/
│   ├── HandReceiver.cs   # Script C# que recebe os dados UDP no Unity
│   └── Scenes/
├── hand_tracker.py       # Script Python de rastreamento
└── venv/                 # Ambiente virtual Python (não versionado)
```
