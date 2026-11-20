import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from scipy.io import wavfile
from scipy.signal import spectrogram

SR = 44100


def save_wav(name, x, sr=SR):
    x = np.asarray(x, dtype=float)
    peak = np.abs(x).max()
    if peak > 1.0:
        x = x / peak
    wavfile.write(name, sr, (x * 32767).astype(np.int16))
    print(f"   збережено: {name}")


def music(dur=3.0, sr=SR):
    t = np.arange(int(dur * sr)) / sr
    x = np.zeros_like(t)
    for f0 in (220.0, 277.18, 329.63):
        for k, a in enumerate([1.0, 0.5, 0.3, 0.18, 0.1, 0.06], start=1):
            x += a * np.sin(2 * np.pi * f0 * k * t + k)
    x *= np.minimum(1.0, t * 8.0) * np.exp(-0.35 * t)
    return 0.85 * x / np.abs(x).max()


def task1_spectrum_and_spectrogram(x):
    print("\n1. Спектр і спектрограма")
    n = 8192
    sp = np.abs(np.fft.rfft(x[:n] * np.hanning(n)))
    db = 20 * np.log10(sp / sp.max() + 1e-12)
    fr = np.fft.rfftfreq(n, 1 / SR)

    fig, ax = plt.subplots(figsize=(8, 4))
    m = fr < 2500
    ax.plot(fr[m], db[m], lw=1.0)
    ax.set_title("Спектр фрагмента (вікно Ганна, N=8192)")
    ax.set_xlabel("Частота, Гц")
    ax.set_ylabel("Рівень, дБ")
    ax.set_ylim(-95, 5)
    ax.grid(alpha=0.3)
    fig.tight_layout()
    fig.savefig("spectrum.png", dpi=130)
    plt.close(fig)
    print("   збережено: spectrum.png")

    f, t, S = spectrogram(x, SR, nperseg=2048, noverlap=1792)
    fig, ax = plt.subplots(figsize=(8, 4))
    ax.pcolormesh(t, f, 10 * np.log10(S + 1e-12), cmap="magma", shading="gouraud")
    ax.set_ylim(0, 2500)
    ax.set_title("Спектрограма сигналу (N=2048)")
    ax.set_xlabel("Час, с")
    ax.set_ylabel("Частота, Гц")
    fig.tight_layout()
    fig.savefig("spectrogram.png", dpi=130)
    plt.close(fig)
    print("   збережено: spectrogram.png")


def task2_windows():
    print("\n2. Порівняння вікон (N=1024)")
    n = 1024
    windows = (
        ("прямокутне", np.ones(n)),
        ("Ганна", np.hanning(n)),
        ("Блекмана", np.blackman(n)),
    )
    for k, label in ((100.0, "рівно на біні"), (100.5, "між бінами")):
        f = k * SR / n
        t = np.arange(n) / SR
        x = np.sin(2 * np.pi * f * t)
        for name, w in windows:
            sp = np.abs(np.fft.rfft(x * w))
            db = 20 * np.log10(sp / sp.max() + 1e-15)
            far = np.concatenate([db[:95], db[106:]])
            print(f"   {label:<14} | {name:<11} | найвищий сторонній пік: {far.max():6.1f} дБ")


def task3_time_vs_frequency():
    print("\n3. Компроміс час проти частоти")
    dur = 1.4
    t = np.arange(int(dur * SR)) / SR
    x = np.zeros_like(t)
    seg = t < 0.6
    x[seg] = 0.5 * np.sin(2 * np.pi * 440 * t[seg]) + 0.5 * np.sin(2 * np.pi * 480 * t[seg])
    for c in (0.80, 0.92, 1.04, 1.16):
        i = int(c * SR)
        x[i:i + 40] += 1.0 * np.hanning(40)

    for n in (128, 4096):
        f, tt, S = spectrogram(x, SR, nperseg=n, noverlap=int(n * 0.9))
        fig, ax = plt.subplots(figsize=(8, 4))
        ax.pcolormesh(tt, f, 10 * np.log10(S + 1e-12), cmap="magma", shading="gouraud")
        ax.set_ylim(0, 2000)
        ax.set_title(f"Спектрограма: вікно N={n} ({n / SR * 1000:.1f} мс, крок {SR / n:.1f} Гц)")
        ax.set_xlabel("Час, с")
        ax.set_ylabel("Частота, Гц")
        fig.tight_layout()
        fname = f"time_freq_{n}.png"
        fig.savefig(fname, dpi=130)
        plt.close(fig)
        print(f"   збережено: {fname}")


def eq(x, bands, sr=SR):
    sp = np.fft.rfft(x)
    fr = np.fft.rfftfreq(len(x), 1 / sr)
    g = np.ones(len(sp))
    for lo, hi, db in bands:
        g[(fr >= lo) & (fr < hi)] = 10 ** (db / 20)
    return np.fft.irfft(sp * g, len(x))


def band_energy(x, lo, hi, sr=SR):
    sp = np.abs(np.fft.rfft(x))
    fr = np.fft.rfftfreq(len(x), 1 / sr)
    return 10 * np.log10((sp[(fr >= lo) & (fr < hi)] ** 2).sum() + 1e-20)


def task4_equalizer(x):
    print("\n4. Еквалайзер та перевірка енергії смуг")
    y_test = eq(x, [(0, 250, 12)])
    bands_check = ((0, 250), (250, 1000), (1000, 4000), (4000, SR // 2))
    for lo, hi in bands_check:
        e_before = band_energy(x, lo, hi)
        e_after = band_energy(y_test, lo, hi)
        print(f"   смуга {lo:>4}–{hi:<5} Гц: {e_before:6.2f} дБ → {e_after:6.2f} дБ (різниця {e_after - e_before:+6.2f} дБ)")

    fig, axes = plt.subplots(1, 2, figsize=(10.5, 4))
    fr = np.fft.rfftfreq(len(x), 1 / SR)
    m = fr < 5000
    for ax, sig, title in (
        (axes[0], x, "До еквалайзера"),
        (axes[1], y_test, "Після еквалайзера (0–250 Гц: +12 дБ)"),
    ):
        sp = np.abs(np.fft.rfft(sig))
        db = 20 * np.log10(sp / sp.max() + 1e-12)
        ax.plot(fr[m], db[m], lw=0.7)
        ax.set_title(title)
        ax.set_xlabel("Частота, Гц")
        ax.set_ylabel("Рівень, дБ")
        ax.set_ylim(-95, 5)
        ax.grid(alpha=0.3)
    fig.tight_layout()
    fig.savefig("eq_spectrum.png", dpi=130)
    plt.close(fig)
    print("   збережено: eq_spectrum.png")

    presets = {
        "eq_bass_boost.wav": [(0, 250, 12), (250, 1000, -3)],
        "eq_voice_forward.wav": [(0, 150, -12), (1000, 4000, 9)],
        "eq_telephone.wav": [(0, 300, -60), (3000, SR // 2, -60)],
    }
    for fname, preset in presets.items():
        save_wav(fname, eq(x, preset))


if __name__ == "__main__":
    src = music()
    save_wav("source.wav", src)
    task1_spectrum_and_spectrogram(src)
    task2_windows()
    task3_time_vs_frequency()
    task4_equalizer(src)
    print("\nУсі завдання ЛР 13 виконано.")