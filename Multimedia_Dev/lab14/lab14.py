import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from scipy.io import wavfile

SR   = 44100
N    = 1024
HOP  = N // 2
WIN  = np.hanning(N + 1)[:-1]
FREQ = np.fft.rfftfreq(N, 1 / SR)


def music(dur=3.0, sr=SR):
    t = np.arange(int(dur * sr)) / sr
    x = np.zeros_like(t)
    for f0 in (220.0, 277.18, 329.63):
        for k, a in enumerate([1.0, 0.5, 0.3, 0.18, 0.1, 0.06], start=1):
            x += a * np.sin(2 * np.pi * f0 * k * t + k)
    x *= np.minimum(1, t * 8) * np.exp(-0.35 * t)
    x += 0.004 * np.random.default_rng(3).standard_normal(len(t))
    return 0.85 * x / np.abs(x).max()


def save_wav(name, x, sr=SR):
    wavfile.write(name, sr, (np.clip(x, -1, 1) * 32767).astype(np.int16))
    print("   збережено:", name)


def snr_db(clean, noisy):
    a, b = clean[N:-N], noisy[N:-N]
    return 10 * np.log10((a ** 2).mean() / (((b - a) ** 2).mean() + 1e-30))


def frames(x):
    for s in range(0, len(x) - N, HOP):
        yield s, x[s:s + N] * WIN


def plot_frame(db, thr, keep, name):
    fig, ax = plt.subplots(figsize=(9, 4))
    m = FREQ < 4000
    ax.plot(FREQ[m], db[m], lw=0.9, color="gray", label="спектр кадру")
    ax.plot(FREQ[m], thr[m], lw=2, color="#B35A1F", label="поріг")
    ax.scatter(FREQ[m & keep], db[m & keep], s=16, color="#1F6F66", label="зберігаємо")
    ax.set_xlabel("частота, Гц")
    ax.set_ylabel("дБ відносно найгучнішого")
    ax.set_ylim(-110, 5)
    ax.legend()
    ax.grid(alpha=0.3)
    fig.tight_layout()
    fig.savefig(name, dpi=130)
    plt.close(fig)
    print("   збережено:", name)


def plot_curve(rows, name):
    fig, ax = plt.subplots(1, 2, figsize=(11, 3.6))
    fl = [r[0] for r in rows]
    ax[0].plot(fl, [1 / r[1] for r in rows], "o-", color="gray", label="лише поріг")
    ax[0].plot(fl, [1 / r[3] for r in rows], "^-", color="#1F6F66", label="з маскуванням")
    ax[0].set_xlabel("поріг, дБ")
    ax[0].set_ylabel("у скільки разів менше чисел")
    ax[1].plot(fl, [r[2] for r in rows], "o-", color="gray", label="лише поріг")
    ax[1].plot(fl, [r[4] for r in rows], "^-", color="#1F6F66", label="з маскуванням")
    ax[1].set_xlabel("поріг, дБ")
    ax[1].set_ylabel("SNR, дБ")
    for a in ax:
        a.legend()
        a.grid(alpha=0.3)
    fig.tight_layout()
    fig.savefig(name, dpi=130)
    plt.close(fig)
    print("   збережено:", name)


def bark(f):
    f = np.asarray(f, dtype=float)
    return 13 * np.arctan(0.00076 * f) + 3.5 * np.arctan((f / 7500) ** 2)


def mask_curve(db):
    thr = np.full_like(db, -300.0)
    for i in np.where(db > db.max() - 50)[0]:
        dz = BZ - BZ[i]
        sl = np.where(dz < 0, 27 * (-dz), 10 * dz)
        thr = np.maximum(thr, db[i] - 10 - sl)
    return thr


def encode(x, floor_db=-60, use_mask=False):
    out = np.zeros(len(x) + N)
    kept = tot = 0
    for s, fr in frames(x):
        sp = np.fft.rfft(fr)
        mag = np.abs(sp)
        db = 20 * np.log10(mag / (mag.max() + 1e-12) + 1e-12)
        thr = np.full_like(db, float(floor_db))
        if use_mask:
            thr = np.maximum(thr, mask_curve(db))
        keep = db > thr
        kept += keep.sum()
        tot += len(keep)
        out[s:s + N] += np.fft.irfft(sp * keep)
    return out[:len(x)], kept / tot


BZ = None


def check(label, ok, detail=""):
    print(f"   [{'OK ' if ok else 'НІ '}] {label}" + ("" if ok or not detail else f"   — {detail}"))
    if not ok:
        print("\n   Зупиняюся. Виправте це місце й запустіть знову.")
        raise SystemExit(1)


