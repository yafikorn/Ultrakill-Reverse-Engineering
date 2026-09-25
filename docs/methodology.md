# Technical Methodology: ULTRAKILL Stamina Trainer

## 1. Static Binary Analysis (dnSpy)
* **Target Assembly:** `ULTRAKILL_Data\Managed\Assembly-CSharp.dll`
* **Decompiler Tool:** dnSpyEx v6.1.8 (64-bit)

### Analysis & Findings:
* Inspecting player movement logic inside the `NewMovement` class revealed the precise internal variable used for dash stamina:
  ```csharp
  public float boostCharge = 300f;