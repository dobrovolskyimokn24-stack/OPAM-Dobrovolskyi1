import sys
import math
from collections import Counter
import numpy as np


# ==========================================
# Модуль читання PPM з ЛР1
# ==========================================
def read_ppm(path):
    with open(path, "rb") as f:
        data = f.read()

    pos = 0
    fields = []
    while len(fields) < 4:
        while data[pos:pos + 1].isspace():
            pos += 1
        if data[pos:pos + 1] == b"#":
            while data[pos:pos + 1] not in (b"\n", b""):
                pos += 1
            continue
        start = pos
        while not data[pos:pos + 1].isspace():
            pos += 1
        fields.append(data[start:pos])

    if fields[0] != b"P6":
        raise ValueError("Це не PPM формату P6")

    width = int(fields[1])
    height = int(fields[2])
    maxval = int(fields[3])

    if maxval != 255:
        raise ValueError("Підтримується лише maxval = 255")

    pos += 1
    need = width * height * 3
    pixels = np.frombuffer(data, dtype=np.uint8, count=need, offset=pos)
    return pixels.reshape(height, width, 3)


# ==========================================
# Ентропія Шеннона з ЛР3
# ==========================================
def calculate_entropy(data):
    flat = data.flatten()
    if len(flat) == 0:
        return 0.0
    _, counts = np.unique(flat, return_counts=True)
    probabilities = counts / len(flat)
    return -sum(p * math.log2(p) for p in probabilities if p > 0)


# ==========================================
# Генерація тестових зображень
# ==========================================
def generate_test_images():
    # 1. Градієнт
    grad = np.tile(np.arange(256, dtype=np.uint8), (256, 1))

    # 2. Білий шум
    np.random.seed(42)
    noise = np.random.randint(0, 256, (256, 256), dtype=np.uint8)

    # 3. Скріншот
    scr = np.zeros((256, 256), dtype=np.uint8)
    scr[::16, :] = 255
    scr[:, ::16] = 255

    # 4. Фотографія
    try:
        photo = read_ppm("Multimedia_Dev/lab01/test.ppm")
        photo = photo[..., 0]
    except Exception:
        xx, yy = np.meshgrid(np.linspace(0, 1, 256), np.linspace(0, 1, 256))
        photo = (np.sin(xx * 10) * np.cos(yy * 10) * 127 + 128).astype(np.uint8)

    return {
        "Градієнт": grad,
        "Білий шум": noise,
        "Скріншот": scr,
        "Фотографія": photo
    }


# ==========================================
# Завдання 1 & 2. Пряме фільтрування (None, Sub, Up)
# ==========================================
def filter_row(cur, prev, f):
    a = np.concatenate(([0], cur[:-1])).astype(np.uint8)
    b = prev

    if f == 0:
        pred = np.zeros_like(cur)
    elif f == 1:
        pred = a
    elif f == 2:
        pred = b
    else:
        raise ValueError("Невідомий фільтр")

    return ((cur.astype(int) - pred.astype(int)) % 256).astype(np.uint8)


def filter_image(img, f):
    out = np.zeros_like(img)
    prev = np.zeros(img.shape[1], dtype=np.uint8)
    for y in range(img.shape[0]):
        out[y] = filter_row(img[y], prev, f)
        prev = img[y]
    return out


# ==========================================
# Завдання 3. Зворотне перетворення (Unfilter)
# ==========================================
def unfilter_row(enc, prev, f):
    n = len(enc)
    out = np.zeros(n, dtype=np.uint8)
    for x in range(n):
        a = out[x - 1] if x > 0 else 0
        b = prev[x]
        pred = 0 if f == 0 else (a if f == 1 else b)
        out[x] = (int(enc[x]) + int(pred)) & 0xFF
    return out


def unfilter_image(enc_img, f):
    out = np.zeros_like(enc_img)
    prev = np.zeros(enc_img.shape[1], dtype=np.uint8)
    for y in range(enc_img.shape[0]):
        out[y] = unfilter_row(enc_img[y], prev, f)
        prev = out[y]
    return out


def unfilter_adaptive(enc_img, chosen):
    out = np.zeros_like(enc_img)
    prev = np.zeros(enc_img.shape[1], dtype=np.uint8)
    for y in range(enc_img.shape[0]):
        f = chosen[y]
        out[y] = unfilter_row(enc_img[y], prev, f)
        prev = out[y]
    return out


# ==========================================
# Завдання 4. Адаптивний вибір фільтра для кожного рядка
# ==========================================
def sum_abs(row):
    r = row.astype(int)
    signed = np.where(r > 127, r - 256, r)
    return int(np.abs(signed).sum())


def filter_adaptive(img):
    out = np.zeros_like(img)
    chosen = []
    prev = np.zeros(img.shape[1], dtype=np.uint8)

    for y in range(img.shape[0]):
        best_f = 0
        best_sum = float('inf')
        best_row = None

        for f in (0, 1, 2):
            filtered = filter_row(img[y], prev, f)
            s = sum_abs(filtered)
            if s < best_sum:
                best_sum = s
                best_f = f
                best_row = filtered

        out[y] = best_row
        chosen.append(best_f)
        prev = img[y]

    return out, chosen


# ==========================================
# Головна програма
# ==========================================
def main():
    print("=== Завдання 1. Самоперевірка filter_row ===")
    row = np.array([40, 42, 44, 46, 48, 50, 52, 54], dtype=np.uint8)
    zeros = np.zeros(8, dtype=np.uint8)
    print("Sub: ", filter_row(row, zeros, 1))
    print("None:", filter_row(row, zeros, 0))
    print("Up:  ", filter_row(row, row, 2))

    images = generate_test_images()

    print("\n=== Завдання 2 та 5. Ентропія та Адаптивний фільтр ===")
    print(f"{'Тип зображення':<12} | {'До (None)':<10} | {'Sub':<8} | {'Up':<8} | {'Adaptive':<8} | {'Оптимальний':<11} | {'Виграш, %':<8}")
    print("-" * 80)

    for name, img in images.items():
        e_orig = calculate_entropy(img)
        f_none = filter_image(img, 0)
        f_sub = filter_image(img, 1)
        f_up = filter_image(img, 2)

        e_sub = calculate_entropy(f_sub)
        e_up = calculate_entropy(f_up)

        f_adap, chosen = filter_adaptive(img)
        e_adap = calculate_entropy(f_adap)

        c = Counter(chosen)
        top_f_num = c.most_common(1)[0][0]
        f_names = {0: "None", 1: "Sub", 2: "Up"}
        opt_f = f_names[top_f_num]

        gain = ((e_orig - e_adap) / e_orig * 100.0) if e_orig > 0 else 0.0

        print(f"{name:<12} | {e_orig:<10.2f} | {e_sub:<8.2f} | {e_up:<8.2f} | {e_adap:<8.2f} | {opt_f:<11} | {gain:<8.1f}%")

        for f in (0, 1, 2):
            enc = filter_image(img, f)
            back = unfilter_image(enc, f)
            assert np.array_equal(img, back), f"Помилка відновлення {name} для f={f}"

        back_adap = unfilter_adaptive(f_adap, chosen)
        assert np.array_equal(img, back_adap), f"Помилка відновлення {name} для Adaptive"

    print("\n[OK] Усі зворотності (Unfilter) перевірено: 100% точне відновлення!")


if __name__ == "__main__":
    main()