def self_test():
    global BZ
    print("\n1. Шкала Барка")
    check("100 Гц ≈ 1 барк", abs(bark(100) - 0.99) < 0.1, f"{float(bark(100)):.2f}")
    check("1000 Гц ≈ 8,5 барка", abs(bark(1000) - 8.51) < 0.2, f"{float(bark(1000)):.2f}")
    check("зростає монотонно", np.all(np.diff(bark(np.linspace(20, 16000, 500))) > 0))
    bw = 100 / (bark(1000 + 50) - bark(1000 - 50))
    check("критична смуга на 1 кГц ≈ 157 Гц", abs(bw - 157) < 15, f"{float(bw):.0f} Гц")
    BZ = bark(FREQ)

    print("\n2. Поріг маскування")
    db = np.full(len(FREQ), -200.0)
    i1k = np.argmin(np.abs(FREQ - 1000))
    db[i1k] = 0.0
    thr = mask_curve(db)
    check("під самим тоном поріг ≈ −10 дБ", abs(thr[i1k] + 10) < 0.5, f"{thr[i1k]:.1f}")
    i_up = np.argmin(np.abs(FREQ - 1200))
    i_dn = np.argmin(np.abs(FREQ - 800))
    check("вгору маскує далі, ніж униз", thr[i_up] > thr[i_dn],
          f"вгору {thr[i_up]:.1f}, вниз {thr[i_dn]:.1f}")
    check("далеко від тону поріг низький", thr[np.argmin(np.abs(FREQ - 6000))] < -60)

    print("\n3. Кодек")
    x = music()
    y, k = encode(x, floor_db=-300)
    check("без викидання відновлення точне", snr_db(x, y) > 100, f"SNR {snr_db(x, y):.1f} дБ")
    check("при цьому збережено все", k > 0.99, f"{k * 100:.1f}%")
    y, k = encode(x, floor_db=-60)
    check("поріг −60 дБ лишає близько 12,6 %", abs(k - 0.126) < 0.02, f"{k * 100:.1f}%")
    y2, k2 = encode(x, floor_db=-60, use_mask=True)
    check("маскування викидає ще більше", k2 < k, f"{k2 * 100:.1f}% проти {k * 100:.1f}%")
    check("і показує гірший SNR", snr_db(x, y2) < snr_db(x, y),
          f"{snr_db(x, y2):.1f} проти {snr_db(x, y):.1f} дБ")

    print("\n   Усі тести пройдено. Далі — завдання 4.\n")
    return x


def run_all(x):
    print("Один кадр під мікроскопом")
    s = int(0.5 * SR)
    fr = x[s:s + N] * WIN
    sp = np.abs(np.fft.rfft(fr))
    db = 20 * np.log10(sp / sp.max() + 1e-12)
    thr = np.maximum(mask_curve(db), -60)
    keep = db > thr
    print(f"   зберігаємо {int(keep.sum())} чисел із {len(keep)}")
    plot_frame(db, thr, keep, "frame.png")

    print("\nКрива стиснення")
    rows = []
    print(f"   {'поріг':>7}{'лише поріг':>22}{'з маскуванням':>24}")
    for fl in (-80, -60, -50, -40, -30, -25, -20):
        y1, k1 = encode(x, fl)
        y2, k2 = encode(x, fl, True)
        rows.append((fl, k1, snr_db(x, y1), k2, snr_db(x, y2)))
        print(f"   {fl:>5} дБ   {1 / k1:>6.1f}× SNR {snr_db(x, y1):>5.1f}   "
              f"{1 / k2:>6.1f}× SNR {snr_db(x, y2):>5.1f}")
    plot_curve(rows, "curve.png")

    print("\nФайли для прослуховування")
    save_wav("original.wav", x)
    for fl in (-60, -40, -25):
        save_wav(f"plain_{-fl}.wav", encode(x, fl)[0])
        save_wav(f"mask_{-fl}.wav", encode(x, fl, True)[0])


if __name__ == "__main__":
    try:
        x = self_test()
    except NotImplementedError as e:
        print(f"\n   Місце {e} ще не зроблено. Знайдіть його у файлі за словом TODO,")
        print("   заповніть за підказкою поруч — і запустіть файл знову.\n")
        raise SystemExit(1)
    run_all(x)
    print("\n   Тепер ПОСЛУХАЙТЕ пари plain_*.wav і mask_*.wav.")
    print("   Числа кажуть, що маскування гірше. Перевірте це вухом.")