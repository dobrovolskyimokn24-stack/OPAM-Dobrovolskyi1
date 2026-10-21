import sys
import struct
import numpy as np


# ==========================================
# Модуль з ЛР1: Низькорівневе читання/запис BMP та PPM
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


def write_bmp24(path, img):
    if img.ndim == 2:  # Градації сірого -> RGB
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
# Завдання 1. Дві функції гами (sRGB <-> Linear Light)
# ==========================================
def to_light(code):
    c = np.asarray(code, dtype=np.float64) / 255.0
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


def to_code(light):
    l = np.asarray(light, dtype=np.float64)
    s = np.where(l <= 0.0031308, l * 12.92, 1.055 * (l ** (1.0 / 2.4)) - 0.055)
    return np.clip(s * 255.0, 0, 255)


# ==========================================
# Завдання 2. Шахівниця та пропорційне зменшення
# ==========================================
def make_board(size=512, cell=8):
    yy, xx = np.indices((size, size))
    return (((yy // cell + xx // cell) % 2) * 255).astype(np.uint8)


def box_average(img, k):
    h, w = img.shape[:2]
    if img.ndim == 3:
        return img.reshape(h // k, k, w // k, k, 3).mean(axis=(1, 3))
    return img.reshape(h // k, k, w // k, k).mean(axis=(1, 3))


# ==========================================
# Завдання 3. YCbCr колірний простір
# ==========================================
def rgb_to_ycbcr(img):
    x = img.astype(np.float64)
    r, g, b = x[..., 0], x[..., 1], x[..., 2]

    y = 0.299 * r + 0.587 * g + 0.114 * b
    cb = 128.0 - 0.168736 * r - 0.331264 * g + 0.5 * b
    cr = 128.0 + 0.5 * r - 0.418688 * g - 0.081312 * b

    return y, cb, cr


def ycbcr_to_rgb(y, cb, cr):
    r = y + 1.402 * (cr - 128.0)
    g = y - 0.344136 * (cb - 128.0) - 0.714136 * (cr - 128.0)
    b = y + 1.772 * (cb - 128.0)

    rgb = np.stack([r, g, b], axis=-1)
    return np.clip(np.round(rgb), 0, 255).astype(np.uint8)


# ==========================================
# Завдання 4. HSV колірний простір
# ==========================================
def rgb_to_hsv(img):
    x = img.astype(np.float64) / 255.0
    r, g, b = x[..., 0], x[..., 1], x[..., 2]
    mx = x.max(-1)
    mn = x.min(-1)
    d = mx - mn

    h = np.zeros_like(mx)
    m = (d != 0) & (mx == r)
    h[m] = (60.0 * ((g - b)[m] / d[m])) % 360.0
    m = (d != 0) & (mx == g)
    h[m] = 60.0 * ((b - r)[m] / d[m]) + 120.0
    m = (d != 0) & (mx == b)
    h[m] = 60.0 * ((r - g)[m] / d[m]) + 240.0

    s = np.where(mx == 0, 0, d / np.where(mx == 0, 1, mx))
    return h, s, mx


def hsv_to_rgb(h, s, v):
    c = v * s
    x = c * (1.0 - np.abs(((h / 60.0) % 2) - 1.0))
    m = v - c

    zeros = np.zeros_like(h)
    r_prime = np.zeros_like(h)
    g_prime = np.zeros_like(h)
    b_prime = np.zeros_like(h)

    conds = [
        (0 <= h) & (h < 60),
        (60 <= h) & (h < 120),
        (120 <= h) & (h < 180),
        (180 <= h) & (h < 240),
        (240 <= h) & (h < 300),
        (300 <= h) & (h <= 360)
    ]

    r_p = [c, x, zeros, zeros, x, c]
    g_p = [x, c, c, x, zeros, zeros]
    b_p = [zeros, zeros, x, c, c, x]

    for i in range(6):
        r_prime[conds[i]] = r_p[i][conds[i]]
        g_prime[conds[i]] = g_p[i][conds[i]]
        b_prime[conds[i]] = b_p[i][conds[i]]

    rgb = np.stack([r_prime + m, g_prime + m, b_prime + m], axis=-1) * 255.0
    return np.clip(np.round(rgb), 0, 255).astype(np.uint8)


# ==========================================
# Головний функціонал / Виконання завдань
# ==========================================
def main():
    print("=== Завдання 1. Перевірка тотожності гами ===")
    mid = (to_light(0) + to_light(255)) / 2.0
    print("to_code(mid):", round(float(to_code(mid))))
    print("to_light(128) %:", round(float(to_light(128)) * 100, 1))
    print("to_light(64) %:", round(float(to_light(64)) * 100, 1))
    print("to_light(192) %:", round(float(to_light(192)) * 100, 1))

    print("\n=== Завдання 2. Шахівниця ===")
    board = make_board(512, 8)
    naive = box_average(board.astype(np.float64), 16)
    board_light = to_light(board)
    correct_light = box_average(board_light, 16)
    correct = to_code(correct_light)

    print("Наївно середній код:    ", round(float(naive.mean()), 1))
    print("Правильно середній код: ", round(float(correct.mean()), 1))

    orig_light_mean = float(board_light.mean()) * 100
    naive_light_mean = float(to_light(naive).mean()) * 100
    correct_light_mean = float(correct_light.mean()) * 100

    print(f"Оригінал світло:          {round(orig_light_mean, 1)}%")
    print(f"Наївно зменшене світло:   {round(naive_light_mean, 1)}%")
    print(f"Правильно зменшене світло:{round(correct_light_mean, 1)}%")

    write_bmp24("Multimedia_Dev/lab02/board_naive.bmp", np.round(naive).astype(np.uint8))
    write_bmp24("Multimedia_Dev/lab02/board_correct.bmp", np.round(correct).astype(np.uint8))

    print("\n=== Завдання 3 та 4 (Тестування на фотографії) ===")
    try:
        img = read_ppm("Multimedia_Dev/lab01/test.ppm")
    except Exception:
        img = np.zeros((100, 100, 3), dtype=np.uint8)
        img[0:50, 0:50] = [255, 100, 50]
        img[50:100, 50:100] = [0, 200, 100]

    # YCbCr
    y, cb, cr = rgb_to_ycbcr(img)
    back_ycbcr = ycbcr_to_rgb(y, cb, cr)
    max_diff_ycbcr = np.abs(back_ycbcr.astype(int) - img.astype(int)).max()
    print(f"YCbCr max diff: {max_diff_ycbcr}")

    y_gray = np.clip(np.round(y), 0, 255).astype(np.uint8)
    naive_gray = img.astype(np.float64).mean(axis=-1).astype(np.uint8)

    write_bmp24("Multimedia_Dev/lab02/photo_y_channel.bmp", y_gray)
    write_bmp24("Multimedia_Dev/lab02/photo_naive_gray.bmp", naive_gray)

    # HSV
    h, s, v = rgb_to_hsv(img)

    # 1. Насиченість * 0.3
    h1, s1, v1 = h.copy(), np.clip(s * 0.3, 0, 1), v.copy()
    write_bmp24("Multimedia_Dev/lab02/hsv_sat_low.bmp", hsv_to_rgb(h1, s1, v1))

    # 2. Насиченість * 2.0
    h2, s2, v2 = h.copy(), np.clip(s * 2.0, 0, 1), v.copy()
    write_bmp24("Multimedia_Dev/lab02/hsv_sat_high.bmp", hsv_to_rgb(h2, s2, v2))

    # 3. Тон + 60
    h3, s3, v3 = (h + 60.0) % 360.0, s.copy(), v.copy()
    write_bmp24("Multimedia_Dev/lab02/hsv_hue_shift.bmp", hsv_to_rgb(h3, s3, v3))

    print("Всі результати збережено в Multimedia_Dev/lab02/")


if __name__ == "__main__":
    main()