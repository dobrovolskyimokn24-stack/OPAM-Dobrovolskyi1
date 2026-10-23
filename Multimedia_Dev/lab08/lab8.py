import struct
import numpy as np


# ==========================================
# Модуль запису 24-бітного BMP (З ЛР1/ЛР5/ЛР7)
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
# Завдання 1. Алгоритми збільшення (Upsampling)
# ==========================================
def resize_nearest(img, new_shape):
    h_in, w_in = img.shape[:2]
    h_out, w_out = new_shape
    row_indices = (np.arange(h_out) * (h_in / h_out)).astype(int)
    col_indices = (np.arange(w_out) * (w_in / w_out)).astype(int)
    return img[np.ix_(row_indices, col_indices)]


def resize_bilinear(img, new_shape):
    h_in, w_in = img.shape[:2]
    h_out, w_out = new_shape

    y = np.linspace(0, h_in - 1, h_out)
    x = np.linspace(0, w_in - 1, w_out)

    y0 = np.floor(y).astype(int)
    y1 = np.minimum(y0 + 1, h_in - 1)
    x0 = np.floor(x).astype(int)
    x1 = np.minimum(x0 + 1, w_in - 1)

    dy = (y - y0)[:, None]
    dx = (x - x0)[None, :]

    IA = img[np.ix_(y0, x0)]
    IB = img[np.ix_(y0, x1)]
    IC = img[np.ix_(y1, x0)]
    ID = img[np.ix_(y1, x1)]

    wa = (1 - dy) * (1 - dx)
    wb = (1 - dy) * dx
    wc = dy * (1 - dx)
    wd = dy * dx

    out = IA * wa + IB * wb + IC * wc + ID * wd
    return np.clip(out, 0, 255).astype(np.uint8)


# ==========================================
# Завдання 2. Шахівниця та аліасинг
# ==========================================
def checker(N=512, cell=4, ang=18):
    y, x = np.indices((N, N)).astype(float) - N / 2.0
    a = np.deg2rad(ang)
    xr = x * np.cos(a) - y * np.sin(a)
    yr = x * np.sin(a) + y * np.cos(a)
    return (((np.floor(xr / cell) + np.floor(yr / cell)) % 2) * 255.0)


# ==========================================
# Завдання 4. Перетворення Гами (sRGB <-> Light)
# ==========================================
def to_light(c):
    c = c / 255.0
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


def to_code(L):
    c = np.where(L <= 0.0031308, L * 12.92, 1.055 * (L ** (1.0 / 2.4)) - 0.055)
    return np.clip(c * 255.0, 0, 255)


def half_naive(img):
    h, w = img.shape
    return img.reshape(h // 2, 2, w // 2, 2).mean(axis=(1, 3))


def half_linear(img):
    L = to_light(img)
    h, w = L.shape
    return to_code(L.reshape(h // 2, 2, w // 2, 2).mean(axis=(1, 3)))


# ==========================================
# Синтетичне тестове фото
# ==========================================
def generate_photo(size=512):
    x = np.linspace(0, 1, size)
    y = np.linspace(0, 1, size)
    xx, yy = np.meshgrid(x, y)
    r = (np.sin(xx * 10) * 127 + 128)
    g = (np.cos(yy * 10) * 127 + 128)
    b = (np.sin((xx + yy) * 10) * 127 + 128)
    return 0.299 * r + 0.587 * g + 0.114 * b


def main():
    print("=== Завдання 1. Збільшення фрагмента ===")
    photo = generate_photo(512)
    small = photo[340:356, 372:388]  # фрагмент 16x16

    up_near = resize_nearest(small, (320, 320))
    up_bilin = resize_bilinear(small, (320, 320))

    write_bmp24('Multimedia_Dev/lab08/up_nearest.bmp', up_near.astype(np.uint8))
    write_bmp24('Multimedia_Dev/lab08/up_bilinear.bmp', up_bilin.astype(np.uint8))
    print("[OK] Файли up_nearest.bmp та up_bilinear.bmp збережено.")

    print("\n=== Завдання 2. Зменшення та Аліасинг ===")
    ch = checker()
    k = 4
    nearest = ch[::k, ::k]
    area = ch.reshape(512 // k, k, 512 // k, k).mean(axis=(1, 3))

    write_bmp24('Multimedia_Dev/lab08/ds_nearest.bmp', nearest.astype(np.uint8))
    write_bmp24('Multimedia_Dev/lab08/ds_area.bmp', area.astype(np.uint8))

    print(f"кожен 4-й піксель: середнє {nearest.mean():.1f}, розкид {nearest.std():.1f}")
    print(f"усереднення 4x4:   середнє {area.mean():.1f}, розкид {area.std():.1f}")

    print("\n=== Завдання 3. Побудова ланцюжка міпмап (Наївно) ===")
    g = photo.copy()
    level = 0
    while g.shape[0] >= 2:
        write_bmp24(f'Multimedia_Dev/lab08/mip_{level}.bmp', g.astype(np.uint8))
        print(f"Level {level}: {g.shape[0]}x{g.shape[1]} | Середня яскравість: {g.mean():.1f}")
        g = half_naive(g)
        level += 1

    print("\n=== Завдання 4. Оцінка впливу Гама-корекції ===")
    cb = np.indices((256, 256)).sum(axis=0) % 2 * 255.0
    print(f"Шахівниця 1x1 наївно:   {half_naive(cb).mean():.1f}")
    print(f"Шахівниця 1x1 правильно: {half_linear(cb).mean():.1f}")

    print("\nНакопичення похибки на фотографії за рівнями міпмап:")
    print(f"{'Рівень':<8} | {'Розмір':<8} | {'Наївно':<8} | {'Правильно':<10} | {'Різниця':<8}")
    print("-" * 50)

    gn = photo.copy()
    gl = photo.copy()
    print(f"{0:<8} | {512:<8} | {gn.mean():<8.1f} | {gl.mean():<10.1f} | {0.0:<+8.1f}")

    for lv in range(1, 6):
        gn = half_naive(gn)
        gl = half_linear(gl)
        diff = gl.mean() - gn.mean()
        print(f"{lv:<8} | {gn.shape[0]:<8} | {gn.mean():<8.1f} | {gl.mean():<10.1f} | {diff:<+8.1f}")

    print("\n[OK] Роботу успішно виконано!")


if __name__ == "__main__":
    main()