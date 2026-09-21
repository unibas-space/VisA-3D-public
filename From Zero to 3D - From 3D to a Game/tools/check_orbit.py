"""Check the ideal numerical reference case; does not run Godot or its collisions.

Run with Python 3: python tools/check_orbit.py
This uses double precision, while Godot's default Vector3 uses single precision.
"""

import json
import math

MU = 96.0
RADIUS = 6.0
CIRCULAR_SPEED = math.sqrt(MU / RADIUS)
PERIOD = 2.0 * math.pi * RADIUS / CIRCULAR_SPEED


def simulate(speed, seconds, step=1.0 / 240.0, gravity=True):
    x, z = RADIUS, 0.0
    vx, vz = 0.0, -speed
    minimum, maximum = RADIUS, RADIUS
    initial_energy = speed * speed / 2.0 - MU / RADIUS
    initial_momentum = x * vz - z * vx
    energy_drift, momentum_drift = 0.0, 0.0
    state = "running"
    elapsed = 0.0
    for _ in range(round(seconds / step)):
        radius = math.hypot(x, z)
        ax, az = (-MU * x / radius ** 3, -MU * z / radius ** 3) if gravity else (0.0, 0.0)
        vx, vz = vx + ax * step, vz + az * step
        x, z = x + vx * step, z + vz * step
        elapsed += step
        radius = math.hypot(x, z)
        minimum, maximum = min(minimum, radius), max(maximum, radius)
        energy = (vx * vx + vz * vz) / 2.0 - MU / radius
        energy_drift = max(energy_drift, abs(energy - initial_energy))
        momentum_drift = max(momentum_drift, abs(x * vz - z * vx - initial_momentum))
        # Endpoint-radius proxy only: actual classroom contact uses Godot's shape sweep.
        if radius <= 2.65:
            state = "contact_proxy"
            break
        if radius > 10.0:
            state = "outside_demo_area"
            break
    return {
        "initial_speed": speed,
        "state": state,
        "elapsed_seconds": elapsed,
        "min_radius": minimum,
        "max_radius": maximum,
        "max_absolute_energy_drift": energy_drift,
        "max_absolute_angular_momentum_drift": momentum_drift,
        "final_position_xz": [x, z],
    }


circle = simulate(4.0, 10.0 * PERIOD)
ellipse = simulate(3.5, 30.0)
fall = simulate(0.0, 10.0)
fast = simulate(6.0, 10.0)
straight = simulate(4.0, 1.0, gravity=False)
coarser_circle = simulate(4.0, 10.0 * PERIOD, step=1.0 / 60.0)

assert circle["state"] == "running"
assert 5.98 < circle["min_radius"] < 6.0 < circle["max_radius"] < 6.02
assert circle["max_absolute_energy_drift"] / 8.0 < 0.0001
assert circle["max_absolute_angular_momentum_drift"] < 1e-9
assert ellipse["state"] == "running" and ellipse["min_radius"] > 2.65
assert fall["state"] == "contact_proxy"
assert fast["state"] == "outside_demo_area"
assert abs(straight["final_position_xz"][0] - 6.0) < 1e-10
assert abs(straight["final_position_xz"][1] + 4.0) < 1e-10
assert circle["max_radius"] - circle["min_radius"] < coarser_circle["max_radius"] - coarser_circle["min_radius"]

print(json.dumps({
    "scope": "Independent double-precision numerical reference, not an engine test",
    "circular_speed": CIRCULAR_SPEED,
    "escape_speed": math.sqrt(2.0 * MU / RADIUS),
    "ideal_period_seconds": PERIOD,
    "circle_four_substeps": circle,
    "circle_one_substep": coarser_circle,
    "ellipse": ellipse,
    "fall": fall,
    "fast": fast,
    "straight_without_gravity": straight,
}, indent=2))
