import sys
import struct
import numpy as np


# ==========================================
# Завдання 1. Створення тесту та читання PPM
# ==========================================
def create_test_ppm(path="test.ppm"):
    img = np.zeros((4, 6, 3), dtype=np.uint8)
    img[0:2, 0:3] = (255, 0, 0)      # Верхній лівий — червоний
    img[0:2, 3:6] = (0, 255, 0)      # Верхній правий — зелений
    img[2:4, 0:3] = (0, 0, 255)      # Нижній лівий — синій
    img[2:4, 3:6] = (255, 255, 255)  # Нижній правий — білий

    with open(path, "wb") as f:
        f.write(b"P6\n6 4\n255\n")
        f.write(img.tobytes())
    return img


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

    if len(data) - pos < need:
        raise ValueError("Файл обірваний: пікселів менше, ніж треба")

    pixels = np.frombuffer(data, dtype=np.uint8, count=need, offset=pos)
    return pixels.reshape(height, width, 3)


def write_ppm(path, img):
    height, width, _ = img.shape
    with open(path, "wb") as f:
        header = f"P6\n{width} {height}\n255\n".encode('ascii')
        f.write(header)
        f.write(img.tobytes())


# ==========================================
# Завдання 2. Запис 24-бітного BMP
# ==========================================
def write_bmp24(path, img):
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
# Завдання 3. Читання 24-бітного BMP
# ==========================================
def read_bmp24(path):
    with open(path, "rb") as f:
        data = f.read()

    offbits = struct.unpack_from("<I", data, 10)[0]
    width = struct.unpack_from("<i", data, 18)[0]
    height = struct.unpack_from("<i", data, 22)[0]
    bits = struct.unpack_from("<H", data, 28)[0]

    if bits != 24:
        raise ValueError("Ця функція підтримує лише 24-бітні BMP")

    stride = ((width * bits + 31) // 32) * 4
    img = np.zeros((height, width, 3), dtype=np.uint8)

    for y in range(height):
        start = offbits + y * stride
        for x in range(width):
            b = data[start + x * 3 + 0]
            g = data[start + x * 3 + 1]
            r = data[start + x * 3 + 2]
            img[height - 1 - y, x] = (r, g, b)

    return img


# ==========================================
# Завдання 4. Палітровий (8-бітний) BMP
# ==========================================
def write_bmp8_palette(path, img):
    height, width, _ = img.shape
    stride = ((width * 8 + 31) // 32) * 4
    padding = stride - width
    offbits = 1078
    filesize = offbits + stride * height

    out = bytearray()
    out += struct.pack("<2sIHHI", b"BM", filesize, 0, 0, offbits)
    out += struct.pack("<IiiHHIIiiII", 40, width, height, 1, 8,
                       0, stride * height, 2835, 2835, 256, 0)

    palette = bytearray()
    for i in range(256):
        if i < 216:
            r = (i // 36) * 51
            g = ((i // 6) % 6) * 51
            b = (i % 6) * 51
        else:
            r = g = b = 0
        palette += bytes((b, g, r, 0))
    out += palette

    def color_index(r, g, b):
        return 36 * (r // 51) + 6 * (g // 51) + (b // 51)

    for y in range(height - 1, -1, -1):
        for x in range(width):
            r, g, b = img[y, x]
            out.append(color_index(r, g, b))
        out += b"\x00" * padding

    with open(path, "wb") as f:
        f.write(out)


# ==========================================
# Завдання 5. Головна програма / Конвертер
# ==========================================
def main():
    if len(sys.argv) == 3:
        src, dst = sys.argv[1], sys.argv[2]
        if src.endswith(".ppm") and dst.endswith(".bmp"):
            img = read_ppm(src)
            write_bmp24(dst, img)
            print("Успішно сконвертовано у 24-бітний BMP!")
        elif src.endswith(".bmp") and dst.endswith(".ppm"):
            img = read_bmp24(src)
            write_ppm(dst, img)
            print("Успішно сконвертовано у PPM!")
        else:
            print("Підтримуються лише розширення .ppm та .bmp")
        return

    print("--- Запуск самоперевірки Завдань 1-4 ---")
    orig_img = create_test_ppm("test.ppm")
    print("1. test.ppm успішно створено.")

    read_ppm_img = read_ppm("test.ppm")
    print(f"2. read_ppm() прочитав масив формою: {read_ppm_img.shape}")

    write_bmp24("test.bmp", read_ppm_img)
    print("3. test.bmp (24-біт) збережено.")

    read_bmp_img = read_bmp24("test.bmp")
    is_equal = np.array_equal(read_ppm_img, read_bmp_img)
    print(f"4. Порівняння оригінального PPM та прочитаного BMP: {is_equal}")

    write_bmp8_palette("test_palette.bmp", read_ppm_img)
    print("5. test_palette.bmp (8-біт палітровий) збережено.")


if __name__ == "__main__":
    main()