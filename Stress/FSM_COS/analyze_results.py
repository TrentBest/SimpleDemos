#!/usr/bin/env python3
"""Summarize paired direct-FSM_API vs FSM_COS stress CSV samples.

All raw rows are preserved. This script reports paired effects and an illustrative
startup break-even estimate; it does not declare a universal performance result.
"""
import csv
import math
import statistics
import sys
from pathlib import Path

source = Path(sys.argv[1] if len(sys.argv) > 1 else "fsm_cos_stress_comparison.csv")
report = Path(sys.argv[2] if len(sys.argv) > 2 else "fsm_cos_paired_analysis.md")
paired_csv = report.with_suffix(".csv")

with source.open(newline="", encoding="utf-8-sig") as stream:
    rows = list(csv.DictReader(stream))
if not rows:
    raise SystemExit(f"No benchmark rows found in {source}")

by_path_trial = {}
for row in rows:
    key = (row["Path"], int(row["Trial"]))
    if key in by_path_trial:
        raise SystemExit(f"Duplicate benchmark row: {key}")
    by_path_trial[key] = row

trials = sorted({trial for path, trial in by_path_trial})
pairs = []
for trial in trials:
    direct = by_path_trial.get(("FSM_API", trial))
    composed = by_path_trial.get(("FSM_COS", trial))
    if direct is None or composed is None:
        raise SystemExit(f"Trial {trial} is missing a direct or FSM_COS row")
    for field in ("Workers", "WorkerUpdatesPerTick", "PayloadOpsPerTick", "MeasuredTicks"):
        if direct[field] != composed[field]:
            raise SystemExit(f"Trial {trial} work mismatch in {field}: {direct[field]} != {composed[field]}")
    direct_tick = float(direct["MeanTickMs"])
    composed_tick = float(composed["MeanTickMs"])
    direct_setup = float(direct["SetupOrCompositionMs"])
    composed_setup = float(composed["SetupOrCompositionMs"])
    delta_tick = direct_tick - composed_tick
    improvement = (delta_tick / direct_tick * 100.0) if direct_tick > 0 else math.nan
    if delta_tick > 0:
        break_even_text = f"{max(0.0, composed_setup - direct_setup) / delta_tick:.2f}"
    else:
        break_even_text = "no steady-state break-even (composed tick is not faster)"
    pairs.append({
        "Trial": trial,
        "Workers": direct["Workers"],
        "DirectSetupMs": f"{direct_setup:.6f}",
        "ComposedSetupMs": f"{composed_setup:.6f}",
        "DirectMeanTickMs": f"{direct_tick:.6f}",
        "ComposedMeanTickMs": f"{composed_tick:.6f}",
        "PairedTickImprovementPercent": f"{improvement:.3f}",
        "ExtraSetupMs": f"{composed_setup - direct_setup:.6f}",
        "BreakEvenTicks": break_even_text,
        "WorkerUpdatesPerTick": direct["WorkerUpdatesPerTick"],
        "PayloadOpsPerTick": direct["PayloadOpsPerTick"],
    })

with paired_csv.open("w", newline="", encoding="utf-8") as stream:
    writer = csv.DictWriter(stream, fieldnames=list(pairs[0]))
    writer.writeheader()
    writer.writerows(pairs)

improvements = [float(p["PairedTickImprovementPercent"]) for p in pairs]
finite_break_even = [float(p["BreakEvenTicks"]) for p in pairs
                     if p["BreakEvenTicks"] != "no steady-state break-even (composed tick is not faster)"]
lines = [
    "# Paired FSM_API / FSM_COS stress analysis",
    "",
    f"- Raw input: \`{source.name}\`",
    f"- Paired trials: {len(pairs)}",
    f"- Median paired steady-state tick-time improvement: **{statistics.median(improvements):.2f}%** (positive means FSM_COS tick was faster; negative means it was slower).",
    f"- Paired improvement range: {min(improvements):.2f}% to {max(improvements):.2f}%.",
    f"- Trials with a faster FSM_COS tick: {sum(x > 0 for x in improvements)}/{len(improvements)}.",
    f"- Trials with a slower FSM_COS tick: {sum(x < 0 for x in improvements)}/{len(improvements)}.",
]
if finite_break_even:
    lines.append(f"- Median modeled break-even among faster-tick pairs: {statistics.median(finite_break_even):.2f} ticks.")
else:
    lines.append("- Modeled break-even: no paired trial showed a faster FSM_COS steady-state tick.")
lines += [
    "",
    "## Paired rows",
    "",
    "| Trial | Workers | Direct tick (ms) | FSM_COS tick (ms) | Paired improvement | Extra setup (ms) | Modeled break-even |",
    "|---:|---:|---:|---:|---:|---:|---:|",
]
for p in pairs:
    lines.append(f"| {p['Trial']} | {p['Workers']} | {p['DirectMeanTickMs']} | {p['ComposedMeanTickMs']} | {p['PairedTickImprovementPercent']}% | {p['ExtraSetupMs']} | {p['BreakEvenTicks']} |")
lines += [
    "",
    "## Interpretation guardrails",
    "",
    "- The median is calculated from paired trial percentages, not from a ratio of aggregate medians.",
    "- Break-even is a simple estimate using measured setup delta divided by measured per-tick savings. It is undefined when the composed tick is not faster and does not model variance or later lifecycle costs.",
    "- All raw trial rows remain in the source CSV. Do not remove outliers without a documented, predeclared reason.",
    "- One hosted run is exploratory evidence, not a general performance claim. Repeat across independent workflow runs and confirm locally.",
    "",
    f"Paired CSV: \`{paired_csv.name}\`",
]
report.write_text("\n".join(lines) + "\n", encoding="utf-8")
print("\n".join(lines[:9]))
print(f"Wrote paired analysis: {report}")
print(f"Wrote paired rows: {paired_csv}")
