# PACSAutomation — PowerScribe One Demo PACS Simulator

A WinForms automation runner that replicates the key workflows of the
**Demo PACS** client from the PowerScribe One SDK.

---

## Project Structure

```
PACSAutomation/
├── Program.cs                 Entry point
├── PowerScribeAutomation.cs   COM wrapper — all PS API calls live here
├── ScenarioRunner.cs          Scenario definitions + execution engine
├── AutomationForm.cs          Main UI form (logic)
├── AutomationForm.Designer.cs Main UI form (layout)
├── ConfigDialog.cs            Runtime config editor dialog
└── PACSAutomation.csproj      Project file (.NET 4.8 WinForms)
```

---

## Prerequisites

1. **PowerScribe One client** installed on the machine.
2. **COM DLL registered** — the `Commissure.PACSConnector.RadWhereCOM.dll`
   must be on the system. Copy it from the SDK:

   ```
   APIClient_sourcecode\APIClient.RadWhereCOM\Lib\Commissure.PACSConnector.RadWhereCOM.dll
   ```

3. **Visual Studio 2019/2022** with the **.NET Framework 4.8** workload.

---

## Setup

1. **Add to the existing SDK solution** (recommended):

   ```
   File → Add → Existing Project → PACSAutomation.csproj
   ```

   The `HintPath` in the project file already points to the COM DLL relative
   to the SDK solution root.

   *Or* open `PACSAutomation.csproj` as a standalone project and fix the
   `HintPath` to point at your copy of `Commissure.PACSConnector.RadWhereCOM.dll`.

2. Build in **Debug** or **Release**.

---

## Usage

### Step-by-step

| Step | Button | What it does |
|------|--------|-------------|
| 1 | **Connect** | Calls `RadWhereCOM.Start()` — launches PS client and blocks until ready |
| 2 | **Login** | Calls `LoginEx(username, password)` |
| 3 | ⚙ **Config** | Opens the config dialog — set accession numbers and site name |
| 4 | **▶ Run Checked** | Runs all ticked scenarios in order |
| 5 | **▶ Run All** | Runs every scenario |
| 6 | **▶ Run Row** | Runs the currently highlighted scenario only |
| 7 | **Logout** | Calls `LogoutEx()` |
| 8 | **Terminate** | Calls `Terminate()` — shuts down the PS client |

### Configuration (⚙ Config dialog)

| Field | Purpose |
|-------|---------|
| Username / Password | PS credentials |
| Site Name | Leave blank for system-wide search across all sites |
| Accession — New | A fresh accession (no existing report). Used by S01, S04, S05, S06 |
| Accession — Existing Draft | An existing report in Draft state. Used by S02, S07, S08, S10 |
| Accession — PendingSignature | A report awaiting final signature. Used by S03 |
| Accession — Discard | Used by the cancel/discard scenario (S05) |
| Accession — Extra | Secondary accession for associate/dissociate tests (S06, S10) |
| Accession — Multi | Two comma-separated accessions for multi-order test (S09) |
| Patient MRN | Optional; leave blank to skip scenario S07 |

---

## Scenarios

| ID  | Name | Key API Calls |
|-----|------|---------------|
| S01 | Open New Draft → Close Without Signing | `OpenReport`, `CloseReport(sign=false)` |
| S02 | Open Existing Report → Sign as Preliminary | `OpenReport`, `CloseReport(sign=true, prelim=true)` |
| S03 | Open PendingSignature Report → Sign as Final | `OpenReport`, `CloseReport(sign=true, prelim=false)` |
| S04 | Open → Save → Close | `OpenReport`, `SaveReport`, `CloseReport` |
| S05 | Open → Cancel / Discard | `OpenReport`, `CancelReport(discard=false)` |
| S06 | Open → Associate Extra Order → Dissociate → Close | `OpenReport`, `AssociateOrders`, `DissociateOrders`, `CloseReport` |
| S07 | Open with MRN (OpenReportEx) | `OpenReportEx(site, acc, mrn)`, `CloseReport` |
| S08 | Preview Orders (no edit) | `PreviewOrders` |
| S09 | Open Multi-Accession Report | `OpenReport("ACC001,ACC002")`, `CloseReport` |
| S10 | AssociateOrdersEx from Explorer | `AssociateOrdersEx(site, current, newList)` |

---

## Key API Notes (from COM Spec)

- **`Start(ref bool started)`** — synchronous; blocks until PS is ready.
- **`Launch(user, pwd, server)`** — asynchronous; `Launched` event fires when ready.
- **`LoginEx(user, pwd)`** — returns `true` if login completes immediately.
  `UserLoggedIn` event fires when extra setup steps complete.
- **`OpenReport(site, acc)`** — must be called from **Explorer** screen.
  `""` for site = all sites; `null` = current Explorer site filter.
- **`CloseReport(sign, preliminary)`** — `sign=true` invokes the Sign action.
  `preliminary=true` → PendingSignature; `false` → Final (attending only).
- **`CancelReport(discard)`** — `discard=false` closes without saving;
  `discard=true` permanently deletes the report.
- **`AssociateOrders(acc)`** — must have a report open (Report screen).
- **`AssociateOrdersEx(site, current, newList)`** — runs from Explorer screen;
  `newList` is the **authoritative** list; extras are dissociated.
- **`Terminate()`** — will prompt if a report is open or modified.

---

## Events Captured

The Event Log panel (dark terminal area) shows all COM events in real time:

- `UserLoggedIn` / `UserLoggedOut`
- `ReportOpened` / `ReportClosed` / `ReportClosed3`
- `ReportChanged` / `AccessionNumbersChanged`
- `DictationStarted` / `DictationStopped` / `AudioTranscribed`
- `ErrorOccurred` / `Terminated`

---

## Extending

Add a new scenario by appending to the list in `ScenarioRunner.BuildScenarios()`:

```csharp
new AutomationScenario
{
    Id          = "S11",
    Name        = "Insert AutoText Template",
    Description = "Opens a report and inserts a canned AutoText.",
    AccessionNumbers = cfg.AccessionNew,
    SiteName    = cfg.SiteName,

    Execute = (ps, s) =>
    {
        bool opened = ps.OpenReport(s.SiteName, s.AccessionNumbers);
        if (!opened) return Fail("OpenReport returned false.");

        Thread.Sleep(1500);

        bool inserted = ps.InsertAutoText("NormalChest", replace: false);
        if (!inserted) return Fail("AutoText 'NormalChest' not found.");

        ps.CloseReport(sign: false);
        return Pass("AutoText inserted successfully.");
    }
},
```
