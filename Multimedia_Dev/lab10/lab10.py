BPP = {
    'RGBA32': 32,
    'ASTC 4x4': 8,
    'ASTC 6x6': 128 / 36,
    'ASTC 8x8': 2,
    'ETC2 RGB': 4,
    'ETC2 RGBA': 8
}

def mem(side, fmt, mips=True):
    b = side * side * BPP[fmt] / 8
    if mips:
        b *= 4 / 3
    return b / (2 ** 20)

initial_scene = [
    ('Ground_Albedo', 2048),
    ('Ground_Normal', 2048),
    ('Building_Albedo', 2048),
    ('Building_Normal', 2048),
    ('Props_Albedo', 1024),
    ('Props_Normal', 1024),
    ('Skybox', 2048),
    ('Character_Albedo', 1024),
    ('Character_Normal', 1024),
    ('UI_Atlas', 2048),
    ('Decals', 1024),
    ('Noise', 512)
]

optimized_plan = [
    ('Ground_Albedo', 2048, 2048, 'ASTC 6x6'),
    ('Ground_Normal', 2048, 1024, 'ASTC 4x4'),
    ('Building_Albedo', 2048, 2048, 'ASTC 6x6'),
    ('Building_Normal', 2048, 1024, 'ASTC 4x4'),
    ('Props_Albedo', 1024, 1024, 'ASTC 6x6'),
    ('Props_Normal', 1024, 1024, 'ASTC 4x4'),
    ('Skybox', 2048, 2048, 'ASTC 6x6'),
    ('Character_Albedo', 1024, 1024, 'ASTC 6x6'),
    ('Character_Normal', 1024, 1024, 'ASTC 4x4'),
    ('UI_Atlas', 2048, 2048, 'ASTC 4x4'),
    ('Decals', 1024, 1024, 'ASTC 6x6'),
    ('Noise', 512, 256, 'ASTC 8x8')
]

etc2_plan = [
    ('Ground_Albedo', 2048, 2048, 'ETC2 RGB'),
    ('Ground_Normal', 2048, 1024, 'ETC2 RGBA'),
    ('Building_Albedo', 2048, 2048, 'ETC2 RGB'),
    ('Building_Normal', 2048, 1024, 'ETC2 RGBA'),
    ('Props_Albedo', 1024, 1024, 'ETC2 RGB'),
    ('Props_Normal', 1024, 1024, 'ETC2 RGBA'),
    ('Skybox', 2048, 1024, 'ETC2 RGB'),
    ('Character_Albedo', 1024, 1024, 'ETC2 RGB'),
    ('Character_Normal', 1024, 1024, 'ETC2 RGBA'),
    ('UI_Atlas', 2048, 2048, 'ETC2 RGBA'),
    ('Decals', 1024, 1024, 'ETC2 RGBA'),
    ('Noise', 512, 256, 'ETC2 RGB')
]

def run_calculator():
    total_before = sum(mem(s, 'RGBA32') for _, s in initial_scene)
    print(f"Початковий бюджет (RGBA32): {total_before:.2f} МБ\n")

    print(f"{'Текстура':<18} | {'Було (МБ)':>9} | {'План':<18} | {'Стало (МБ)':>10} | {'Економія':>9}")
    print("-" * 76)
    total_after = 0.0
    for name, orig_s, new_s, fmt in optimized_plan:
        m_before = mem(orig_s, 'RGBA32')
        m_after = mem(new_s, fmt)
        total_after += m_after
        plan_str = f"{new_s} ({fmt})"
        print(f"{name:<18} | {m_before:>9.2f} | {plan_str:<18} | {m_after:>10.2f} | {m_before - m_after:>8.2f}")

    print("-" * 76)
    print(f"Разом за планом ASTC: {total_after:.2f} МБ (ліміт 24.00 МБ, запас {24.0 - total_after:.2f} МБ)\n")

    total_etc2 = sum(mem(new_s, fmt) for _, _, new_s, fmt in etc2_plan)
    print(f"План для пристроїв без ASTC (ETC2): {total_etc2:.2f} МБ")

if __name__ == "__main__":
    run_calculator()