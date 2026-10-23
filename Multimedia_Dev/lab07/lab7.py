import struct
import time
import numpy as np


# ==========================================
# Модуль запису 24-бітного BMP (з ЛР1, без залежностей)
# ==========================================
def write_bmp24(path, img):
    if img.ndim == 2:
        img = np.stack([img, img, img], axis=-1)

    height, width, _ = img.shape
    stride = ((width * 24 + 31) // 32) * 4
    padding = stride - width * 3
    offbits = 54
    filesize = offbits + stride * height

    out = bytearray()
    out += struct.pack("<2sIHHI", b"BM", filesize, 0, 0, offbits)
    out += struct.pack("<IiiHHIIiiII", 40, width, height, 1, 24,
                       0, stride * height, 2835, 2835, 0, 0)

    for y in range(height - 1, -1, -1):
        for x in range(width):
            r, g, b = img[y, x]
            out += bytes((b, g, r))
        out += b"\x00" * padding

    with open(path, "wb") as f:
        f.write(out)


# ==========================================
# Завдання 1. Пряма 2D Згортка (NumPy Vectorized)
# ==========================================
def conv2(img, K):
    kh, kw = K.shape
    ph, pw = kh // 2, kw // 2
    pad = np.pad(img, ((ph, ph), (pw, pw)), mode='edge')
    out = np.zeros_like(img, dtype=float)
    for i in range(kh):
        for j in range(kw):
            out += K[i, j] * pad[i:i + img.shape[0], j:j + img.shape[1]]
    return out


# ==========================================
# Завдання 2. Одновимірне Ядро Гауса
# ==========================================
def gauss1d(sigma, n):
    x = np.arange(n) - n // 2
    k = np.exp(-x**2 / (2 * sigma**2))
    return k / k.sum()


# ==========================================
# Завдання 3. Роздільна Згортка (Separable Convolution)
# ==========================================
def conv_sep(img, k):
    n = len(k) // 2
    # Горизонтальний прохід
    pad_h = np.pad(img, ((0, 0), (n, n)), mode='edge')
    tmp = np.zeros_like(img, dtype=float)
    for j, v in enumerate(k):
        tmp += v * pad_h[:, j:j + img.shape[1]]

    # Вертикальний прохід
    pad_v = np.pad(tmp, ((n, n), (0, 0)), mode='edge')
    out = np.zeros_like(img, dtype=float)
    for i, v in enumerate(k):
        out += v * pad_v[i:i + img.shape[0], :]
    return out


# ==========================================
# Допоміжна функція заміру часу
# ==========================================
def best(f, r=5):
    ts = []
    for _ in range(r):
        t = time.perf_counter()
        f()
        ts.append(time.perf_counter() - t)
    return min(ts) * 1000.0  # у мілісекундах


# ==========================================
# Генерація Тестового Градієнтного Синтетичного Зображення (512x512)
# ==========================================
def generate_synthetic_image(size=512):
    x = np.linspace(0, 1, size)
    y = np.linspace(0, 1, size)
    xx, yy = np.meshgrid(x, y)
    # Створення зображення з текстурами та контрастними межами
    img = np.sin(xx * 15) * np.cos(yy * 15) * 100 + 128
    img[128:384, 128:384] += 50.0
    return np.clip(img, 0, 255)


# ==========================================
# Головний Виконавчий Скрипт
# ==========================================
def main():
    print("=== Завдання 1 & 2. Запуск та згортка ядрами ===")
    gray = generate_synthetic_image(512)

    # 1. Розмиття за Гаусом
    k7 = gauss1d(1.6, 7)
    BLUR_K = np.outer(k7, k7)
    blur = conv2(gray, BLUR_K)
    write_bmp24('Multimedia_Dev/lab07/blur.bmp', np.clip(blur, 0, 255).astype(np.uint8))

    # 2. Оператор Собеля (Пошук Країв)
    SX = np.array([[-1, 0, 1],
                   [-2, 0, 2],
                   [-1, 0, 1]], float)
    SY = SX.T

    gx = conv2(gray, SX)
    gy = conv2(gray, SY)
    edges = np.hypot(gx, gy)
    edges = edges / edges.max() * 255.0
    write_bmp24('Multimedia_Dev/lab07/sobel.bmp', np.clip(edges, 0, 255).astype(np.uint8))

    # 3. Підвищення Різкості (Unsharp Masking)
    detail = gray - blur
    sharp = np.clip(gray + 1.2 * detail, 0, 255)
    write_bmp24('Multimedia_Dev/lab07/sharp.bmp', sharp.astype(np.uint8))

    print("[OK] Зображення blur.bmp, sobel.bmp, sharp.bmp збережено успішно.")

    # Точність Роздільної Згортки
    sep_blur = conv_sep(gray, k7)
    diff = np.abs(blur - sep_blur).max()
    print(f"Максимальна розбіжність conv2 vs conv_sep: {diff:.2e}")

    print("\n=== Завдання 3 & 4. Заміри Продуктивності та Виграшу ===")
    print(f"{'Ядро':<8} | {'2D 2D (мс)':<12} | {'Роздільне 1D (мс)':<18} | {'Виграш (x)':<10}")
    print("-" * 55)

    sizes = [3, 5, 9, 15, 31]
    for n in sizes:
        sigma = max(n / 5.0, 0.6)
        k = gauss1d(sigma, n)
        K = np.outer(k, k)

        ta = best(lambda: conv2(gray, K))
        tb = best(lambda: conv_sep(gray, k))
        speedup = ta / tb if tb > 0 else 0.0

        print(f"{n:>2}x{n:<2}   | {ta:<12.2f} | {tb:<18.2f} | {speedup:<10.2f}x")

    print("\n[OK] Роботу успішно виконано!")


if __name__ == "__main__":
    main()