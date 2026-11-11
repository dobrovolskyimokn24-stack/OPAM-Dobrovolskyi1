import numpy as np
from PIL import Image


# ════════════════════════════════════════════════════════════════════
#  ГОТОВЕ
# ════════════════════════════════════════════════════════════════════

def to565(c):
    """Квантування кольору під RGB565."""
    c = np.asarray(c, dtype=float)
    r = np.round(c[..., 0] / 255 * 31) / 31 * 255
    g = np.round(c[..., 1] / 255 * 63) / 63 * 255
    b = np.round(c[..., 2] / 255 * 31) / 31 * 255
    return np.stack([r, g, b], axis=-1)


def blocks_of(img):
    """Зображення (h, w, 3) -> масив блоків (кількість, 16, 3)."""
    h, w, _ = img.shape
    return (img.reshape(h // 4, 4, w // 4, 4, 3)
               .transpose(0, 2, 1, 3, 4)
               .reshape(-1, 16, 3).astype(float))


def unblocks(bl, h, w):
    """Зворотна операція до blocks_of."""
    return (bl.reshape(h // 4, w // 4, 4, 4, 3)
              .transpose(0, 2, 1, 3, 4)
              .reshape(h, w, 3))


def stats(a, b):
    """Повертає (макс, середня, RMSE, PSNR)."""
    d = np.abs(np.asarray(a, float) - np.asarray(b, float))
    rmse = np.sqrt((d ** 2).mean())
    psnr = 20 * np.log10(255 / rmse) if rmse > 0 else float("inf")
    return d.max(), d.mean(), rmse, psnr


def angle_error(a, b):
    """Кутова похибка між нормалями, у градусах."""
    def unit(x):
        v = np.asarray(x, float) / 255 * 2 - 1
        norm = np.linalg.norm(v, axis=-1, keepdims=True)
        norm[norm == 0] = 1.0
        return v / norm
    cos = (unit(a) * unit(b)).sum(-1)
    return np.degrees(np.arccos(np.clip(cos, -1, 1)))


def save(img, name):
    Image.fromarray(np.clip(img, 0, 255).astype(np.uint8)).save(name)
    print("   збережено:", name)


def photo(N=256):
    """Тестова фотографія/текстура (згенерована без додаткових бібліотек)."""
    y, x = np.indices((N, N)).astype(float) / N
    r = (np.sin(x * 10) * 0.5 + 0.5) * 255
    g = (np.cos(y * 10) * 0.5 + 0.5) * 255
    b = (np.sin((x + y) * 5) * 0.5 + 0.5) * 255
    return np.stack([r, g, b], -1).astype(np.uint8)

def gradient(N=256):
    x = np.linspace(0, 255, N)
    g = np.zeros((N, N, 3))
    for c in range(3):
        g[..., c] = x[None, :]
    return g.astype(np.uint8)

def normalmap(N=256, freq=14, amp=0.75):
    y, x = np.indices((N, N)).astype(float) / N * 2 * np.pi
    hx = amp * np.cos(freq * x) * np.cos(freq * 0.7 * y)
    hy = amp * np.cos(freq * 0.7 * x) * np.cos(freq * y)
    n = np.stack([hx, hy, np.ones_like(hx)], -1)
    n /= np.linalg.norm(n, axis=-1, keepdims=True)
    return ((n * 0.5 + 0.5) * 255).round().astype(np.uint8)

def stripes(N=256, wd=37):
    im = np.zeros((N, N, 3), np.uint8)
    cols = [(255,0,0), (0,255,0), (0,0,255), (255,255,0),
            (0,255,255), (255,0,255), (255,255,255)]
    for i in range(0, N, wd):
        im[:, i:i+wd] = cols[(i // wd) % len(cols)]
    return im

TESTS = {"фотографія": photo(), "градієнт": gradient(),
         "нормал-мапа": normalmap(), "різкі межі": stripes()}


# ════════════════════════════════════════════════════════════════════
#  РЕАЛІЗОВАНІ ТРИ МІСЦЯ (TODO 1, TODO 2, TODO 3)
# ════════════════════════════════════════════════════════════════════

def make_palette(c0, c1):
    c0 = np.asarray(c0, float)
    c1 = np.asarray(c1, float)
    return np.stack([c0, c1, (2 * c0 + c1) / 3, (c0 + 2 * c1) / 3])


def pick_endpoints(blk, mode="bbox"):
    if mode == "bbox":
        return blk.max(axis=0), blk.min(axis=0)
    else:
        L = blk @ np.array([0.299, 0.587, 0.114])
        return blk[L.argmax()], blk[L.argmin()]


def encode_block(blk, mode="bbox"):
    c0, c1 = pick_endpoints(blk, mode)
    pal = make_palette(to565(c0), to565(c1))
    d = ((blk[:, None, :] - pal[None, :, :]) ** 2).sum(-1)
    idx = d.argmin(axis=1)
    return pal, idx


# ════════════════════════════════════════════════════════════════════
#  КОДУВАННЯ ТА ТЕСТИ
# ════════════════════════════════════════════════════════════════════

def bc1(img, mode="bbox"):
    h, w, _ = img.shape
    out = np.zeros((img.shape[0] // 4 * img.shape[1] // 4, 16, 3))
    for i, blk in enumerate(blocks_of(img)):
        pal, idx = encode_block(blk, mode)
        out[i] = pal[idx]
    return unblocks(out, h, w)


def check(label, ok, detail=""):
    print(f"   [{'OK ' if ok else 'НІ '}] {label}" + ("" if ok or not detail else f"   — {detail}"))
    if not ok:
        print("\n   Зупиняюся. Виправте це місце й запустіть знову.")
        raise SystemExit(1)


def self_test():
    print("\n1. Палітра з чотирьох кольорів")
    pal = make_palette(np.array([255., 255., 255.]), np.array([0., 0., 0.]))
    check("форма (4, 3)", np.shape(pal) == (4, 3), str(np.shape(pal)))
    check("колір 0 — це перший опорний", np.allclose(pal[0], 255))
    check("колір 1 — це другий опорний", np.allclose(pal[1], 0))
    check("колір 2 на третині шляху", np.allclose(pal[2], 170, atol=1), f"у вас {pal[2][0]:.0f}")
    check("колір 3 на двох третинах", np.allclose(pal[3], 85, atol=1), f"у вас {pal[3][0]:.0f}")

    print("\n2. Вибір опорних кольорів")
    blk = np.zeros((16, 3))
    blk[:8] = [255, 0, 0]
    blk[8:] = [0, 255, 0]
    c0, c1 = pick_endpoints(blk, "bbox")
    check("коробка дає жовтий і чорний",
          np.allclose(sorted([c0.sum(), c1.sum()]), [0, 510]),
          f"{c0.round(0)} і {c1.round(0)}")
    c0, c1 = pick_endpoints(blk, "pixel")
    got = sorted([tuple(c0), tuple(c1)])
    check("реальні пікселі дають червоний і зелений",
          got == [(0., 255., 0.), (255., 0., 0.)], f"{c0.round(0)} і {c1.round(0)}")

    print("\n3. Кодування одного блоку")
    pal, idx = encode_block(blk, "pixel")
    check("індексів рівно 16", len(idx) == 16, str(len(idx)))
    check("усі індекси в діапазоні 0…3", idx.min() >= 0 and idx.max() <= 3)
    err = np.abs(pal[idx] - blk).max()
    check("два чисті кольори відтворено точно", err < 1, f"похибка {err:.0f}")
    pal, idx = encode_block(blk, "bbox")
    err = np.abs(pal[idx] - blk).max()
    check("а коробка на тому самому блоці помиляється", err > 100, f"похибка {err:.0f}")

    print("\n4. Ціле зображення")
    out = bc1(TESTS["фотографія"])
    check("розмір збігся", out.shape == TESTS["фотографія"].shape, str(out.shape))

    print("\n   Усі тести пройдено. Можна братися за завдання 3 і 4.\n")


def run_all():
    print("--- ЗАВДАННЯ 3: Порівняння двох методів (RMSE) ---")
    print(f"   {'зображення':<14}{'Коробка':>10}{'Пікселі':>10}{'Кращий':>12}")
    for name, im in TESTS.items():
        out_box = bc1(im, 'bbox')
        out_pix = bc1(im, 'pixel')
        r_box = stats(im, out_box)[2]
        r_pix = stats(im, out_pix)[2]
        winner = "пікселі" if r_pix < r_box else ("коробка" if r_box < r_pix else "однаково")
        print(f"   {name:<14}{r_box:>10.2f}{r_pix:>10.2f}{winner:>12}")
        save(out_box, f"bc1_{name}.png")

    print("\n--- ЗАВДАННЯ 4: Кутова похибка нормал-мапи ---")
    ang = angle_error(TESTS["нормал-мапа"], bc1(TESTS["нормал-мапа"], 'bbox'))
    print(f"   нормал-мапа (bbox): середня {ang.mean():.2f}°, макс {ang.max():.2f}°")
    print(f"   пікселів із похибкою більше 2°: {(ang > 2).mean() * 100:.1f}%")


if __name__ == "__main__":
    try:
        self_test()
    except NotImplementedError as e:
        print(f"\n   Місце {e} ще не зроблено.")
        raise SystemExit(1)
    run_all()