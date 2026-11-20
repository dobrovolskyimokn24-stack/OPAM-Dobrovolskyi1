import os
import subprocess
import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import matplotlib.cbook as cbook
from PIL import Image

W, H, FPS = 640, 352, 30
NF = 90
BS = 16
SR = 16

with cbook.get_sample_data("grace_hopper.jpg") as _f:
    _base = np.array(Image.open(_f).convert("RGB").resize((900, 520)))


def gray(f):
    return (np.asarray(f, float) @ np.array([0.299, 0.587, 0.114]))


def scene_static():
    bg = _base[80:80+H, 120:120+W].copy()
    out = []
    for i in range(NF):
        f = bg.copy()
        x = 40 + int(i*5.5)
        y = 150 + int(40*np.sin(i/12))
        f[y:y+46, x:x+46] = [250, 210, 40]
        out.append(f)
    return out


def scene_pan():
    return [_base[80:80+H, 20+int(i*2.6):20+int(i*2.6)+W].copy() for i in range(NF)]


def scene_shake():
    r = np.random.default_rng(5)
    out = []
    for _ in range(NF):
        dy, dx = int(r.integers(-9, 10)), int(r.integers(-9, 10))
        out.append(_base[100+dy:100+dy+H, 130+dx:130+dx+W].copy())
    return out


def scene_many():
    bg = _base[80:80+H, 120:120+W].copy()
    r = np.random.default_rng(11)
    P = [(int(r.integers(0, W-50)), int(r.integers(0, H-50)),
          r.uniform(-6, 6), r.uniform(-6, 6), r.integers(0, 256, 3))
         for _ in range(28)]
    out = []
    for i in range(NF):
        f = bg.copy()
        for x0, y0, vx, vy, c in P:
            x = int(x0 + vx*i) % (W-48)
            y = int(y0 + vy*i) % (H-48)
            f[y:y+44, x:x+44] = c
        out.append(f)
    return out


SCENES = {
    "нерухома камера": scene_static,
    "рівна панорама": scene_pan,
    "тремтлива камера": scene_shake,
    "багато руху": scene_many,
}


def video_to_frames(path, limit=60):
    os.makedirs("_frames", exist_ok=True)
    subprocess.run(
        ["ffmpeg", "-y", "-hide_banner", "-loglevel", "error",
         "-i", path, "-vf", f"scale={W}:{H}", "-frames:v", str(limit),
         "_frames/%04d.png"],
        check=True,
    )
    names = sorted(os.listdir("_frames"))
    return [np.array(Image.open("_frames/" + n).convert("RGB")) for n in names]


def frames_to_video(frames, path, fps=FPS):
    raw = path + ".raw"
    with open(raw, "wb") as f:
        for fr in frames:
            f.write(np.clip(fr, 0, 255).astype(np.uint8).tobytes())
    subprocess.run(
        ["ffmpeg", "-y", "-hide_banner", "-loglevel", "error",
         "-f", "rawvideo", "-pix_fmt", "rgb24", "-s", f"{W}x{H}",
         "-r", str(fps), "-i", raw, "-c:v", "libx264", "-crf", "20",
         "-pix_fmt", "yuv420p", path],
        check=True,
    )
    os.remove(raw)
    print("   збережено:", path)


def entropy(a):
    v = np.clip(np.round(a) + 255, 0, 510).astype(int).ravel()
    c = np.bincount(v, minlength=511).astype(float)
    c = c[c > 0] / len(v)
    return float(-(c * np.log2(c)).sum())


def draw_vectors(frame, vecs, path, scale=2.2, color="#E9873A"):
    fig, ax = plt.subplots(figsize=(W/100, H/100), dpi=130)
    ax.imshow(frame, alpha=0.6)
    for bx, by, dx, dy in vecs:
        if dx or dy:
            ax.arrow(
                bx + BS / 2, by + BS / 2, dx * scale, dy * scale,
                head_width=4, head_length=4, color=color,
                lw=1.0, length_includes_head=True,
            )
    ax.set_xticks([])
    ax.set_yticks([])
    fig.tight_layout(pad=0)
    fig.savefig(path)
    plt.close(fig)
    print("   збережено:", path)


def search_block(cur, prev, by, bx):
    blk = cur[by:by+BS, bx:bx+BS]
    best_dy, best_dx = 0, 0
    best_sad = float(np.abs(blk - prev[by:by+BS, bx:bx+BS]).sum())
    for dy in range(-SR, SR + 1):
        y = by + dy
        if y < 0 or y + BS > H:
            continue
        for dx in range(-SR, SR + 1):
            x = bx + dx
            if x < 0 or x + BS > W:
                continue
            sad = float(np.abs(blk - prev[y:y+BS, x:x+BS]).sum())
            if sad < best_sad:
                best_sad = sad
                best_dy, best_dx = dy, dx
    return best_dy, best_dx, best_sad


