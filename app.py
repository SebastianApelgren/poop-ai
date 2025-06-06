# app.py

import io
import os

import torch
import torch.nn as nn
import torch.nn.functional as F
from fastapi import FastAPI, File, UploadFile
from PIL import Image
from torchvision import transforms, models

# ─── CONFIGURATION ─────────────────────────────────────────────────────────────
DATA_DIR   = "./data"                 # not used here, but kept for reference
MODEL_PATH = "./stool_model.pth"      # Path to your saved .pth file
IMG_SIZE   = 224
NUM_CLASSES = 7

DEVICE = "cuda" if torch.cuda.is_available() else "cpu"

# ─── MODEL DEFINITION ───────────────────────────────────────────────────────────
def load_model():
    model = models.efficientnet_b0(pretrained=False)
    in_features = model.classifier[1].in_features
    model.classifier = nn.Sequential(
        nn.Linear(in_features, 512),
        nn.ReLU(inplace=True),
        nn.Dropout(0.4),
        nn.Linear(512, NUM_CLASSES)
    )
    state_dict = torch.load(MODEL_PATH, map_location=DEVICE)
    model.load_state_dict(state_dict)
    model.to(DEVICE)
    model.eval()
    return model

model = load_model()

# ─── IMAGE TRANSFORMS ────────────────────────────────────────────────────────────
transform = transforms.Compose([
    transforms.Resize((IMG_SIZE, IMG_SIZE)),
    transforms.ToTensor(),
    transforms.Normalize(mean=[0.485, 0.456, 0.406],
                         std=[0.229, 0.224, 0.225]),
])

# ─── FASTAPI SETUP ──────────────────────────────────────────────────────────────
app = FastAPI()

# Precompute class labels sorted alphabetically
class_labels = sorted(os.listdir(DATA_DIR))  # e.g., ["type-1", "type-2", ..., "type-7"]

@app.post("/predict")
async def predict(image_file: UploadFile = File(...)):
    # 1. Read the uploaded image file
    contents = await image_file.read()
    image = Image.open(io.BytesIO(contents)).convert("RGB")
    
    # 2. Preprocess
    img_t = transform(image).unsqueeze(0).to(DEVICE)
    
    # 3. Model inference
    with torch.no_grad():
        outputs = model(img_t)
        probs = F.softmax(outputs, dim=1)
        top_prob, top_cls = torch.max(probs, 1)
    
    # 4. Map to label
    predicted_label = class_labels[top_cls.item()]
    confidence = float(top_prob.item())
    
    # 5. Return JSON response
    return {
        "predicted_type": predicted_label,
        "confidence": confidence
    }