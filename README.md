# 🛠️ ULTRAKILL — Stamina Memory Trainer (Reverse Engineering)

![License](https://img.shields.io/badge/License-MIT-blue.svg)
![Target Engine](https://img.shields.io/badge/Engine-Unity%20Mono-black)
![Platform](https://img.shields.io/badge/Platform-Windows%20x64-0078D6)
![Category](https://img.shields.io/badge/Category-Reverse%20Engineering-red)

An educational reverse engineering project demonstrating static binary inspection and dynamic memory pointer resolution on ULTRAKILL (Unity Engine), accompanied by an external C# CLI trainer to achieve infinite stamina.

---

## ⚠️ Educational Disclaimer & Legal Notice

> **IMPORTANT:** This repository and its contents are created strictly for **educational, academic, and security research purposes only**. 
> 
> * **No Copyright Infringement Intended:** This project does not contain, distribute, or host any copyrighted game assets, proprietary binaries, or cracked executables of *ULTRAKILL*.
> * **Research Scope:** The primary objective is to study Unity Mono runtime architecture, memory allocation behavior, and Windows API interaction.
> * **Single-Player Focus:** *ULTRAKILL* is a single-player offline game. The techniques demonstrated here cannot and should not be applied to online competitive environments or multi-player games.
> 
> The author assumes no responsibility for any misuse or violation of third-party Terms of Service.

---

## 📌 Project Overview

This project demonstrates the practical reverse engineering pipeline for Unity Mono games:
* **Static Binary Decompilation:** Decompiling `Assembly-CSharp.dll` to inspect movement routines and identify internal field names and parameters.
* **Dynamic Memory Inspection & Pointer Scanning:** Tracking floating-point values in RAM and constructing a resilient multi-level pointer chain.
* **External Memory Trainer:** Building an external C# CLI tool using Windows Win32 APIs (`OpenProcess`, `ReadProcessMemory`, `WriteProcessMemory`) to dynamically freeze stamina values.

---

## 🛠️ Tools Used & Technical Rationale

| Tool | Purpose & Justification |
| :--- | :--- |
| **dnSpyEx** | Used for static decompilation. ULTRAKILL uses Unity's Mono framework, allowing `Assembly-CSharp.dll` to be reversed into readable C#. This revealed that stamina is stored as `boostCharge` with a default float value of `300f`. |
| **Cheat Engine 7.x** | Used for runtime dynamic memory analysis. It isolated the `boostCharge` address in memory and resolved a 7-level pointer chain back to `UnityPlayer.dll`. |
| **.NET C# (csc.exe)** | Chosen for building an external user-mode trainer via P/Invoke calls to Windows memory management APIs (`kernel32.dll`). |

---

## 🧠 Technical Workflow & Architecture

### 1. Static Analysis (`Assembly-CSharp.dll`)
Using `dnSpyEx`, static decompilation of `Assembly-CSharp.dll` revealed the internal movement routines. The stamina mechanic was identified as the field `boostCharge` with a default value of `300f`.

### 2. Dynamic Pointer Chain Resolution
Because memory addresses are allocated dynamically on each game launch, Cheat Engine was used to isolate the volatile IEEE 754 floating-point address of `boostCharge` and resolve a stable 7-level pointer chain anchored back to `UnityPlayer.dll`.

### 3. External Memory Manipulation (P/Invoke)
An external C# CLI application was developed to interface directly with the Windows API via P/Invoke (`kernel32.dll`):
1. `OpenProcess` — Acquires a process handle to *ULTRAKILL* with VM read/write permissions.
2. `ReadProcessMemory` — Traverses the 7-level pointer chain dynamically to locate the runtime `boostCharge` memory address.
3. `WriteProcessMemory` — Continuously overwrites the value to freeze stamina.

---

## 📂 Repository Structure

```text
ULTRAKILL-Reverse-Engineering/
├── README.md
├── .gitignore
├── docs/
│   ├── methodology.md
│   ├── dnspy-boostcharge.png
│   └── stamina-pointer-chain.png
├── cheat-engine/
│   ├── stamina.PTR
│   └── stamina_rescan.PTR
└── src/
    └── UltrakillTrainer/
        ├── Program.cs
        ├── GameObjects.cs
        ├── Memory.cs
        ├── build.bat
        └── UltrakillTrainer.csproj
