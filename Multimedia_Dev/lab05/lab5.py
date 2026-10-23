import struct
import numpy as np


# ==========================================
# Модуль запису BMP з ЛР1 (без сторонніх бібліотек)
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
# Завдання 1. Пряме та зворотне перетворення YCbCr
# ==========================================
def rgb2ycbcr(img):
    a = img.astype(np.float64)
    R, G, B = a[..., 0], a[..., 1], a[..., 2]
    Y = 0.299 * R + 0.587 * G + 0.114 * B
    Cb = 128.0 - 0.168736 * R - 0.331264 * G + 0.5 * B
    Cr = 128.0 + 0.5 * R - 0.418688 * G - 0.081312 * B
    return Y, Cb, Cr


def ycbcr2rgb(Y, Cb, Cr):
    R = Y + 1.402 * (Cr - 128.0)
    G = Y - 0.344136 * (Cb - 128.0) - 0.714136 * (Cr - 128.0)
    B = Y + 1.772 * (Cb - 128.0)
    rgb = np.stack([R, G, B], axis=-1)
    return np.clip(np.round(rgb), 0, 255).astype(np.uint8)


# ==========================================
# Завдання 2 & 3. Схеми субдискретизації
# ==========================================
def down422(C):
    h, w = C.shape
    return C.reshape(h, w // 2, 2).mean(axis=2)


def up422(C, w):
    return np.repeat(C, 2, axis=1)[:, :w]


def down420(C):
    h, w = C.shape
    return C.reshape(h // 2, 2, w // 2, 2).mean(axis=(1, 3))


def up420(C, h, w):
    return np.repeat(np.repeat(C, 2, axis=0), 2, axis=1)[:h, :w]


def scheme(img, mode):
    Y, Cb, Cr = rgb2ycbcr(img)
    h, w = Y.shape
    if mode == '4:4:4':
        pass
    elif mode == '4:2:2':
        Cb = up422(down422(Cb), w)
        Cr = up422(down422(Cr), w)
    elif mode == '4:2:0':
        Cb = up420(down420(Cb), h, w)
        Cr = up420(down420(Cr), h, w)
    return ycbcr2rgb(Y, Cb, Cr)


# ==========================================
# Генератори тестових зображень
# ==========================================
def redtext(W=320, H=96):
    im = np.zeros((H, W, 3), dtype=np.uint8)
    # Малюємо текстуру «RED»
    im[20:76, 30:40] = [255, 0, 0]
    im[20:30, 30:70] = [255, 0, 0]
    im[43:53, 30:65] = [255, 0, 0]
    im[20:76, 60:70] = [255, 0, 0]

    im[20:76, 90:100] = [255, 0, 0]
    im[20:30, 90:130] = [255, 0, 0]
    im[43:53, 90:125] = [255, 0, 0]
    im[66:76, 90:130] = [255, 0, 0]

    im[20:76, 150:160] = [255, 0, 0]
    im[20:30, 150:190] = [255, 0, 0]
    im[66:76, 150:190] = [255, 0, 0]
    im[20:76, 180:190] = [255, 0, 0]
    return im


def lines(W=320, H=160):
    im = np.zeros((H, W, 3), np.uint8)
    cols = [(255, 0, 0), (0, 255, 0), (0, 0, 255),
            (255, 255, 0), (0, 255, 255), (255, 0, 255)]
    for i, c in enumerate(cols):
        x = 24 + i * 48
        im[12:70, x:x + 2] = c
        im[12:70, x + 22:x + 23] = c
    for i, c in enumerate(cols[:4]):
        y = 90 + i * 16
        im[y:y + 2, 24:296] = c
        im[y + 7:y + 8, 24:296] = c
    return im


def generate_photo(W=320, H=320):
    xx, yy = np.meshgrid(np.linspace(0, 1, W), np.linspace(0, 1, H))
    r = (np.sin(xx * 5) * 127 + 128).astype(np.uint8)
    g = (np.cos(yy * 5) * 127 + 128).astype(np.uint8)
    b = (np.sin((xx + yy) * 5) * 127 + 128).astype(np.uint8)
    return np.stack([r, g, b], axis=-1)


# ==========================================
# Завдання 4 & 5. Карта різниці та вимірювання
# ==========================================
def diffmap(orig, out):
    d = np.abs(out.astype(int) - orig.astype(int))
    return d.max(axis=2)


def measure(orig, mode):
    out = scheme(orig, mode)
    d = np.abs(out.astype(int) - orig.astype(int))
    mx = d.max(axis=2)
    return d.max(), d.mean(), (mx > 0).mean() * 100.0, (mx > 30).mean() * 100.0


def main():
    print("=== Завдання 1. Перевірка тотожності RGB <-> YCbCr ===")
    txt = redtext()
    Y, Cb, Cr = rgb2ycbcr(txt)
    back = ycbcr2rgb(Y, Cb, Cr)
    max_err = np.abs(back.astype(int) - txt.astype(int)).max()
    print(f"Макс похибка RGB <-> YCbCr: {max_err}")

    print("\n=== Збереження тестових зображень та карт різниці ===")
    out422 = scheme(txt, '4:2:2')
    write_bmp24('Multimedia_Dev/lab05/text422.bmp', np.concatenate([txt, out422], axis=0))

    out420 = scheme(txt, '4:2:0')
    write_bmp24('Multimedia_Dev/lab05/text420.bmp', np.concatenate([txt, out420], axis=0))

    lin = lines()
    trio = [scheme(lin, m) for m in ('4:4:4', '4:2:2', '4:2:0')]
    write_bmp24('Multimedia_Dev/lab05/lines420.bmp', np.concatenate(trio, axis=1))

    dm_txt = diffmap(txt, out420)
    dm_txt_vis = np.clip(dm_txt * 2, 0, 255).astype(np.uint8)
    write_bmp24('Multimedia_Dev/lab05/diff_text.bmp', dm_txt_vis)

    photo = generate_photo()
    out_photo = scheme(photo, '4:2:0')
    dm_photo = diffmap(photo, out_photo)
    dm_photo_vis = np.clip(dm_photo * 2, 0, 255).astype(np.uint8)
    write_bmp24('Multimedia_Dev/lab05/diff_photo.bmp', dm_photo_vis)

    print("\n=== Завдання 5. Підсумкова таблиця вимірювань ===")
    tests = {
        'червоний текст': txt,
        'тонкі лінії': lin,
        'фотографія': photo
    }

    print(f"{'Зображення':<16} | {'Схема':<6} | {'Макс':<5} | {'Середня':<7} | {'Змінених, %':<12} | {'Помітних, %':<12}")
    print("-" * 72)

    for name, im in tests.items():
        for m in ('4:4:4', '4:2:2', '4:2:0'):
            mx, mean_err, changed, visible = measure(im, m)
            print(f"{name:<16} | {m:<6} | {mx:<5d} | {mean_err:<7.2f} | {changed:<11.1f}% | {visible:<11.2f}%")

    print("\n[OK] Роботу успішно виконано, усі результати збережено!")


if __name__ == "__main__":
    main()