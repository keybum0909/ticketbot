from flask import Flask, request, jsonify
from PIL import Image
import torch
from torchvision import transforms
import io

# 簡化的 CRNN 模型（請換成你實際的模型定義）
class CRNN(torch.nn.Module):
    def __init__(self):
        super(CRNN, self).__init__()
        self.conv = torch.nn.Sequential(
            torch.nn.Conv2d(1, 64, 3, 1, 1),
            torch.nn.ReLU(),
            torch.nn.MaxPool2d(2, 2)
        )
        self.rnn = torch.nn.LSTM(64 * 16, 32, bidirectional=True)
        self.fc = torch.nn.Linear(64, 37)  # 36 chars + 1 blank

    def forward(self, x):
        x = self.conv(x)
        b, c, h, w = x.size()
        x = x.permute(3, 0, 2, 1).view(w, b, -1)
        x, _ = self.rnn(x)
        x = self.fc(x)
        return x

def decode(preds, charset):
    preds = preds.argmax(2).permute(1, 0).squeeze(0)
    prev_char = -1
    result = ''
    for char in preds:
        if char.item() != prev_char and char.item() != 0:
            result += charset[char.item() - 1]
        prev_char = char.item()
    return result

app = Flask(__name__)
model = CRNN()
model.load_state_dict(torch.load("crnn.pth", map_location='cpu'))
model.eval()
charset = "0123456789abcdefghijklmnopqrstuvwxyz"  # +1 blank for CTC

transform = transforms.Compose([
    transforms.Grayscale(),
    transforms.Resize((32, 100)),
    transforms.ToTensor(),
    transforms.Normalize((0.5,), (0.5,))
])

@app.route("/ocr", methods=["POST"])
def ocr():
    file = request.files["image"]
    image = Image.open(file.stream).convert('L')
    image = transform(image).unsqueeze(0)

    with torch.no_grad():
        output = model(image)
        text = decode(output, charset)

    return jsonify({"text": text})

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)
