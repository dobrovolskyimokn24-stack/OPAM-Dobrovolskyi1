import io
import zlib
import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from PIL import Image
from skimage import data
from skimage.metrics import peak_signal_noise_ratio as psnr
from skimage.metrics import structural_similarity as ssim

Q50 = np.array([
    [16, 11, 10, 16,  24,  40,  51,  61],
    [12, 12, 14, 19,  26,  58,  60,  55],
    [14, 13, 16, 24,  40,  57,  69,  56],
    [14, 17, 22, 29,  51,  87,  80,  62],
    [18, 22, 37, 56,  68, 109, 103,  77],
    [24, 35, 55, 64,  81, 104, 113,  92],
    [49, 64, 78, 87, 103, 121, 120, 101],
    [72, 92, 95, 98, 112, 100, 103,  99],
], dtype=float)

def load_gray(path=None):
    if path is None:
        img = np.array(Image.fromarray(data.astronaut()).convert("L"))
    else:
        img = np.array(Image.open(path).convert("L"))
    h, w = img.shape
    return img[:h // 8 * 8, :w // 8 * 8]

def save_gray(img, path):
    Image.fromarray(img.astype(np.uint8)).save(path)
    print("   збережено:", path)

def blockify(img):
    h, w = img.shape
    return (img.reshape(h // 8, 8, w // 8, 8)
               .transpose(0, 2, 1, 3)
               .reshape(-1, 8, 8))

def unblockify(blocks, h, w):
    return (blocks.reshape(h // 8, w // 8, 8, 8)
                  .transpose(0, 2, 1, 3)
                  .reshape(h, w))

def packed(coeffs):
    return len(zlib.compress(coeffs.astype(np.int16).tobytes(), 9))

def quality_of(orig, out):
    return (psnr(orig, out, data_range=255),
            ssim(orig, out, data_range=255))

def jpeg_bytes(img, q):
    bio = io.BytesIO()
    Image.fromarray(img).save(bio, "JPEG", quality=q)
    return len(bio.getvalue())

def plot_curve(rows, path="curve.png"):
    q  = [r[0] for r in rows]
    kb = [r[1] / 1024 for r in rows]
    P  = [r[2] for r in rows]
    S  = [r[3] for r in rows]
    fig, ax = plt.subplots(1, 2, figsize=(11, 4))
    ax[0].plot(q, kb, "o-", color="#B35A1F", lw=2)
    ax[0].set_xlabel("якість q"); ax[0].set_ylabel("розмір, КБ")
    ax[0].set_title("Розмір"); ax[0].grid(alpha=0.3)
    ax[1].plot(q, P, "o-", color="#1F6F66", lw=2)
    ax[1].set_xlabel("якість q"); ax[1].set_ylabel("PSNR, дБ")
    ax[1].tick_params(axis="y", labelcolor="#1F6F66")
    tw = ax[1].twinx()
    tw.plot(q, S, "^-", color="#2E5E8A", lw=2)
    tw.set_ylabel("SSIM"); tw.tick_params(axis="y", labelcolor="#2E5E8A")
    ax[1].set_title("Якість"); ax[1].grid(alpha=0.3)
    fig.tight_layout()
    fig.savefig(path, dpi=140)
    plt.close(fig)
    print("   збережено:", path)

def dct_matrix(N=8):
    D = np.zeros((N, N))
    for k in range(N):
        a = np.sqrt(1 / N) if k == 0 else np.sqrt(2 / N)
        for n in range(N):
            D[k, n] = a * np.cos(np.pi * (2 * n + 1) * k / (2 * N))
    return D

def dct2(block):
    D = dct_matrix()
    return D @ block @ D.T

def idct2(coeffs):
    D = dct_matrix()
    return D.T @ coeffs @ D

def qtable(q):
    q = max(1, min(99, q))
    if q < 50:
        S = 5000 / q
    else:
        S = 200 - 2 * q
    T = np.floor((S * Q50 + 50) / 100)
    return np.clip(T, 1, 255)

def encode(img, q):
    Q = qtable(q)
    img_float = img.astype(float) - 128
    blocks = blockify(img_float)
    coeffs = np.array([np.round(dct2(b) / Q) for b in blocks])
    return coeffs.astype(np.int16), Q

def decode(coeffs, Q, h, w):
    blocks = np.array([idct2(b * Q) for b in coeffs])
    img = unblockify(blocks, h, w)
    img += 128
    return np.clip(img, 0, 255).astype(np.uint8)

def check(label, ok, detail=""):
    print(f"   [{'OK ' if ok else 'НІ '}] {label}" + ("" if ok or not detail else f"   — {detail}"))
    if not ok:
        print("\n   Зупиняюся. Виправте це місце й запустіть знову.")
        raise SystemExit(1)

def self_test():
    img = load_gray()
    h, w = img.shape

    print("\n1. Матриця базису DCT")
    D = dct_matrix()
    check("розмір 8×8", D.shape == (8, 8))
    check("не порожня", np.abs(D).sum() > 0)
    check("рядки одиничної довжини", np.allclose((D**2).sum(axis=1), 1),
          f"сума квадратів рядка 0 = {(D[0]**2).sum():.3f}")
    check("рядки взаємно перпендикулярні", np.allclose(D @ D.T, np.eye(8), atol=1e-9))

    print("\n2. DCT туди й назад")
    b = img[88:96, 444:452].astype(float) - 128
    back = idct2(dct2(b))
    e = np.abs(back - b).max()
    check("блок відновлюється точно", e < 1e-6, f"макс похибка {e:.1e}")

    print("\n3. Таблиця квантування")
    check("при q=50 збігається з Q50", np.allclose(qtable(50), Q50))
    check("при більшій якості числа менші", qtable(90).mean() < qtable(50).mean(),
          f"{qtable(90).mean():.0f} проти {qtable(50).mean():.0f}")
    check("ніде не перевищує 255", qtable(1).max() <= 255)
    check("ніде не менше 1", qtable(99).min() >= 1)

    print("\n4. Квантування контрольного блоку")
    cq = np.round(dct2(b) / qtable(50))
    nz = int((cq != 0).sum())
    check("лишилося рівно 2 ненульові числа з 64", nz == 2, f"у вас {nz}")
    check("перші два числа — 26 і 16",
          [cq[0, 0], cq[0, 1]] == [26, 16], f"у вас {cq[0,0]:.0f} і {cq[0,1]:.0f}")

    print("\n5. Кодер і декодер на всьому зображенні")
    co, Q = encode(img, 50)
    check("форма коефіцієнтів", co.shape == (h * w // 64, 8, 8), str(co.shape))
    out = decode(co, Q, h, w)
    check("форма відновленого", out.shape == img.shape, str(out.shape))
    check("тип uint8", out.dtype == np.uint8, str(out.dtype))
    alive = (co != 0).sum(axis=(1, 2)).mean()
    check("у середньому вижило близько 8 коефіцієнтів із 64",
          abs(alive - 8.0) < 0.8, f"у вас {alive:.1f}")
    P, S = quality_of(img, out)
    check("PSNR при q=50 близько 34.7", abs(P - 34.7) < 0.6, f"у вас {P:.1f}")
    check("SSIM при q=50 близько 0.956", abs(S - 0.956) < 0.01, f"у вас {S:.3f}")

    print("\n   Усі тести пройдено. Можна братися за завдання 4 і 5.\n")
    return img

def build_curve(img):
    h, w = img.shape
    rows = []
    print(f"   {'q':>3}{'байт':>9}{'стиснення':>11}{'нулів':>8}{'PSNR':>8}{'SSIM':>8}")
    for q in (95, 90, 75, 50, 30, 15, 5):
        co, Q = encode(img, q)
        out = decode(co, Q, h, w)
        n = packed(co)
        P, S = quality_of(img, out)
        rows.append((q, n, P, S))
        print(f"   {q:>3}{n:>9}{h*w/n:>10.1f}x"
              f"{(co == 0).mean()*100:>7.1f}%{P:>8.1f}{S:>8.3f}")
        save_gray(out, f"Multimedia_Dev/lab06/q{q:02d}.png")
    plot_curve(rows, path="Multimedia_Dev/lab06/curve.png")
    return rows

if __name__ == "__main__":
    image = self_test()
    print("Завдання 4. Крива якості")
    build_curve(image)