def compensate(cur, prev):
    pred = np.zeros_like(cur)
    vecs = []
    sad0 = sad1 = 0.0
    for by in range(0, H - BS + 1, BS):
        for bx in range(0, W - BS + 1, BS):
            dy, dx, s = search_block(cur, prev, by, bx)
            pred[by:by+BS, bx:bx+BS] = prev[by+dy:by+dy+BS, bx+dx:bx+dx+BS]
            vecs.append((bx, by, dx, dy))
            sad0 += float(np.abs(cur[by:by+BS, bx:bx+BS] - prev[by:by+BS, bx:bx+BS]).sum())
            sad1 += s
    return pred, vecs, sad0, sad1


def check(label, ok, detail=""):
    print(f"   [{'OK ' if ok else 'НІ '}] {label}" + ("" if ok or not detail else f"   — {detail}"))
    if not ok:
        print("\n   Зупиняюся. Виправте це місце й запустіть знову.")
        raise SystemExit(1)


def self_test():
    print("\n1. Пошук одного блоку")
    a = gray(scene_pan()[40])
    b = gray(scene_pan()[41])
    dy, dx, s = search_block(b, a, 180, 300)
    check("знайдено горизонтальний зсув", dx != 0, f"({dx}, {dy})")
    check("вертикального зсуву немає", dy == 0, f"dy = {dy}")
    check("SAD після зсуву мала",
          s < np.abs(b[180:196, 300:316] - a[180:196, 300:316]).sum() / 5,
          f"{s:.0f}")

    print("\n2. Нерухома камера: зсув має бути нульовим")
    a = gray(scene_static()[40])
    b = gray(scene_static()[41])
    dy, dx, _ = search_block(b, a, 16, 16)
    check("на нерухомому тлі вектор нульовий", (dy, dx) == (0, 0), f"({dx}, {dy})")

    print("\n3. Компенсація всього кадру")
    a = gray(scene_pan()[40])
    b = gray(scene_pan()[41])
    pred, vecs, s0, s1 = compensate(b, a)
    nb = (H // BS) * (W // BS)
    check("оброблено всі блоки", len(vecs) == nb, f"{len(vecs)} замість {nb}")
    check("компенсація зменшила SAD", s1 < s0 / 10, f"{s0/1e6:.2f} М → {s1/1e6:.2f} М")
    e0, e1 = entropy(b - a), entropy(b - pred)
    check("ентропія різниці впала", e1 < e0 / 5, f"{e0:.2f} → {e1:.2f} біта")
    check("виграш SAD понад 50×", s0 / s1 > 50, f"{s0/s1:.1f}×")

    print("\n   Усі тести пройдено. Далі — завдання 3 і 4.\n")


def run_all():
    print(f"   {'сцена':<20}{'SAD різниці':>14}{'SAD після':>12}{'виграш':>9}"
          f"{'ентропія':>12}{'після':>8}")
    for name, gen in SCENES.items():
        fr = gen()
        a, b = gray(fr[40]), gray(fr[41])
        pred, vecs, s0, s1 = compensate(b, a)
        e0, e1 = entropy(b - a), entropy(b - pred)
        print(f"   {name:<20}{s0/1e6:>12.2f} М{s1/1e6:>11.2f} М{s0/s1:>8.1f}×"
              f"{e0:>11.2f}{e1:>8.2f}")
        draw_vectors(fr[41], vecs, f"vectors_{name.split()[0]}.png")

    print("\n   Різниці до і після, панорама:")
    fr = scene_pan()
    a, b = gray(fr[40]), gray(fr[41])
    pred, _, _, _ = compensate(b, a)
    for arr, nm in ((b - a, "diff_plain.png"), (b - pred, "diff_compensated.png")):
        plt.figure(figsize=(6.4, 3.6), dpi=130)
        plt.imshow(np.clip(np.abs(arr) * 4, 0, 255), cmap="inferno", vmin=0, vmax=255)
        plt.axis("off")
        plt.tight_layout(pad=0)
        plt.savefig(nm)
        plt.close()
        print("   збережено:", nm)


def task4_demo_clip():
    print("\n   Завдання 4: аналіз відеокліпу (плавний рух vs склейка)")
    clip = scene_pan()[:30] + scene_many()[:30]
    for label, idx, fname in (
        ("плавний проїзд (кадри 15→16)", 15, "my_vectors_best.png"),
        ("склейка сцен (кадри 29→30)", 29, "my_vectors_worst.png"),
    ):
        a, b = gray(clip[idx]), gray(clip[idx + 1])
        pred, vecs, s0, s1 = compensate(b, a)
        print(f"   {label}: виграш {s0/s1:.1f}×, ентропія {entropy(b-a):.2f} → {entropy(b-pred):.2f}")
        draw_vectors(clip[idx + 1], vecs, fname)


if __name__ == "__main__":
    try:
        self_test()
    except NotImplementedError as e:
        print(f"\n   Місце {e} ще не зроблено. Знайдіть його у файлі за словом TODO,")
        print("   заповніть за підказкою поруч — і запустіть файл знову.\n")
        raise SystemExit(1)
    run_all()
    task4_demo_clip()